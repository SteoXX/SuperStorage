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

| Area | Stato | Note |
|---|---|---|
| Documentazione progetto | Partial | Struttura `Doc` creata con architettura, sicurezza, UI, test, osservabilita' e roadmap. |
| Clean Architecture | Partial | Progetti separati gia' presenti e convenzioni iniziali definite. Da completare con test architetturali. |
| MediatR | Done | `ICommand`, `IQuery`, handler e pipeline behavior configurati. |
| Validation | Done | FluentValidation collegato tramite pipeline behavior. |
| Unit of Work / Transaction behavior | Done | I command passano da behavior transazionale tramite unit of work. |
| EF Core / DbContext | Done | `WmsDbContext` configurato con Identity e schema/naming PascalCase. |
| Repository base | Done | Repository write model e read model con query no-tracking disponibili. |
| Identity / Auth cookie / BFF | Done | Identity, cookie HttpOnly, antiforgery, BFF same-origin e UI auth implementati. |
| Authorization policies | Done | Policy base per prodotti e gestione utenti definite. |
| Product aggregate | Partial | Aggregate, value object, repository, command/query, API presenti. Manca UI completa e test. |
| Product API | Partial | Endpoint protetti presenti. Da completare con update/delete, paginazione e test integration. |
| UI Blazor/MudBlazor | Partial | Login/register/logout e navigazione auth presenti. Residui template rimossi. UI gestionale prodotto ancora da implementare. |
| Logging / Observability | Planned | Serilog, health checks, correlation id e audit ancora da implementare. |
| Inventory / Movements | Planned | Non iniziato. |
| Customers / Suppliers / Orders | Planned | Non iniziato. |
| Reporting | Planned | Non iniziato. |
| Tests | Planned | Da aggiungere unit, integration e architecture tests. |

## Ledger indicizzato

Usare questi ID quando aggiorniamo il progresso o quando dobbiamo riprendere una feature.

| ID | Area | Stato | Fonte principale |
|---|---|---|---|
| `DOCS` | Documentazione progetto | Partial | `Doc/` |
| `ARCH` | Clean Architecture e convenzioni | Partial | `Doc/01-architettura.md` |
| `MEDIATR` | MediatR, command/query e behavior | Done | `src/SuperStorage.Application` |
| `EFCORE` | DbContext, EF Core, Identity schema | Done | `src/SuperStorage.Infrastructure/Persistence` |
| `REPOSITORY` | Repository base e query no-tracking | Done | `src/SuperStorage.Application/Abstractions/Persistence`, `src/SuperStorage.Infrastructure/Persistence/Repositories` |
| `AUTH-BFF` | Identity, cookie auth, CSRF, hosted WASM BFF | Done | `Doc/Auth/`, `src/SuperStorage.Api/Endpoints/AuthEndpoints.cs`, `src/SuperStorage.Client/Auth` |
| `PRODUCTS` | Product aggregate, handlers, repository, API | Partial | `src/SuperStorage.Domain/Products`, `src/SuperStorage.Application/Features/Products`, `src/SuperStorage.Api/Endpoints/ProductEndpoints.cs` |
| `PRODUCTS-UI` | UI gestione prodotti | Planned | `src/SuperStorage.Client` |
| `OBSERVABILITY` | Logging, health checks, audit, diagnostics | Planned | `Doc/08-osservabilita-deploy.md` |
| `INVENTORY` | Giacenze e movimenti | Planned | Da definire |
| `PARTIES` | Clienti e fornitori | Planned | Da definire |
| `ORDERS` | Ordini e workflow | Planned | Da definire |
| `REPORTING` | Reportistica e dashboard | Planned | Da definire |
| `TESTS` | Unit, integration e architecture tests | Planned | `Doc/07-test-qualita.md` |

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
- Read repository/query context per letture.
- Query EF Core in lettura con `AsNoTracking()`.
- Evitare tracking sulle query MediatR read-only.

### Naming e stile codice

Decisioni:

- Tabelle, colonne e schema database in PascalCase.
- Preferire sempre `is null` e `is not null` per controlli null su oggetti.
- Usare async/await ove possibile.
- Forwardare sempre il `CancellationToken`.
- Commenti nel codice in inglese.

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

Stato: `Partial`

Completato:

- aggregate root `Product`;
- value object `Sku`;
- repository prodotto;
- query/command/handler iniziali;
- configurazione EF;
- API products protette.

Da fare:

- UI gestione prodotti;
- update prodotto;
- delete/disattivazione prodotto;
- paginazione e filtri;
- test unit e integration.

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

1. Completare Product vertical slice con UI MudBlazor e test.
2. Aggiungere logging/observability minima: Serilog, request logging, health checks.
3. Introdurre `ICurrentUser` e audit base.
4. Implementare Category/Warehouse/Location.
5. Passare a StockBalance e InventoryMovement.

## Domande aperte

- Vogliamo considerare Product API iniziale sufficiente per passare alla UI, o preferiamo completare subito update/delete?
- Vogliamo aggiungere prima test integration per Auth/Product, o andare avanti con UI e poi coprire?
- Il modello ruoli resta role-based per ora, oppure iniziamo presto con permission claims granulari?

## Ultimo checkpoint noto

Data: 2026-05-09

Checkpoint:

- BFF e autenticazione cookie funzionanti.
- Logout corretto dopo fix del CSRF token legato all'identita'.
- Documentazione Auth/BFF aggiornata.
- Prossimo blocco naturale: decidere se chiudere Product vertical slice oppure introdurre osservabilita' minima.
