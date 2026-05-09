# Login, Cookie Auth e Protezione CSRF

Questo documento spiega il workflow di autenticazione implementato in SuperStorage con ASP.NET Core Identity, cookie auth, BFF same-origin e antiforgery token.

## Concetti base

Nel nostro modello BFF ci sono due elementi distinti:

- cookie di autenticazione;
- token antiforgery, chiamato anche CSRF token.

Il cookie di autenticazione risponde alla domanda:

```text
Chi sei?
```

Il token antiforgery risponde alla domanda:

```text
Questa azione e' stata davvero iniziata dalla nostra app?
```

Servono entrambi perche' il browser invia i cookie automaticamente, anche quando una richiesta viene innescata da una pagina esterna. Il token antiforgery invece viene letto dalla nostra app e inviato in un header custom, quindi dimostra che la richiesta mutativa parte dal nostro client legittimo.

## Cookie di autenticazione

Quando un utente fa login, ASP.NET Core Identity crea un cookie:

```text
__Host-SuperStorage.Auth
```

La configurazione principale e':

- `HttpOnly`: JavaScript non puo' leggere il cookie;
- `Secure`: il cookie viaggia solo su HTTPS;
- `SameSite=Strict`: il browser limita l'invio cross-site;
- `Path=/`: richiesto dal prefisso `__Host-`;
- redirect API disabilitati: `/api/*` riceve `401` o `403`, non HTML.

Il cookie permette al server di ricostruire l'identita' dell'utente:

```text
Cookie valido -> utente autenticato -> ruoli/policy applicabili
```

Nel client Blazor non leggiamo mai questo cookie. Il cookie resta opaco e viene gestito dal browser.

## Cos'e' CSRF

CSRF significa Cross-Site Request Forgery.

Esempio:

1. Sei loggato in SuperStorage.
2. Il browser ha un cookie auth valido.
3. Visiti un sito malevolo.
4. Quel sito prova a far partire una richiesta verso SuperStorage.
5. Il browser potrebbe allegare automaticamente il cookie auth.

Senza antiforgery, il server potrebbe vedere una richiesta autenticata anche se l'utente non l'ha avviata dalla UI SuperStorage.

Il token antiforgery evita questo problema. Le richieste mutative devono includere:

```http
Cookie: __Host-SuperStorage.Auth=...
X-CSRF-TOKEN: ...
```

Un sito esterno non puo' conoscere il token generato dalla nostra API e quindi non puo' produrre una richiesta mutativa valida.

## Endpoint coinvolti

Gli endpoint Auth sono definiti in:

```text
src/SuperStorage.Api/Endpoints/AuthEndpoints.cs
```

Endpoint principali:

- `GET /api/auth/me`
- `GET /api/auth/csrf`
- `POST /api/auth/register`
- `POST /api/auth/login`
- `POST /api/auth/logout`

Gli endpoint mutativi validano antiforgery:

- register;
- login;
- logout;
- products POST.

## Workflow login

### 1. Il client apre la pagina Login

La pagina Blazor `/login` raccoglie:

- email;
- password;
- remember me.

Il client non legge cookie e non salva token di autenticazione.

### 2. Il client prepara la POST

La chiamata passa da `ApiHttpMessageHandler`.

Il handler:

1. imposta `BrowserRequestCredentials.Include`;
2. vede che `POST /api/auth/login` e' una richiesta mutativa;
3. se non ha un token antiforgery valido in memoria, chiama `GET /api/auth/csrf`;
4. aggiunge il token come header `X-CSRF-TOKEN`.

La richiesta finale e' simile a:

```http
POST /api/auth/login
X-CSRF-TOKEN: token-generato-dal-server
```

Se esistono gia' cookie same-origin, il browser li invia automaticamente.

### 3. Il server valida antiforgery

L'endpoint login esegue:

```csharp
await antiforgery.ValidateRequestAsync(httpContext);
```

Se il token manca o non e' valido, la richiesta viene rifiutata.

### 4. Il server valida le credenziali

L'endpoint usa:

```csharp
UserManager<ApplicationUser>
SignInManager<ApplicationUser>
```

Passaggi:

1. cerca l'utente per email;
2. verifica che l'utente sia attivo;
3. chiama `PasswordSignInAsync`;
4. rispetta il lockout configurato;
5. aggiorna `LastLoginAt`;
6. restituisce `AuthUserResponse`.

### 5. Identity crea il cookie auth

Se il login ha successo, Identity emette il cookie:

```text
__Host-SuperStorage.Auth
```

Da quel momento il browser allega il cookie alle richieste same-origin.

### 6. Il client aggiorna lo stato auth

Il client riceve `AuthUserResponse`:

```json
{
  "isAuthenticated": true,
  "userId": "...",
  "email": "admin@example.com",
  "displayName": "Administrator",
  "roles": ["Administrator"]
}
```

`CookieAuthenticationStateProvider` crea un `ClaimsPrincipal` locale per la UI Blazor. Questo serve solo alla UI per `AuthorizeView`, nav menu e routing. L'autorita' vera resta sempre il server.

## Perche' puliamo il token CSRF dopo login

Il token antiforgery e' legato all'identita' corrente.

Caso problematico:

```text
Token generato da anonimo
Login riuscito come Admin
Logout inviato con token anonimo + cookie Admin
```

Il server rifiuta questa combinazione:

```text
The provided antiforgery token was meant for a different claims-based user than the current user.
```

Per questo `AuthApiClient` chiama:

```csharp
apiHttpMessageHandler.ClearCsrfToken();
```

prima e dopo:

- register;
- login;
- logout.

In questo modo, dopo un cambio di identita', il client richiede un nuovo token legato all'utente corrente.

## Workflow logout

### 1. Il client apre `/logout`

La pagina e' protetta con:

```csharp
[Authorize]
```

### 2. Il client scarta il vecchio token CSRF

Prima della POST, `AuthApiClient.LogoutAsync` chiama:

```csharp
ClearCsrfToken();
```

### 3. Il client richiede un token fresco

`ApiHttpMessageHandler` vede una POST e chiama:

```http
GET /api/auth/csrf
```

Questa volta il browser invia il cookie auth dell'utente loggato. Il server genera quindi un token legato a quell'utente.

### 4. Il client invia logout

La POST contiene:

```http
POST /api/auth/logout
Cookie: __Host-SuperStorage.Auth=...
X-CSRF-TOKEN: token-legato-all-utente-corrente
```

### 5. Il server valida e firma fuori l'utente

L'endpoint:

1. valida antiforgery;
2. chiama `SignInManager.SignOutAsync`;
3. ritorna `204 No Content`.

### 6. Il client aggiorna la UI

`CookieAuthenticationStateProvider` viene notificato con:

```csharp
NotifyUserLoggedOut();
```

La UI torna anonima e reindirizza a `/login`.

## Workflow register

La registrazione pubblica crea sempre utenti con ruolo:

```text
Viewer
```

Passaggi:

1. valida antiforgery;
2. valida campi base;
3. crea `ApplicationUser`;
4. assegna ruolo `Viewer`;
5. se l'assegnazione ruolo fallisce, elimina l'utente appena creato;
6. esegue sign-in automatico;
7. ritorna `AuthUserResponse`.

L'utente Admin non viene creato da register. Viene creato solo dal seeder se sono configurati:

```text
IdentitySeed:AdminEmail
IdentitySeed:AdminPassword
IdentitySeed:AdminDisplayName
```

## Authorization

Le policy sono definite con ruoli:

- `ProductsRead`: `Administrator`, `WarehouseManager`, `Operator`, `Viewer`;
- `ProductsWrite`: `Administrator`, `WarehouseManager`;
- `UsersManage`: `Administrator`.

Esempio:

```text
Viewer -> puo' leggere prodotti
Viewer -> non puo' creare prodotti
Administrator -> puo' leggere e creare prodotti
```

La UI puo' nascondere elementi tramite `AuthorizeView`, ma la protezione reale e' sempre sugli endpoint API.

## Regole operative

- Non salvare token auth in localStorage/sessionStorage.
- Non leggere il cookie auth dal client.
- Usare sempre `HttpOnly` cookie per l'identita'.
- Usare antiforgery su tutte le POST/PUT/PATCH/DELETE same-origin basate su cookie.
- Dopo login/register/logout, scartare sempre il token antiforgery cacheato.
- Forwardare sempre `CancellationToken`.
- Usare `is null` e `is not null` per controlli null.

## Sintesi

Il cookie auth prova l'identita':

```text
Sono Stefano/Admin
```

Il token antiforgery prova l'origine logica dell'azione:

```text
Questa POST arriva dalla UI SuperStorage
```

Il BFF e' sicuro perche' richiede entrambi sulle operazioni mutative.
