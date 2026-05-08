# Librerie e Pattern Consigliati

## Librerie richieste

### MediatR

Uso:

- command/query;
- notification domain events;
- pipeline behavior.

Pacchetto:

- `MediatR`

### FluentValidation

Uso:

- validatori per command/query;
- pipeline behavior unico;
- validazione input lato Application.

Pacchetti:

- `FluentValidation`
- `FluentValidation.DependencyInjectionExtensions`

### EF Core PostgreSQL

Uso:

- persistenza dominio;
- Identity;
- migrazioni;
- transazioni.

Pacchetti:

- `Microsoft.EntityFrameworkCore`
- `Microsoft.EntityFrameworkCore.Design`
- `Npgsql.EntityFrameworkCore.PostgreSQL`
- `EFCore.NamingConventions`

### MudBlazor

Già presente. Usarlo come design system principale.

### Serilog

Uso:

- log strutturati;
- request logging;
- errori;
- performance;
- sink database o Seq.

Pacchetti:

- `Serilog.AspNetCore`
- `Serilog.Settings.Configuration`
- `Serilog.Sinks.Console`
- `Serilog.Sinks.Async`
- `Serilog.Sinks.PostgreSQL` oppure `Serilog.Sinks.Seq`

Consiglio pratico: usare Console + Seq in sviluppo, PostgreSQL schema `logging` se vuoi tenere tutto nel database. Per produzione vera, meglio sink separato dal database transazionale.

## Librerie consigliate

### Test

- `xunit.v3` o `xunit` v2, scegliendone uno e mantenendolo coerente.
- `FluentAssertions`
- `NSubstitute` oppure `Moq`
- `Microsoft.AspNetCore.Mvc.Testing`
- `Testcontainers.PostgreSql`
- `Respawn` per reset database test.

### Architettura

- `NetArchTest.Rules` per verificare dipendenze tra progetti.
- `Ardalis.GuardClauses` per guard clauses semplici.
- `ErrorOr` o `Ardalis.Result` per risultati applicativi.

### API

- `Microsoft.AspNetCore.OpenApi` già presente.
- `Scalar.AspNetCore` o Swagger UI per esplorazione API.

### Observability

- `OpenTelemetry.Extensions.Hosting`
- `OpenTelemetry.Instrumentation.AspNetCore`
- `OpenTelemetry.Instrumentation.Http`
- `OpenTelemetry.Instrumentation.EntityFrameworkCore`
- exporter OTLP se userai Grafana/Tempo/Jaeger.

### Background jobs

Per MVP:

- `BackgroundService` nativo per outbox processor.

Futuro:

- `Hangfire` se servono dashboard job, retry evoluti e schedulazioni.

### CSV/Excel

Per import/export futuri:

- `CsvHelper`
- `ClosedXML`

## Pattern approvati

### Clean Architecture

Mantenerla con test architetturali:

- Domain non dipende da nessuno.
- Application non dipende da Infrastructure/API/Client.
- Infrastructure non dipende da API/Client.
- Client non dipende da Infrastructure/Domain.

### DDD pragmatico

Usarlo per:

- stock;
- ordini;
- movimenti;
- prenotazioni;
- workflow.

Non forzarlo su CRUD semplici come categorie se non hanno regole.

### CQRS leggero

Separare command e query, ma senza introdurre due database all'inizio.

### Repository per aggregate

Repository specifici, non generici.

### Unit of Work

EF Core `DbContext` è già Unit of Work. Non creare un wrapper superfluo salvo per esporre `SaveChangesAsync` e transazioni in Application.

### Specification pattern

Utile più avanti per query composabili. Non necessario nella prima implementazione se i query handler restano leggibili.

### Result pattern

Consigliato per errori applicativi attesi:

- not found;
- conflict;
- validation;
- forbidden.

### Domain events

Utile per separare effetti secondari:

- audit;
- aggiornamento read model;
- outbox.

### Outbox pattern

Da introdurre quando esistono eventi da processare fuori dalla transazione o background job affidabili. Consiglio di predisporre presto la tabella, anche se il primo processor è semplice.

### Typed HTTP Client

Nel Client:

- un client per bounded context;
- gestione centralizzata errori;
- antiforgery handler;
- no chiamate `HttpClient` sparse nelle pagine.

## Pattern da evitare

- Generic repository universale.
- Entità EF esposte come DTO.
- Logica business nei componenti Blazor.
- Validazioni duplicate solo in UI.
- `SaveChangesAsync` dentro loop.
- Lazy loading EF Core.
- JWT in localStorage.
- Endpoint API che accedono direttamente a Infrastructure.
- Query senza paginazione.
- Eccezioni usate come normale flusso applicativo.

## Pacchetti da aggiungere in ordine

1. MediatR + FluentValidation in Application.
2. EF Core + Npgsql + naming conventions in Infrastructure.
3. Identity in Infrastructure/API.
4. Serilog in API.
5. Test packages e Testcontainers.
6. OpenTelemetry/health checks.
7. Scalar/Swagger UI per dev API.
