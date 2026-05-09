# Database ed EF Core

## Provider

Usare PostgreSQL con `Npgsql.EntityFrameworkCore.PostgreSQL`.

Pacchetti consigliati:

- `Microsoft.EntityFrameworkCore`
- `Microsoft.EntityFrameworkCore.Design`
- `Npgsql.EntityFrameworkCore.PostgreSQL`
- `EFCore.NamingConventions`

`EFCore.NamingConventions` permette snake_case coerente in PostgreSQL.

## Schema

Schema consigliati:

- `wms`: tabelle dominio.
- `identity`: utenti, ruoli, claim, token.
- `logging`: log Serilog, se si usa stesso database.
- `audit`: audit applicativo, se separato dal dominio.

Per progetto personale puoi usare un solo database PostgreSQL con più schema. Un database separato per logging è più pulito in produzione, ma aggiunge complessità locale.

Scelta consigliata iniziale:

```text
Database: superstorage
Schema dominio: wms
Schema identity: identity
Schema log tecnici: logging
```

## DbContext

Due opzioni:

1. `ApplicationDbContext` unico per dominio + Identity.
2. `ApplicationDbContext` per dominio e `IdentityDbContext` separato.

Consiglio MVP: contesto unico che eredita da `IdentityDbContext<ApplicationUser, ApplicationRole, Guid>`, ma configurazioni separate per cartella/schema. È più semplice per transazioni e seed.

Quando il dominio cresce, si può separare Identity.

## Configurazione EF

Regole:

- configurare tutto con Fluent API;
- niente data annotations nel Domain;
- `ApplyConfigurationsFromAssembly`;
- precisione decimal esplicita;
- max length espliciti;
- indici espliciti;
- delete behavior esplicito;
- concurrency token sugli aggregate critici.

Per PostgreSQL, valutare `xmin` come concurrency token:

```csharp
builder.UseXminAsConcurrencyToken();
```

## Migrations

Le migrazioni sono codice di prodotto:

- nomi descrittivi;
- niente migrazioni monolitiche enormi;
- generare script idempotenti per deploy;
- seed ruoli/permessi controllato;
- evitare dati fittizi dentro migrazioni di produzione.

Comandi:

```bash
dotnet ef migrations add InitialIdentityAndWms \
  --project src/SuperStorage.Infrastructure \
  --startup-project src/SuperStorage.Api \
  --output-dir Persistence/Migrations

dotnet ef database update \
  --project src/SuperStorage.Infrastructure \
  --startup-project src/SuperStorage.Api
```

## Query

Regole performance:

- read model sempre proiettati a DTO;
- `AsNoTracking` per query read-only;
- paginazione obbligatoria;
- niente lazy loading;
- `Include` solo quando serve davvero;
- `AsSplitQuery` per grafi grandi;
- indici creati quando una query entra nel prodotto;
- review del SQL generato per report importanti.
- usare sempre metodi EF Core async quando disponibili: `ToListAsync`, `SingleOrDefaultAsync`, `AnyAsync`, `CountAsync`, `SaveChangesAsync`;
- forwardare sempre il `CancellationToken` ricevuto dal chiamante.

## Repository e DbContext

Per comandi:

- repository specifiche per aggregate root, appoggiate a una base `IRepository<TAggregate, TId>` minimale;
- `SaveChangesAsync` centralizzato in `IUnitOfWork.CommitAsync`;
- apertura, commit e rollback centralizzati in `UnitOfWorkBehavior`;
- domain events raccolti dal DbContext.

Per query:

- query handler read-only con `IQueryDbContext.Query<TEntity>()`, che applica `AsNoTracking` centralmente;
- proiezioni dirette;
- paginazione obbligatoria;
- eventuali query service se i report diventano complessi.

## Transazioni

Serve transazione applicativa per:

- ricezione merce;
- trasferimento stock;
- rettifica;
- conferma ordine;
- allocazione ordine;
- spedizione;
- cancellazione ordine allocato;
- import massivi.

Il `UnitOfWorkBehavior` deve aprire una transazione per i command MediatR:

```csharp
public interface ICommand<out TResponse> : IRequest<TResponse>;
public interface IQuery<out TResponse> : IRequest<TResponse>;
```

Il behavior deve essere vincolato a `where TRequest : ICommand<TResponse>`, così le query restano fuori dalla transazione applicativa.

## Audit

Audit tecnico e audit funzionale sono diversi:

- log tecnico: eccezioni, request, performance, SQL lento.
- audit funzionale: utente X ha rettificato prodotto Y da 10 a 8 con motivo Z.

L'audit funzionale deve essere queryable dall'app.

Campi audit consigliati:

- `id`
- `occurred_at`
- `user_id`
- `user_name`
- `action`
- `entity_type`
- `entity_id`
- `correlation_id`
- `before`
- `after`
- `metadata`

Usare `jsonb` per payload flessibili.

## Reportistica

Per MVP, report su query EF ottimizzate:

- giacenze per magazzino/ubicazione;
- movimenti per periodo;
- prodotti sotto soglia;
- ordini aperti;
- valore stock se si gestiscono costi.

Per volumi elevati:

- viste materializzate;
- read model dedicati;
- indici parziali;
- job di refresh;
- esportazioni asincrone.

## Seed iniziale

Seed consigliato:

- ruoli;
- permessi;
- admin user dev;
- magazzino demo;
- ubicazioni standard: receiving, storage, picking, shipping, quarantine;
- categorie demo opzionali solo in ambiente Development.

I seed dev non devono finire in produzione automaticamente.

## Docker locale

Consiglio un `docker-compose.yml` con:

- PostgreSQL;
- pgAdmin opzionale;
- Seq o Grafana stack opzionale.

Per i test di integrazione, preferire Testcontainers invece di dipendere dal compose locale.
