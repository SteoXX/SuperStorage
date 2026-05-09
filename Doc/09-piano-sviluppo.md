# Piano di Sviluppo

Questo piano è ordinato per ridurre rischio: prima fondamenta tecniche, poi dominio, poi workflow verticali, poi report e hardening.

Lo stato reale di avanzamento vive in:

```text
Doc/10-progress-tracker.md
```

## Fase 0 - Allineamento iniziale

1. Decidere hosting UI: hosted WASM via API come BFF same-origin.
2. Aggiornare `project-info.md` o README con scelte definitive.
3. Creare `Doc` come fonte viva delle decisioni.
4. Aggiungere `.editorconfig`.
5. Verificare `dotnet build Dev.slnx`.

Deliverable: solution buildabile e convenzioni documentate.

## Fase 1 - Pacchetti e bootstrap architettura

1. Aggiungere pacchetti centralizzati in `Directory.Packages.props`.
2. Configurare `AddApplication()`.
3. Configurare MediatR.
4. Configurare FluentValidation.
5. Implementare pipeline `ValidationBehavior`.
6. Aggiungere skeleton `Result` oppure scegliere libreria.
7. Configurare `AddInfrastructure()`.
8. Configurare `AddApiServices()`.
9. Creare cartelle standard nei progetti.
10. Aggiungere test architetturale base.

Deliverable: architettura pronta per feature verticali.

## Fase 2 - Database, Identity e sicurezza base

1. Aggiungere EF Core PostgreSQL.
2. Creare `ApplicationDbContext`.
3. Configurare Identity con `Guid`.
4. Configurare schema `wms`, `identity`, `logging`.
5. Configurare migrazione iniziale.
6. Configurare cookie auth.
7. Configurare authorization policies.
8. Configurare antiforgery per BFF.
9. Creare endpoint auth: login, logout, me, csrf.
10. Seed ruoli e permessi.
11. Aggiungere rate limit su login.
12. Aggiungere test integration auth.

Deliverable: utenti, ruoli e sessione sicura funzionanti.

## Fase 3 - Logging, error handling e osservabilità minima

1. Configurare Serilog.
2. Configurare request logging.
3. Configurare ProblemDetails.
4. Aggiungere middleware correlation id.
5. Aggiungere health checks.
6. Configurare OpenAPI/Scalar in development.
7. Creare tabella audit applicativo.
8. Implementare servizio `ICurrentUser`.
9. Implementare servizio `IDateTimeProvider`.

Deliverable: app diagnosticabile e pronta per debug serio.

## Fase 4 - Dominio anagrafiche

1. Implementare value object `Sku`, `LocationCode`, `Quantity`.
2. Implementare `Product`.
3. Implementare `Category`.
4. Implementare `Warehouse`.
5. Implementare `Location`.
6. Configurare EF per anagrafiche.
7. Creare migration.
8. Implementare CRUD Products con MediatR.
9. Implementare CRUD Categories.
10. Implementare CRUD Warehouses.
11. Implementare CRUD Locations.
12. Aggiungere UI MudBlazor per anagrafiche.
13. Aggiungere unit/integration test.

Deliverable: anagrafiche WMS gestibili da UI.

## Fase 5 - Inventario e movimenti

1. Implementare `StockBalance`.
2. Implementare `InventoryMovement`.
3. Implementare regole stock negativo.
4. Implementare `ReceiveStockCommand`.
5. Implementare `TransferStockCommand`.
6. Implementare `AdjustStockCommand`.
7. Implementare query giacenze paginata.
8. Implementare query registro movimenti.
9. Aggiungere transazioni sui comandi stock.
10. Aggiungere audit su movimenti.
11. Creare UI Inventario.
12. Creare UI Movimenti.
13. Aggiungere test concorrenza base.

Deliverable: stock reale movimentabile e tracciato.

## Fase 6 - Clienti, fornitori e ordini

1. Implementare `Customer`.
2. Implementare `Supplier`.
3. Implementare CRUD clienti.
4. Implementare CRUD fornitori.
5. Implementare `CustomerOrder`.
6. Implementare righe ordine cliente.
7. Implementare workflow ordine cliente.
8. Implementare `PurchaseOrder`.
9. Implementare righe ordine fornitore.
10. Implementare workflow ordine fornitore.
11. UI clienti/fornitori.
12. UI ordini clienti.
13. UI ordini fornitori.
14. Test workflow stati.

Deliverable: ordini base funzionanti.

## Fase 7 - Allocazione, picking, ricezione e spedizione

1. Implementare `StockReservation`.
2. Implementare allocazione ordine cliente.
3. Implementare rilascio prenotazioni.
4. Implementare picking.
5. Implementare packing minimale.
6. Implementare shipment.
7. Collegare shipment a decremento stock.
8. Implementare ricezione da purchase order.
9. Gestire ricezioni parziali.
10. UI azioni operative ordine.
11. Test end-to-end workflow ordine cliente.
12. Test end-to-end workflow ordine fornitore.

Deliverable: ciclo entrata/uscita completo.

## Fase 8 - Reportistica

1. Report giacenze.
2. Report movimenti per periodo.
3. Report prodotti sotto scorta.
4. Report ordini aperti.
5. Dashboard operativa.
6. Export CSV.
7. Ottimizzazione query e indici.
8. Test integration query report.

Deliverable: visibilità operativa sul magazzino.

## Fase 9 - UI polish e usabilità

1. Layout gestionale definitivo.
2. Navigation menu per ruoli.
3. Loading/error/empty states condivisi.
4. Dialog conferma azioni critiche.
5. Toast coerenti.
6. Tabelle responsive.
7. Filtri persistenti per pagina dove utile.
8. Accessibilità base.
9. Smoke test manuale desktop/tablet/mobile.

Deliverable: UI fluida, coerente e usabile.

## Fase 10 - Hardening tecnico

1. Test architetturali completi.
2. Test integration su tutti i workflow critici.
3. Ottimizzazione query lente.
4. Concurrency token su stock/ordini.
5. Idempotency key su comandi critici.
6. Outbox per eventi critici.
7. Background worker outbox.
8. Backup/restore documentato.
9. Dockerfile e compose.
10. CI completa.

Deliverable: prodotto robusto e mantenibile.

## Fase 11 - Feature avanzate future

1. Lotti e seriali.
2. Barcode scanning.
3. Cycle count avanzato.
4. Resi cliente.
5. Resi fornitore.
6. Integrazione corrieri.
7. Import/export Excel avanzato.
8. Multi-warehouse allocation strategies.
9. Multi-tenant.
10. Notifiche realtime con SignalR.

Deliverable: roadmap evolutiva post-MVP.

## Ordine consigliato delle prime 10 issue

1. Add core packages and architecture registrations.
2. Add EF Core PostgreSQL and ApplicationDbContext.
3. Add Identity cookie auth and auth endpoints.
4. Add Serilog, ProblemDetails and health checks.
5. Add Domain base types and value objects.
6. Implement Product and Category vertical slice.
7. Implement Warehouse and Location vertical slice.
8. Implement StockBalance and InventoryMovement model.
9. Implement receive/transfer/adjust stock commands.
10. Build initial MudBlazor operational shell.
