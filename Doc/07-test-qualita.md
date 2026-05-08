# Test e Qualità

## Strategia

La piramide consigliata:

1. Unit test Domain: veloci, tanti, zero infrastruttura.
2. Unit test Application: handler con dipendenze fake dove utile.
3. Integration test Infrastructure/API: PostgreSQL reale via Testcontainers.
4. Component/UI test mirati per Blazor quando le schermate crescono.
5. End-to-end smoke test solo sui workflow principali.

## Progetti test consigliati

```text
tests/SuperStorage.Domain.Tests
tests/SuperStorage.Application.Tests
tests/SuperStorage.Infrastructure.Tests
tests/SuperStorage.Api.Tests
tests/SuperStorage.Architecture.Tests
```

Per la UI, valutare più avanti:

```text
tests/SuperStorage.Client.Tests
```

## xUnit

Scegliere xUnit v3 se parti ora da zero, oppure xUnit v2 se vuoi massima compatibilità con esempi e tooling consolidato. L'importante è non mischiare runner e pacchetti.

Pacchetti comuni:

- `xunit.v3`
- `FluentAssertions`
- `NSubstitute`
- `Microsoft.NET.Test.Sdk` se si usa runner VSTest

## Test Domain

Copertura prioritaria:

- creazione SKU valida/non valida;
- prodotto attivo/disattivo;
- ubicazione codice univoco;
- movimento stock con quantità positiva;
- trasferimento richiede origine e destinazione diverse;
- ordine non confermabile senza righe;
- ordine non spedibile prima di allocazione/picking;
- stock negativo vietato;
- rettifica crea movimento tracciabile.

## Test Application

Copertura:

- validator FluentValidation;
- command handler crea entità corrette;
- query handler pagina e filtra;
- autorizzazioni applicative;
- transaction behavior chiama commit/rollback;
- domain events dispatchati.

## Test Integration

Usare PostgreSQL reale con Testcontainers:

- migrazioni applicabili;
- configurazioni EF corrette;
- indici/vincoli univoci;
- query principali funzionanti;
- endpoint login/logout/me;
- endpoint CRUD anagrafiche;
- workflow ricezione, trasferimento, spedizione.

Usare `Respawn` per pulire database tra test senza ricreare container ogni volta.

## Test API

Con `WebApplicationFactory`:

- happy path;
- validation problem;
- unauthorized;
- forbidden;
- not found;
- conflict;
- antiforgery su POST/PUT/DELETE;
- cookie auth.

## Test architetturali

Verificare:

- Domain non dipende da Application/Infrastructure/API/Client;
- Application non dipende da Infrastructure/API/Client;
- endpoint non referenziano DbContext;
- Client non referenzia Domain/Infrastructure;
- Infrastructure non referenzia Api/Client.

`NetArchTest.Rules` è sufficiente.

## Qualità codice

Consigli:

- `TreatWarningsAsErrors` almeno nei progetti core quando il progetto si stabilizza;
- nullable abilitato, già presente;
- analyzers .NET;
- editorconfig;
- format automatico;
- CI con restore, build, test.

## Definition of Done per feature

Una feature è completa quando:

- endpoint implementato;
- command/query + validator;
- regole dominio coperte da test;
- migrazione EF se serve;
- UI con loading/error/empty states;
- autorizzazione applicata;
- log/audit per azioni critiche;
- test integration per path principale;
- documentazione aggiornata se introduce concetti nuovi.

## Comandi iniziali

Build:

```bash
dotnet build Dev.slnx
```

Test, quando i progetti saranno creati:

```bash
dotnet test Dev.slnx
```

Format:

```bash
dotnet format Dev.slnx
```

## CI minima

Pipeline:

1. checkout;
2. setup .NET;
3. restore;
4. build release;
5. test;
6. publish API;
7. upload artifacts.

Quando i test integration useranno Testcontainers, il runner CI deve supportare Docker.
