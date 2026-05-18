# Progress Tracker

Questo documento tiene traccia dello stato reale del progetto SuperStorage.

La roadmap completa vive in:

```text
Doc/09-piano-sviluppo.md
```

Questo file invece serve per riprendere rapidamente il contesto operativo: cosa e' gia' stato implementato, cosa e' in corso, cosa va validato e quali decisioni sono gia' state prese.

## Come aggiornarlo

Alla fine di ogni feature importante dobbiamo decidere insieme lo stato:

- `Done`: implementata, build verificata, comportamento provato almeno con smoke test.
- `Partial`: implementata in parte, manca UI, test, endpoint o hardening.
- `Needs Fix`: implementata ma con bug noto o scelta da rivedere.
- `Planned`: non ancora iniziata.

Domanda standard da fare a fine feature:

```text
Consideriamo questa feature Done, Partial o Needs Fix?
Manca qualcosa prima di passare alla prossima?
```

## Stato generale

| Area                                | Stato   | Note                                                                                                                     |
| ----------------------------------- | ------- | ------------------------------------------------------------------------------------------------------------------------ |
| Documentazione progetto             | Done    | Struttura `Doc` creata con architettura, sicurezza, UI, test, osservabilita' e roadmap.                                  |
| Clean Architecture                  | Done    | Progetti separati e test architetturali iniziali per dependency boundaries e convenzioni naming presenti.                |
| MediatR                             | Done    | `ICommand`, `IQuery`, handler e pipeline behavior configurati.                                                           |
| Validation                          | Done    | FluentValidation collegato tramite pipeline behavior.                                                                    |
| Unit of Work / Transaction behavior | Done    | I command passano da behavior transazionale tramite unit of work.                                                        |
| EF Core / DbContext                 | Done    | `WmsDbContext` configurato con Identity e schema/naming PascalCase.                                                      |
| Repository base                     | Done    | Repository write model e read repository specifiche con read context no-tracking disponibili.                            |
| Identity / Auth cookie / BFF        | Done    | Identity, cookie HttpOnly, antiforgery, BFF same-origin e UI auth implementati.                                          |
| Authorization policies              | Done    | Policy base per prodotti e gestione utenti definite.                                                                     |
| Product aggregate                   | Done    | Aggregate, value object, category link, audit fields, repository, command/query e API presenti per lo scope corrente.    |
| Product API                         | Done    | Endpoint protetti per lista/dettaglio/create/update/delete presenti. Test integration tracciati nell'area `Tests`.       |
| UI Blazor/MudBlazor                 | Partial | Login/register/logout, navigazione auth, pagine prodotto e pagine categoria presenti. Mancano polish generale e test UI. |
| Logging / Observability             | Planned | Serilog, health checks, correlation id e audit ancora da implementare.                                                   |
| Inventory / Movements               | Planned | Non iniziato.                                                                                                            |
| Customers / Suppliers / Orders      | Planned | Non iniziato.                                                                                                            |
| Reporting                           | Planned | Non iniziato.                                                                                                            |
| Tests                               | Done    | Unit/Application/Architecture test e integration test API obbligatori configurati. CI GitHub Actions aggiunta.           |

## Ledger indicizzato

Usare questi ID quando aggiorniamo il progresso o quando dobbiamo riprendere una feature.

| ID              | Area                                               | Stato   | Fonte principale                                                                                                                                                                                      |
| --------------- | -------------------------------------------------- | ------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `DOCS`          | Documentazione progetto                            | Done    | `Doc/`                                                                                                                                                                                                |
| `ARCH`          | Clean Architecture e convenzioni                   | Done    | `Doc/01-architettura.md`, `tests/SuperStorage.ArchitectureTests`                                                                                                                                      |
| `MEDIATR`       | MediatR, command/query e behavior                  | Done    | `src/SuperStorage.Application`                                                                                                                                                                        |
| `EFCORE`        | DbContext, EF Core, Identity schema                | Done    | `src/SuperStorage.Infrastructure/Persistence`                                                                                                                                                         |
| `REPOSITORY`    | Repository base e query no-tracking                | Done    | `src/SuperStorage.Application/Abstractions/Persistence`, `src/SuperStorage.Infrastructure/Persistence/Repositories`                                                                                   |
| `AUTH-BFF`      | Identity, cookie auth, CSRF, hosted WASM BFF       | Done    | `Doc/Auth/`, `src/SuperStorage.Api/Endpoints/AuthEndpoints.cs`, `src/SuperStorage.Client/Auth`                                                                                                        |
| `PRODUCTS`      | Product aggregate, handlers, repository, API       | Done    | `src/SuperStorage.Domain/Products`, `src/SuperStorage.Application/Features/Products`, `src/SuperStorage.Api/Endpoints/ProductEndpoints.cs`                                                            |
| `PRODUCTS-UI`   | UI gestione prodotti                               | Done    | `src/SuperStorage.Client/Pages/Products`, `src/SuperStorage.Client/Services/ApiClients/ProductsApiClient.cs`                                                                                          |
| `CATEGORIES`    | Category aggregate, handlers, repository, API e UI | Done    | `src/SuperStorage.Domain/Products/Category.cs`, `src/SuperStorage.Application/Features/Categories`, `src/SuperStorage.Api/Endpoints/CategoryEndpoints.cs`, `src/SuperStorage.Client/Pages/Categories` |
| `OBSERVABILITY` | Logging, health checks, audit, diagnostics         | Planned | `Doc/08-osservabilita-deploy.md`                                                                                                                                                                      |
| `INVENTORY`     | Giacenze e movimenti                               | Planned | Da definire                                                                                                                                                                                           |
| `PARTIES`       | Clienti e fornitori                                | Planned | Da definire                                                                                                                                                                                           |
| `ORDERS`        | Ordini e workflow                                  | Planned | Da definire                                                                                                                                                                                           |
| `REPORTING`     | Reportistica e dashboard                           | Planned | Da definire                                                                                                                                                                                           |
| `TESTS`         | Unit, integration e architecture tests             | Done    | `Doc/07-test-qualita.md`, `tests/`                                                                                                                                                                    |

## Decisioni gia' prese

### Hosting e BFF

`SuperStorage.Api` hosta `SuperStorage.Client` come Blazor WebAssembly hosted app.

Decisioni:

- API e client devono lavorare same-origin.
- L'auth usa cookie HttpOnly, non JWT nel browser.
- Le richieste mutative usano antiforgery token.
- `UseBlazorFrameworkFiles`, `UseStaticFiles` e `MapFallbackToFile("index.html")` sono parte del modello hosted WASM/BFF.

Documenti:

```text
Doc/Auth/blazor-wasm-bff-hosting.md
Doc/Auth/login-cookie-csrf-workflow.md
Doc/04-sicurezza-bff-auth.md
```

### Identity e ruoli

Ruoli base:

- `Administrator`
- `WarehouseManager`
- `Operator`
- `Viewer`

Decisioni:

- La registrazione pubblica assegna sempre `Viewer`.
- L'admin viene creato solo tramite seeder configurabile.
- Nessuna password admin hardcoded nel repository.

### MediatR e transazioni

Decisioni:

- Le API chiamano MediatR.
- Le query sono read-only.
- I command modificano stato.
- Il transaction behavior apre/commit/rollback solo per command.
- Gli handler devono forwardare sempre il `CancellationToken`.

### Repository e query

Decisioni:

- Repository write model per aggregate root.
- Read repository specifiche per query, lookup e proiezioni DTO.
- `IReadDbContext` come porta read-only minimale.
- `SuperStorageReadDbContext` come adapter EF Core Infrastructure con `AsNoTracking()`.
- `WmsDbContext` resta focalizzato su mapping EF/Identity, senza wrapper applicativi di query.
- `IQueryable` resta interno al read side Infrastructure e non viene esposto dalle interfacce repository.
- Evitare tracking sulle query MediatR read-only.

### Naming e stile codice

Decisioni:

- Tabelle, colonne e schema database in PascalCase.
- Preferire sempre `is null` e `is not null` per controlli null su oggetti.
- Usare async/await ove possibile.
- Forwardare sempre il `CancellationToken`.
- Commenti nel codice in inglese.
- I test architetturali validano naming MediatR e repository: `Command`, `Query`, `CommandHandler`, `QueryHandler`, repository write/read.

### CI e integration test

Decisioni:

- La CI gira su GitHub Actions per `push` su `main` e `pull_request` verso `main`.
- La build CI usa configurazione `Release`.
- Gli integration test API con PostgreSQL/Testcontainers non devono essere skippati.
- Non usiamo docker-compose per la CI test: Testcontainers gestisce il database PostgreSQL disposable.
- Le migration vengono validate in CI con `dotnet ef migrations list --no-connect`; l'applicazione delle migration viene verificata dagli integration test su database Testcontainers.

## Feature completate

### Bootstrap documentazione

Stato: `Partial`

Completato:

- visione e scope;
- architettura;
- dominio;
- EF Core/database;
- sicurezza BFF/auth;
- UI/UX;
- librerie/pattern;
- test/qualita';
- osservabilita'/deploy;
- piano sviluppo.

Da fare:

- aggiornare progress tracker a fine feature;
- tenere allineato il piano con cio' che viene davvero implementato.

### Auth + Authorization + BFF

Stato: `Done`

Completato:

- Identity user/role con `Guid`;
- cookie auth sicuro;
- antiforgery endpoint e middleware;
- endpoint auth `me`, `csrf`, `register`, `login`, `logout`;
- seeder ruoli/admin;
- UI login/register/logout;
- `CookieAuthenticationStateProvider`;
- `ApiHttpMessageHandler` per cookie credentials e CSRF header;
- fix token CSRF dopo cambio identita';
- protezione Product API con policy.

Da verificare periodicamente:

- HTTPS/cookie secure in ambiente reale;
- data protection keys persistenti in produzione;
- rate limiting login;
- test integration auth.

### Product vertical slice iniziale

Stato: `Done`

Completato:

- aggregate root `Product`;
- value object `Sku`;
- `Code` come stringa semplice validata dal dominio;
- entity/aggregate `Category` con relazione opzionale su Product, senza navigation inversa verso i prodotti;
- campi audit `CreatedAtUtc` e `UpdatedAtUtc`;
- repository prodotto;
- query/command/handler iniziali;
- configurazione EF;
- API products protette;
- delete prodotto con conferma UI;
- migration `AddProductCategoriesAndAuditFields`;
- migration `RemoveProductNameAndUseStringCode`;
- typed client Blazor per prodotti;
- pagine lista, creazione, dettaglio ed edit prodotto.

Da fare:

- valutare soft delete/disattivazione dedicata se vorremo preservare storico operativo;
- migliorare paginazione/filtri se emergeranno esigenze operative;
- test unit e integration tracciati nell'area `Tests`.

### Category vertical slice iniziale

Stato: `Done`

Completato:

- repository categoria;
- command/query/handler per create/update/list/detail;
- endpoint protetti `/api/categories`;
- delete categoria con preview dei primi 5 prodotti collegati;
- rimozione categoria dai prodotti collegati prima del delete;
- policy `CategoriesRead` e `CategoriesWrite`;
- typed client Blazor per categorie;
- pagine lista, creazione, dettaglio ed edit categoria;
- nome categoria modificabile.

Da fare:

- valutare soft delete/disattivazione dedicata se vorremo preservare storico operativo;
- riusare componenti condivisi per form/list quando emergera' duplicazione;
- test unit e integration tracciati nell'area `Tests`.

### UI cleanup iniziale

Stato: `Done`

Completato:

- rimossi `Counter.razor` e `Weather.razor`;
- rimosso `wwwroot/sample-data/weather.json`;
- rimossi link template dal menu;
- sostituita la home template con una dashboard placeholder SuperStorage;
- rimosso CSS isolation template non usato;
- verificata build solution.

## Prossime feature consigliate

1. Verificare il primo run GitHub Actions su PR/main e correggere eventuali problemi specifici del runner.
2. Aggiungere logging/observability minima: Serilog, request logging, health checks.
3. Introdurre `ICurrentUser` e audit base.
4. Implementare Warehouse/Location.
5. Valutare architecture test aggiuntivi per endpoint che non referenziano direttamente EF Core.

## Domande aperte

- Vogliamo estendere gli architecture test agli endpoint API e ai typed client Blazor?
- Il modello ruoli resta role-based per ora, oppure iniziamo presto con permission claims granulari?

## Ultimo checkpoint noto

Data: 2026-05-10

Checkpoint:

- BFF e autenticazione cookie funzionanti.
- Logout corretto dopo fix del CSRF token legato all'identita'.
- Documentazione Auth/BFF aggiornata.
- Product e Category hanno create/list/detail/edit/delete a livello API e UI; prossimo blocco naturale: test vertical slice o osservabilita' minima.
- Read side refactor completato: `IReadDbContext`, `SuperStorageReadDbContext`, `IReadRepository` minimale e read repository specifiche per Product/Category.
- Unit test iniziali completati: Domain e Application con xUnit v3, Moq e Shouldly; 39 test verdi.
- Progetti test presenti sotto solution folder `Tests`.
- Integration test API iniziali aggiunti con `WebApplicationFactory` e `Testcontainers.PostgreSql`: auth/csrf/register/login/logout, authorization products e workflow category-product delete impact.
- Gli integration test API sono obbligatori: localmente falliscono se Docker non e' accessibile.
- Architecture tests aggiunti con `NetArchTest.Rules` e reflection: 16 test verdi per Clean Architecture dependencies, naming MediatR e naming repository.
- CI GitHub Actions aggiunta in `.github/workflows/ci.yml`: restore tools, restore solution, build Release, check EF migrations, test separati per Domain/Application/Architecture/API integration.
- Integration test API verificati con Docker accessibile tramite gruppo `docker`: 8 test verdi, nessuno skipped.
