# Blazor WASM Hosted e Pattern BFF

Questo documento spiega come SuperStorage usa ASP.NET Core per servire una Blazor WebAssembly app in modalita' hosted e per ottenere un modello BFF same-origin.

## Obiettivo

Vogliamo che browser, UI Blazor e API lavorino nello stesso origin:

```text
https://superstorage.local
  -> index.html e asset Blazor WASM
  -> /_framework/*
  -> /api/*
```

In questo modo:

- il client Blazor viene scaricato dal backend;
- le chiamate API usano lo stesso dominio;
- il browser puo' inviare il cookie auth HttpOnly sulle chiamate `/api/*`;
- non serve esporre token di autenticazione leggibili da JavaScript;
- CSRF viene gestito con antiforgery token sulle richieste mutative.

## Cosa succede quando apri il sito

Quando l'utente apre SuperStorage, il flusso e':

```text
1. Browser -> SuperStorage.Api: GET /
2. SuperStorage.Api -> Browser: index.html
3. Browser scarica asset statici e file Blazor WASM
4. Browser avvia il runtime WebAssembly
5. SuperStorage.Client gira nel browser
6. SuperStorage.Client chiama SuperStorage.Api su /api/*
```

Il backend non renderizza ogni pagina Blazor. Il backend serve l'app, poi la UI gira nel browser.

Esempio:

```text
Browser -> GET /login
API -> fallback index.html
Blazor WASM si avvia
Router Blazor mostra Login.razor
```

## Middleware coinvolti

Nel progetto API abbiamo:

```csharp
app.UseBlazorFrameworkFiles();
app.UseStaticFiles();
app.MapFallbackToFile("index.html");
```

Questi tre elementi sono cio' che permette a `SuperStorage.Api` di hostare `SuperStorage.Client`.

## UseBlazorFrameworkFiles

`UseBlazorFrameworkFiles()` serve i file necessari al runtime Blazor WebAssembly.

Esempi:

```text
/_framework/blazor.webassembly.js
/_framework/dotnet.wasm
/_framework/*.dll
/_framework/*.json
```

Senza questo middleware, il browser non riuscirebbe a scaricare il runtime .NET WebAssembly e gli assembly compilati del progetto client.

## UseStaticFiles

`UseStaticFiles()` serve i file statici.

Esempi:

```text
/index.html
/css/app.css
/favicon.png
/images/*
```

In una hosted WASM app, questi file sono l'entry point e gli asset della SPA.

## MapFallbackToFile

`MapFallbackToFile("index.html")` gestisce il routing client-side.

Quando l'utente visita direttamente:

```text
/products
/login
/register
```

ASP.NET Core potrebbe non avere endpoint server reali con quei path. Quelle route appartengono al router Blazor nel browser.

Il fallback risponde con:

```text
index.html
```

Poi Blazor si avvia e decide quale componente visualizzare.

Questo e' fondamentale per evitare errori 404 quando l'utente:

- aggiorna la pagina;
- apre un link diretto;
- usa la barra indirizzi;
- torna indietro/avanti con la navigazione browser.

## Perche' questo abilita il BFF

La hosted WASM app diventa BFF quando la stessa applicazione ASP.NET Core:

- serve il client Blazor;
- espone le API;
- gestisce Identity;
- emette cookie auth HttpOnly;
- applica authorization policy;
- valida antiforgery token.

Il browser vede tutto come un'unica applicazione:

```text
GET  /login
GET  /_framework/blazor.webassembly.js
GET  /api/auth/me
POST /api/auth/login
GET  /api/products
POST /api/products
```

Tutte queste richieste vanno allo stesso origin.

## Differenza rispetto a WASM standalone pubblico

In una WASM standalone pubblica classica:

```text
Browser -> sito statico Blazor
Browser -> API separata
```

Spesso servono:

- CORS;
- gestione token lato client;
- OIDC/JWT nel browser;
- maggiore attenzione a storage e refresh token.

Nel modello BFF di SuperStorage:

```text
Browser -> SuperStorage.Api
SuperStorage.Api -> serve Blazor WASM
SuperStorage.Api -> espone /api/*
SuperStorage.Api -> gestisce cookie auth e policy
```

Il client Blazor non possiede il token di autenticazione. Il cookie auth resta gestito dal browser e non e' leggibile da JavaScript.

## Cookie auth e CSRF nel BFF

Con il BFF usiamo cookie auth:

```text
__Host-SuperStorage.Auth
```

Il cookie risponde alla domanda:

```text
Chi e' l'utente?
```

Il token antiforgery risponde alla domanda:

```text
Questa richiesta mutativa arriva davvero dalla nostra app?
```

Questo e' necessario perche' il browser invia i cookie automaticamente. Per `POST`, `PUT`, `PATCH` e `DELETE`, il client aggiunge anche:

```http
X-CSRF-TOKEN: ...
```

Nel nostro progetto questa parte client vive in:

```text
src/SuperStorage.Client/Services/ApiClients/ApiHttpMessageHandler.cs
```

La validazione server vive negli endpoint mutativi e nel middleware antiforgery:

```csharp
app.UseAntiforgery();
```

## Ordine pipeline consigliato

Per SuperStorage l'ordine della pipeline deve restare intenzionale:

```text
ExceptionHandler
-> HTTPS
-> Blazor framework files
-> Static files
-> Authentication
-> Authorization
-> Antiforgery
-> API endpoints
-> SPA fallback
```

Nel progetto:

```text
src/SuperStorage.Api/Program.cs
```

L'idea e':

- prima gestiamo errori e HTTPS;
- poi serviamo gli asset della client app;
- poi abilitiamo auth e autorizzazione;
- poi validiamo antiforgery sugli endpoint che lo richiedono;
- infine mappiamo API e fallback SPA.

## Regola pratica

SuperStorage e' un BFF perche':

- `SuperStorage.Api` hosta `SuperStorage.Client`;
- Blazor WASM gira nel browser dopo il download;
- le API sono chiamate same-origin;
- Identity usa cookie HttpOnly;
- le richieste mutative includono antiforgery token;
- il server resta l'autorita' per autenticazione e autorizzazione.

Quindi il frontend e' ricco e interattivo come una SPA, ma la sicurezza resta centrata sul backend.
