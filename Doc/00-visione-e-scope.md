# SuperStorage WMS - Visione e Scope

## Obiettivo

SuperStorage è un Warehouse Management System sviluppato come progetto personale ma impostato con criteri da prodotto reale: dominio esplicito, database robusto, UI operativa fluida, sicurezza corretta, test automatizzati e osservabilità.

La priorità non è solo "far funzionare" le schermate, ma costruire una base estendibile: ogni funzionalità deve essere aggiungibile senza riscrivere il nucleo applicativo.

## Stato attuale della soluzione

La soluzione è già organizzata in progetti compatibili con Clean Architecture:

- `SuperStorage.Domain`: modello di dominio, value object, regole e domain events.
- `SuperStorage.Application`: use case CQRS con MediatR, validazioni, contratti applicativi, pipeline behavior.
- `SuperStorage.Infrastructure`: EF Core, Identity, repository concreti, logging sink, servizi esterni.
- `SuperStorage.Api`: endpoint HTTP/BFF, middleware, auth cookie, antiforgery, health checks.
- `SuperStorage.Client`: Blazor WebAssembly + MudBlazor.
- `SuperStorage.Contracts`: DTO e contratti condivisi tra client e API.

Al momento i progetti sono quasi vuoti; quindi le decisioni architetturali vanno fissate prima di implementare feature verticali.

## Scope MVP consigliato

L'MVP deve dimostrare un ciclo WMS completo ma controllato:

1. Autenticazione, ruoli e autorizzazioni.
2. Anagrafiche base: prodotti, categorie, magazzini, ubicazioni, clienti, fornitori.
3. Stock disponibile per prodotto e ubicazione.
4. Movimenti: entrata, uscita, trasferimento, rettifica.
5. Ordini cliente con workflow essenziale: bozza, confermato, allocato, in preparazione, spedito, annullato.
6. Ordini fornitore/ricezione merce: creato, inviato, parzialmente ricevuto, ricevuto, chiuso.
7. Report operativi: giacenze, movimenti, prodotti sotto scorta, ordini aperti.
8. Audit log applicativo e logging tecnico.
9. Test unitari su dominio/use case e test di integrazione su API + PostgreSQL.

## Cosa manca rispetto alla lista iniziale

Oltre agli elementi già indicati, per un WMS completo servono alcuni concetti fondamentali:

- `Location`/ubicazioni: magazzino, zona, corsia, scaffale, ripiano, bin.
- `InventoryBalance`: giacenza calcolata o materializzata per prodotto/ubicazione.
- `Reservation`: stock prenotato per ordini non ancora spediti.
- `Lot`/lotto e `SerialNumber`: opzionali ma importanti per tracciabilità.
- `UnitOfMeasure`: pezzi, colli, pallet, kg, litri, conversioni.
- `StockAdjustment`: rettifiche inventariali con motivo e approvazione.
- `CycleCount`: inventario periodico.
- `Picking` e `Packing`: preparazione e colli spedizione.
- `Receiving`: ricezione merce da fornitore, anche parziale.
- `Shipment`: spedizione verso cliente.
- `Audit Trail`: chi ha fatto cosa, quando e da dove.
- `Outbox`: pubblicazione affidabile di domain/integration events.

## Decisioni guida

- Usare Clean Architecture in modo pragmatico: dipendenze verso l'interno, ma niente astrazioni inutili.
- Usare DDD dove il dominio ha regole vere: stock, prenotazioni, ordini e movimenti.
- Usare MediatR per ogni endpoint mutativo o query applicativa.
- Usare FluentValidation nei pipeline behavior, non nei controller/endpoints.
- Usare EF Core come Unit of Work; repository solo per aggregate root con invarianti importanti.
- Usare DTO nei Contracts; non esporre entità Domain verso UI/API.
- Usare cookie auth in stile BFF: niente JWT salvati nel browser.
- Rendere l'osservabilità parte del progetto fin dall'inizio.

## Vincoli consigliati

- PostgreSQL come unico database transazionale.
- Migrazioni EF Core versionate e revisionate.
- Niente lazy loading EF Core.
- Query read-only sempre con `AsNoTracking`.
- Paginazione obbligatoria su liste operative.
- Idempotenza per comandi sensibili: ricezioni, spedizioni, rettifiche, import.
- Transazioni esplicite per use case che modificano stock e ordini.
- Concorrenza ottimistica su aggregate critici tramite `xmin` PostgreSQL o `RowVersion` equivalente.

## Non obiettivi iniziali

Questi temi sono validi, ma conviene rimandarli dopo il MVP:

- Multi-tenant completo.
- Integrazione reale con corrieri, ERP o marketplace.
- Barcode scanner hardware specifico.
- Allocazione avanzata multi-strategia.
- Slotting automatico.
- Forecasting e machine learning.
- Mobile app nativa.

## Definizione di "prodotto completo"

Per questo progetto, "completo" significa:

- un utente può configurare magazzino/prodotti;
- può registrare entrate, uscite, trasferimenti e rettifiche;
- può gestire ordini cliente e fornitore con stati coerenti;
- può vedere giacenze e report affidabili;
- il sistema impedisce stock negativo non autorizzato;
- ogni operazione critica è tracciata;
- l'app è testata, documentata, avviabile localmente e deployabile.
