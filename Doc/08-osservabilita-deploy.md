# Osservabilità, Logging e Deploy

## Logging

Usare Serilog come logging provider principale.

Output consigliati:

- console strutturata in sviluppo/container;
- Seq in sviluppo avanzato;
- PostgreSQL schema `logging` se vuoi consultare log dal DB;
- file solo se necessario per debug locale.

Campi strutturati da includere:

- `CorrelationId`
- `UserId`
- `UserName`
- `RequestPath`
- `RequestMethod`
- `StatusCode`
- `ElapsedMs`
- `SourceContext`
- `Environment`

## Request logging

Abilitare `UseSerilogRequestLogging` con arricchimento:

- correlation id;
- utente;
- endpoint;
- durata;
- status code.

Evitare di loggare:

- password;
- cookie;
- token antiforgery;
- dati personali non necessari;
- payload completi di ordini se non filtrati.

## Health checks

Endpoint:

- `/health/live`: processo attivo;
- `/health/ready`: database raggiungibile, migrazioni ok se controllate;
- `/health`: riepilogo.

Health checks:

- PostgreSQL;
- outbox lag, quando presente;
- spazio disco opzionale;
- endpoint esterni futuri.

## Metrics e tracing

Introdurre OpenTelemetry in modo incrementale.

Metriche utili:

- richieste HTTP per endpoint/status;
- durata richieste;
- query EF lente;
- numero movimenti stock;
- ordini aperti per stato;
- outbox pending/failed;
- login falliti.

Trace:

- request API;
- handler MediatR;
- query EF;
- chiamate HTTP esterne future.

## Audit operativo

Audit obbligatorio su:

- login falliti ripetuti;
- cambio ruoli/permessi;
- creazione/modifica prodotti;
- rettifiche stock;
- trasferimenti;
- conferma/cancellazione/spedizione ordini;
- ricezione merce;
- modifiche a magazzini e ubicazioni.

## Configurazione

Usare options strongly typed:

- `DatabaseOptions`
- `SerilogOptions` via configurazione standard;
- `SecurityOptions`
- `CorsOptions` se necessario;
- `OutboxOptions`

Segreti:

- user-secrets in locale;
- environment variables in container;
- secret manager del provider in produzione.

## Docker e deploy locale

File consigliati:

- `docker-compose.yml` per PostgreSQL + Seq;
- `Dockerfile` per API;
- script `dev-up`/`dev-down` opzionali;
- README con istruzioni.

## Migrazioni in produzione

Per progetto personale si può applicare migrazione all'avvio solo in Development.

In produzione:

- generare script idempotente;
- applicare in step controllato;
- backup prima di migrazioni distruttive.

## Backup

Anche per progetto personale, documentare:

- backup PostgreSQL;
- retention;
- restore testato;
- export report non sostituisce backup.

## Ambienti

Minimo:

- Development;
- Test;
- Production.

Config diverse:

- connection string;
- logging level;
- cookie secure;
- CORS;
- seed demo;
- OpenAPI/Scalar visibile solo dev o protetto.
