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
tests/SuperStorage.Domain.UnitTests
tests/SuperStorage.Application.UnitTests
tests/SuperStorage.Api.IntegrationTests
tests/SuperStorage.ArchitectureTests
```

Per la UI, valutare più avanti:

```text
tests/SuperStorage.Client.Tests
```

I progetti test devono essere aggiunti alla solution sotto il solution folder `Tests`, cosi' restano separati dai progetti applicativi anche in IDE.

## xUnit, Moq e Shouldly

Il progetto usa xUnit v3 per i nuovi test.

Pacchetti comuni:

- `xunit.v3`
- `xunit.runner.visualstudio`
- `xunit.analyzers`
- `Microsoft.NET.Test.Sdk`
- `Moq`
- `Shouldly`

Le versioni sono gestite tramite Central Package Management in `Directory.Packages.props`.

Per gli integration test API sono configurati anche:

- `Microsoft.AspNetCore.Mvc.Testing`;
- `Testcontainers.PostgreSql`.

## Test Domain

Copertura prioritaria:

- creazione SKU valida/non valida;
- prodotto attivo/disattivo;
- normalizzazione e validazione codice prodotto;
- aggiornamento dettagli prodotto;
- rimozione categoria da prodotto;
- creazione e aggiornamento categoria;
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
- command handler delete rimuove entità o ritorna `false` quando non trovata;
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

Gli integration test API attuali usano `WebApplicationFactory` e PostgreSQL reale tramite Testcontainers. Richiedono accesso a Docker e non devono essere skippati: se Docker non e' disponibile o il socket non e' accessibile, i test devono fallire.

Usare `Respawn` piu' avanti per pulire database tra test senza ricreare container ogni volta, quando il numero di integration test crescera'.

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

Il progetto reale e' `tests/SuperStorage.ArchitectureTests`.

Usiamo `NetArchTest.Rules` per le regole di dipendenza tra layer perche' e' leggero, leggibile e pensato per essere integrato in normali unit test. `ArchUnitNET` resta una buona alternativa piu' ricca se in futuro vorremo regole piu' profonde su classi, membri, PlantUML o dependency graph avanzati.

Regole attuali:

- Clean Architecture dependencies tra Domain, Application, Infrastructure, Api, Client e Contracts;
- naming `ICommand` / `IQuery`;
- naming e matching `CommandHandler` / `QueryHandler`;
- handler MediatR internal sealed;
- naming e inheritance repository write/read;
- repository concrete internal sealed.

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

Solo integration test API:

```bash
dotnet test tests/SuperStorage.Api.IntegrationTests/SuperStorage.Api.IntegrationTests.csproj
```

Se gli integration test API falliscono con `DockerUnavailableException`, verificare che Docker sia avviato e che l'utente corrente abbia accesso a `/var/run/docker.sock`.

Format:

```bash
dotnet format Dev.slnx
```

## CI minima

Pipeline GitHub Actions:

1. checkout;
2. setup .NET;
3. cache NuGet packages;
4. restore .NET tools;
5. restore solution;
6. build Release;
7. check EF Core migrations con `dotnet ef migrations list --no-connect`;
8. test Domain, Application, Architecture e API integration in step separati.

La CI gira su `push` verso `main` e su `pull_request` verso `main`. I runner GitHub-hosted Linux hanno Docker disponibile per Testcontainers; non usiamo docker-compose per i test perche' il ciclo di vita PostgreSQL e' gia' gestito dal test harness.
