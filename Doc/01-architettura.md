# Architettura

## Forma consigliata

La soluzione deve restare su Clean Architecture con separazione netta:

```text
Client -> Api -> Application -> Domain
                 Infrastructure -> Application/Domain
Contracts condiviso da Api, Application e Client
```

Regola principale: `Domain` non conosce nessun altro progetto. `Application` conosce `Domain` e `Contracts`. `Infrastructure` implementa porte definite da `Application`. `Api` compone tutto tramite dependency injection.

## Progetti

### SuperStorage.Domain

Contiene:

- aggregate root;
- entity;
- value object;
- enum di dominio;
- domain events;
- eccezioni di dominio;
- interfacce marker minime, se utili.

Non deve contenere:

- EF Core attributes;
- DTO API;
- MediatR;
- FluentValidation;
- logica HTTP;
- servizi infrastrutturali.

### SuperStorage.Application

Contiene use case e contratti applicativi:

- command/query MediatR;
- handler;
- validator FluentValidation;
- interfacce repository;
- interfaccia `IApplicationDbContext` se si decide di usare EF direttamente nei read model;
- pipeline behavior;
- authorization requirements applicativi;
- mapping verso DTO.

Gli handler devono essere piccoli e orientati al caso d'uso. Se una regola è una regola di business, va spostata nel dominio.

### SuperStorage.Infrastructure

Contiene:

- `ApplicationDbContext`;
- configurazioni EF Core;
- implementazioni repository;
- Identity;
- Serilog sink;
- outbox processor;
- clock, current user, file storage, email se servono;
- seed iniziali;
- migrazioni.

### SuperStorage.Api

Contiene:

- Minimal API endpoints raggruppati per feature;
- autenticazione cookie;
- autorizzazione;
- antiforgery;
- problem details;
- health checks;
- OpenAPI;
- rate limiting;
- request logging;
- static hosting del client, se si sceglie hosted WASM/BFF same-origin.

Gli endpoint devono chiamare solo `ISender`/MediatR e non accedere mai direttamente a `DbContext`.

### SuperStorage.Client

Contiene:

- pagine Blazor;
- componenti MudBlazor;
- typed API clients;
- stato UI locale;
- validazione UI ergonomica;
- gestione loading/error/empty states.

Non deve contenere:

- regole di business critiche;
- accesso diretto al database;
- segreti;
- token persistiti in browser storage.

### SuperStorage.Contracts

Contiene:

- request/response DTO;
- paginazione;
- filtri query;
- enum di contratto stabili;
- error DTO se necessari.

Evitare di inserire entità Domain qui.

## CQRS con MediatR

Usare una separazione leggera:

- `Commands`: modificano stato e aprono transazioni.
- `Queries`: read-only, proiettano DTO e usano `AsNoTracking`.

Convenzioni obbligatorie per handler, repository, query service e typed HTTP client:

- usare API asincrone con `async`/`await` ogni volta che esiste una controparte async;
- accettare sempre un `CancellationToken`;
- inoltrare sempre il `CancellationToken` alle chiamate successive: MediatR, EF Core, HTTP client, file/storage, servizi esterni;
- evitare sync-over-async come `.Result`, `.Wait()` o `GetAwaiter().GetResult()`.
- per controlli null su oggetti usare sempre pattern matching `is null` e `is not null`, non `== null` o `!= null`.

Pipeline behavior consigliati, in ordine:

1. `UnhandledExceptionBehavior`
2. `LoggingBehavior`
3. `AuthorizationBehavior`
4. `ValidationBehavior`
5. `IdempotencyBehavior` per comandi sensibili
6. `TransactionBehavior` per comandi
7. `DomainEventDispatchBehavior` o dispatch post-save

## Endpoint pattern

Ogni endpoint dovrebbe avere questa forma:

```csharp
group.MapPost("/", async (CreateProductRequest request, ISender sender, CancellationToken ct) =>
{
    var result = await sender.Send(new CreateProductCommand(request), ct);
    return Results.Created($"/api/products/{result.Id}", result);
})
.RequireAuthorization(Policies.ProductsWrite);
```

L'endpoint:

- fa binding HTTP;
- chiama MediatR;
- traduce il risultato in HTTP;
- non contiene regole di dominio.

## Repository pattern

Decisione consigliata: usare repository per tutte le operazioni di scrittura sugli aggregate root, con un'interfaccia base comune e repository specifici per il dominio. La transazione non deve stare nei repository: viene aperta, committata o annullata da `IUnitOfWork` dentro un pipeline behavior MediatR applicato ai soli command.

Questo modello è adatto al progetto perché:

- mantiene gli handler indipendenti da EF Core;
- impedisce `SaveChangesAsync` sparsi nei repository;
- garantisce una sola transazione per caso d'uso;
- rende naturali rollback, domain events e outbox;
- separa bene comandi transazionali e query read-only.

### Interfaccia base

`IRepository<TAggregate, TId>` deve contenere solo operazioni comuni realmente utili agli aggregate:

```csharp
public interface IRepository<TAggregate, TId>
    where TAggregate : AggregateRoot<TId>
{
    Task<TAggregate?> GetByIdAsync(TId id, CancellationToken ct = default);
    Task AddAsync(TAggregate aggregate, CancellationToken ct = default);
    void Remove(TAggregate aggregate);
}
```

Evitare un `GetAll()` generico non paginato: in un WMS diventa rapidamente pericoloso per performance e memoria. Per liste e report usare query paginabili/proiettate.

### Repository specifici

Ogni aggregate root importante espone una repository specifica che eredita dalla base e aggiunge metodi orientati al dominio:

- `IProductRepository`
- `IWarehouseRepository`
- `IInventoryRepository`
- `IOrderRepository`
- `IPurchaseOrderRepository`

Esempio:

```csharp
public interface IProductRepository : IRepository<Product, ProductId>
{
    Task<Product?> GetBySkuAsync(Sku sku, CancellationToken ct = default);
    Task<bool> ExistsBySkuAsync(Sku sku, CancellationToken ct = default);
}
```

Gli handler usano sempre la repository specifica, non direttamente `IRepository<TAggregate, TId>`, così il linguaggio del dominio resta esplicito.

### Unit of Work e Transaction Behavior

`IUnitOfWork` è l'unico punto autorizzato a gestire transazione e persistenza finale:

```csharp
public interface IUnitOfWork
{
    Task BeginTransactionAsync(CancellationToken ct = default);
    Task CommitAsync(CancellationToken ct = default);
    Task RollbackAsync(CancellationToken ct = default);
}
```

I command implementano un marker MediatR:

```csharp
public interface ICommand<out TResponse> : IRequest<TResponse>;
public interface IQuery<out TResponse> : IRequest<TResponse>;
```

Il behavior transazionale deve essere vincolato ai soli command:

```csharp
public sealed class UnitOfWorkBehavior<TRequest, TResponse>(IUnitOfWork unitOfWork)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ICommand<TResponse>
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        await unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var response = await next(cancellationToken);
            await unitOfWork.CommitAsync(cancellationToken);
            return response;
        }
        catch
        {
            await unitOfWork.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
```

`CommitAsync` deve includere `SaveChangesAsync`, dispatch dei domain events e salvataggio outbox secondo l'ordine scelto nell'Infrastructure.

### Query

Le query non devono aprire transazioni applicative e non devono usare repository pensati per aggregate mutabili. Per liste, lookup e report usare `IQueryDbContext.Query<TEntity>()`, che restituisce sempre una query `AsNoTracking`, con proiezioni DTO e paginazione.

In sintesi:

- command: repository specifiche + `IUnitOfWork` transazionale;
- query: `IQueryDbContext` + read model/proiezioni ottimizzate no-tracking;
- repository: nessun `SaveChanges`, nessun commit, nessun rollback.

## Domain events e outbox

Eventi di dominio utili:

- `ProductCreatedDomainEvent`
- `StockReceivedDomainEvent`
- `StockMovedDomainEvent`
- `StockAdjustedDomainEvent`
- `OrderConfirmedDomainEvent`
- `StockReservedDomainEvent`
- `OrderShippedDomainEvent`

Gli eventi interni possono aggiornare read model o audit. Se in futuro vengono pubblicati fuori dal processo, usare Outbox:

- tabella `outbox_messages`;
- salvataggio nella stessa transazione del caso d'uso;
- background worker che pubblica e marca come processato;
- retry con backoff e dead-letter logico.

## Risultati ed error handling

Consiglio:

- eccezioni di dominio solo per invarianti violate dentro aggregate;
- risultato applicativo per casi attesi come `NotFound`, `Conflict`, `Forbidden`;
- `ProblemDetails` in API;
- validazioni input tramite FluentValidation.

Una libreria utile è `ErrorOr` oppure `Ardalis.Result`. In alternativa si può implementare un piccolo `Result<T>` interno.

## Mapping

Opzioni:

- mapping manuale per i primi use case;
- Mapster se cresce molto il numero di DTO;
- evitare AutoMapper all'inizio, perché può nascondere mapping complessi e query non ottimali.

Per proiezioni EF Core, preferire `Select` espliciti.

## Dependency injection

Ogni progetto dovrebbe esporre un metodo:

- `AddApplication()`
- `AddInfrastructure(configuration, environment)`
- `AddApiServices()`

L'API deve restare il composition root.

## Convenzioni cartelle consigliate

```text
src/SuperStorage.Domain/
  Common/
  Products/
  Warehouses/
  Inventory/
  Orders/
  Purchasing/
  Parties/

src/SuperStorage.Application/
  Common/
    Behaviors/
    Interfaces/
    Security/
  Products/
    Commands/
    Queries/
  Inventory/
  Orders/

src/SuperStorage.Infrastructure/
  Persistence/
    Configurations/
    Migrations/
    Repositories/
  Identity/
  Logging/
  Outbox/

src/SuperStorage.Api/
  Endpoints/
  Middleware/
  Security/
  OpenApi/
```
