# Sicurezza, BFF, Identity e Autorizzazione

## Strategia consigliata

Usare ASP.NET Core Identity con cookie auth e pattern BFF same-origin:

```text
Browser -> SuperStorage.Api/BFF -> Application -> Infrastructure -> PostgreSQL
Browser riceve cookie HttpOnly/Secure/SameSite, non token JWT persistiti.
```

Il client Blazor WebAssembly deve chiamare API same-origin. Se API e Client restano su host diversi in sviluppo, configurare proxy/dev server con attenzione, ma l'obiettivo prodotto dovrebbe essere same-origin.

## Perché cookie BFF

Vantaggi:

- cookie `HttpOnly`: JavaScript non legge credenziali;
- niente access token in localStorage/sessionStorage;
- CSRF mitigabile con antiforgery;
- Identity supporta ruoli, claim, lockout, password policy;
- adatto ad applicazione gestionale interna.

## Hosting consigliato

Con la soluzione attuale ci sono due strade:

### Opzione A - Hosted WASM con API come host

`SuperStorage.Api` serve gli asset del client e gli endpoint API.

Pro:

- BFF same-origin naturale;
- cookie auth semplice;
- deploy singolo.

Contro:

- serve configurare il publish del client dentro API;
- separazione meno netta a runtime.

### Opzione B - Blazor Web App server-side/interattivo

Convertire verso Blazor Web App.

Pro:

- auth cookie ancora più naturale;
- UI più integrata con server;
- meno problemi CORS/CSRF.

Contro:

- cambia il modello progettuale;
- WebAssembly puro perde centralità.

Scelta consigliata per questo progetto: Opzione A, mantenendo il client WASM esistente ma servito dall'API.

Dettaglio operativo del modello hosted WASM/BFF:

```text
Doc/Auth/blazor-wasm-bff-hosting.md
```

## Configurazione cookie

Requisiti:

- `HttpOnly = true`
- `SecurePolicy = Always` in produzione
- `SameSite = Strict` o `Lax` se necessario
- sliding expiration valutata con sessione utente
- login path e access denied path espliciti
- data protection keys persistenti in produzione

## CSRF

Con cookie auth, CSRF è un tema reale.

Pattern consigliato:

- abilitare antiforgery in API;
- endpoint `GET /api/auth/csrf` che emette token;
- typed HTTP client invia token su metodi mutativi;
- endpoint mutativi richiedono antiforgery;
- `SameSite=Lax/Strict` riduce superficie ma non sostituisce token.

## XSS

Mitigazioni:

- non usare `MarkupString` salvo necessità controllata;
- validare input;
- output encoding standard Blazor;
- Content Security Policy;
- niente HTML salvato e renderizzato senza sanitizzazione.

## SQL injection

Mitigazioni:

- EF Core LINQ parametrizzato;
- raw SQL solo con parametri;
- vietare concatenazione stringhe in SQL;
- review dedicata per report custom.

## Authorization

Usare policy basate su permessi:

```text
Role -> Permission claims -> Policy
```

Esempio:

- ruolo `WarehouseManager` contiene `Inventory.Read`, `Inventory.Move`, `Inventory.Adjust`;
- policy `CanAdjustInventory` richiede permission `Inventory.Adjust`;
- endpoint `POST /api/inventory/adjustments` richiede quella policy.

Questo evita di spargere `RequireRole("Admin")` ovunque.

## Identity

Entità consigliate:

- `ApplicationUser : IdentityUser<Guid>`
- `ApplicationRole : IdentityRole<Guid>`

Campi user aggiuntivi:

- `DisplayName`
- `IsActive`
- `LastLoginAt`
- `CreatedAt`
- `UpdatedAt`

Funzioni:

- login/logout/current user;
- gestione utenti;
- gestione ruoli;
- assegnazione permessi;
- reset password in fase futura.

## Endpoint auth minimi

- `POST /api/auth/login`
- `POST /api/auth/logout`
- `GET /api/auth/me`
- `GET /api/auth/csrf`
- `GET /api/users`
- `POST /api/users`
- `PUT /api/users/{id}/roles`
- `GET /api/roles`
- `PUT /api/roles/{id}/permissions`

## Hardening ASP.NET Core

Middleware consigliati:

- exception handler + problem details;
- HTTPS redirection;
- HSTS in produzione;
- security headers;
- authentication;
- authorization;
- antiforgery;
- rate limiter su login;
- request logging Serilog;
- health checks.

In SuperStorage la configurazione dell'exception handler API vive in:

```text
src/SuperStorage.Api/DependencyInjection/ExceptionHandlerExtensions.cs
```

`Program.cs` deve limitarsi a chiamare `UseApiExceptionHandler()` per mantenere pulito il bootstrap della pipeline.

Ordine indicativo:

```text
ExceptionHandler -> HSTS -> HTTPS -> StaticFiles -> Routing
-> Authentication -> Authorization -> Antiforgery -> RateLimiter -> Endpoints
```

## Sessione e logout

Logout deve:

- invalidare cookie;
- aggiornare security stamp se necessario per eventi sensibili;
- non lasciare stato utente client incoerente.

Il client deve gestire `401` e `403` con redirect/feedback coerenti.

## Checklist sicurezza MVP

- password policy configurata;
- lockout login configurato;
- ruoli e permessi seedati;
- cookie secure in produzione;
- antiforgery sui metodi mutativi;
- CSP base;
- CORS disabilitato o strettissimo;
- secrets tramite user-secrets/env vars;
- connection string non committata;
- endpoint admin protetti;
- audit su operazioni critiche.
