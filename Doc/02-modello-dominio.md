# Modello Dominio WMS

## Principi

Il dominio deve proteggere le regole che non possono essere affidate alla UI:

- uno SKU deve essere valido e univoco;
- una giacenza non deve diventare negativa se la policy non lo consente;
- un ordine non può saltare stati non ammessi;
- una spedizione non può uscire senza stock allocato;
- una ricezione non può superare le quantità attese senza regola esplicita;
- ogni movimento stock deve essere tracciabile.

## Base entity

Consiglio una base minima:

- `Entity<TId>` con `Id`;
- `AggregateRoot<TId>` con lista interna di domain events;
- id basati su `Guid` passati nel costruttore/factory per test deterministici;
- timestamp/audit fuori dal dominio puro oppure come interfacce gestite da EF.

Esempio di scelta:

- `ProductId`
- `WarehouseId`
- `LocationId`
- `OrderId`
- `InventoryMovementId`

Usare value object typed id migliora espressività e riduce errori, ma aumenta configurazione EF e DTO mapping. Per il progetto personale lo consiglio sui concetti core, non su ogni tabella secondaria.

## Value object consigliati

- `Sku`: normalizzato, trim, uppercase, lunghezza e caratteri ammessi.
- `Quantity`: decimal positivo con precisione controllata.
- `Money`: amount + currency se si gestiscono prezzi.
- `Barcode`: opzionale.
- `LocationCode`: codice ubicazione leggibile.
- `UnitOfMeasure`: codice e descrizione.
- `Address`: clienti/fornitori.
- `EmailAddress`, se usata nel dominio party.

## Aggregate root

### Product

Responsabilità:

- identità SKU;
- nome e descrizione;
- categoria;
- unità di misura base;
- barcode;
- soglie scorta;
- flag tracciabilità lotto/seriale;
- stato attivo/disattivo.

Non dovrebbe contenere direttamente giacenze: lo stock è dominio Inventory.

### Category

Può essere aggregate semplice:

- nome;
- codice;
- categoria padre opzionale;
- ordinamento;
- attiva/disattiva.

Se le categorie restano pure anagrafiche, si può gestire con CRUD semplice.

### Warehouse

Responsabilità:

- codice e nome;
- indirizzo;
- abilitazione operativa;
- lista di ubicazioni o relazione controllata.

### Location

Concetto fondamentale:

- warehouse;
- codice;
- tipo: receiving, storage, picking, packing, shipping, quarantine, adjustment;
- capacità opzionale;
- attiva/disattiva;
- regole di stoccaggio opzionali.

Le ubicazioni possono essere entity dentro `Warehouse` oppure aggregate separato. Per semplicità MVP: aggregate separato con vincolo `WarehouseId + Code` univoco.

### InventoryItem / StockBalance

Rappresenta la disponibilità corrente:

- prodotto;
- magazzino;
- ubicazione;
- lotto/seriale opzionale;
- quantità on hand;
- quantità reserved;
- quantità available calcolata.

Regola:

```text
Available = OnHand - Reserved
```

In PostgreSQL conviene materializzare `OnHand` e `Reserved`, aggiornandoli in transazione con movimenti e prenotazioni.

### InventoryMovement

Registro immutabile dei movimenti:

- tipo: receipt, issue, transfer, adjustment, reservation, unreservation, shipment;
- prodotto;
- quantità;
- from location;
- to location;
- riferimento business: ordine, ricezione, rettifica;
- utente;
- timestamp;
- motivo;
- note.

I movimenti devono essere append-only. Correzioni tramite movimento inverso/rettifica.

### CustomerOrder

Responsabilità:

- cliente;
- righe ordine;
- stato;
- conferma;
- allocazione/prenotazione;
- picking;
- spedizione;
- annullamento.

Stati consigliati:

```text
Draft -> Confirmed -> Allocated -> Picking -> Packed -> Shipped
Draft/Confirmed/Allocated -> Cancelled
```

Regole:

- modifiche righe solo in `Draft`;
- conferma richiede almeno una riga valida;
- allocazione richiede stock disponibile o backorder consentito;
- spedizione richiede righe preparate.

### PurchaseOrder

Responsabilità:

- fornitore;
- righe attese;
- stato;
- ricezioni parziali;
- chiusura.

Stati:

```text
Draft -> Sent -> PartiallyReceived -> Received -> Closed
Draft/Sent -> Cancelled
```

### Supplier e Customer

Possono condividere un concetto `Party`, ma per chiarezza MVP tenerli separati:

- codice;
- nome;
- contatti;
- indirizzo;
- partita IVA/codice fiscale opzionale;
- attivo/disattivo.

## Servizi di dominio

Da introdurre solo dove una regola attraversa aggregate diversi:

- `InventoryAllocationService`: assegna stock disponibile a un ordine.
- `StockTransferService`: valida trasferimenti tra ubicazioni.
- `OrderWorkflowService`: se la macchina stati diventa complessa.

Non usare servizi di dominio per semplici setter.

## Permessi funzionali

Ruoli iniziali:

- `Administrator`: configurazione, utenti, permessi.
- `WarehouseManager`: anagrafiche operative, inventario, report.
- `Operator`: ricezioni, picking, trasferimenti, rettifiche limitate.
- `Sales`: ordini cliente e clienti.
- `Purchasing`: ordini fornitore e fornitori.
- `Viewer`: sola lettura.

Permessi granulari consigliati:

- `Products.Read`, `Products.Write`
- `Warehouses.Read`, `Warehouses.Write`
- `Inventory.Read`, `Inventory.Move`, `Inventory.Adjust`, `Inventory.Count`
- `Orders.Read`, `Orders.Write`, `Orders.Ship`
- `Purchasing.Read`, `Purchasing.Write`, `Purchasing.Receive`
- `Reports.Read`
- `Users.Manage`

Usare ruoli come contenitori di permessi, non come unica logica hard-coded.

## Tabelle principali

- `products`
- `categories`
- `warehouses`
- `locations`
- `stock_balances`
- `inventory_movements`
- `customers`
- `suppliers`
- `customer_orders`
- `customer_order_lines`
- `purchase_orders`
- `purchase_order_lines`
- `stock_reservations`
- `cycle_counts`
- `cycle_count_lines`
- `shipments`
- `shipment_lines`
- `audit_events`
- `outbox_messages`
- tabelle Identity ASP.NET Core

## Indici fondamentali

- `products.sku` unique
- `categories.code` unique
- `warehouses.code` unique
- `locations (warehouse_id, code)` unique
- `stock_balances (product_id, location_id, lot_id, serial_number)` unique
- `inventory_movements (product_id, occurred_at)`
- `inventory_movements (reference_type, reference_id)`
- `customer_orders.order_number` unique
- `purchase_orders.order_number` unique
- `customers.code` unique
- `suppliers.code` unique

## Note su stock negativo

Consiglio default: vietare stock negativo.

Eccezioni possibili:

- rettifica autorizzata da manager;
- ubicazione speciale `adjustment`;
- feature futura di backorder senza decrementare on hand.

Questa regola deve stare in Application/Domain, non nella UI.
