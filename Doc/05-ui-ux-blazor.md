# UI/UX Blazor e MudBlazor

## Obiettivo UI

La UI deve sembrare uno strumento operativo, non una landing page. Un WMS viene usato per cercare, filtrare, confrontare, registrare movimenti e correggere errori rapidamente.

Priorità:

- densità informativa;
- navigazione chiara;
- feedback immediato;
- form veloci;
- tabelle filtrabili;
- layout responsive per tablet;
- accessibilità da tastiera dove utile.

## Struttura navigazione

Menu principale:

- Dashboard
- Prodotti
- Categorie
- Magazzini
- Ubicazioni
- Inventario
- Movimenti
- Ordini clienti
- Ordini fornitori
- Clienti
- Fornitori
- Report
- Utenti e ruoli
- Impostazioni

## Pagine MVP

### Dashboard

Widget:

- ordini da evadere;
- ricezioni attese;
- prodotti sotto scorta;
- movimenti recenti;
- alert stock negativo/conflitti;
- riepilogo giacenze per magazzino.

### Prodotti

Lista:

- SKU;
- nome;
- categoria;
- unità misura;
- stock totale;
- sotto scorta;
- stato.

Azioni:

- crea/modifica;
- attiva/disattiva;
- dettaglio stock per ubicazione;
- storico movimenti.

### Magazzini e ubicazioni

Gestione:

- albero o tabella ubicazioni;
- filtri per tipo ubicazione;
- azioni rapide per creare zona/corsia/bin;
- stato attivo/disattivo.

### Inventario

Vista operativa:

- prodotto;
- magazzino;
- ubicazione;
- on hand;
- reserved;
- available;
- lotto/seriale se presente.

Azioni:

- trasferisci;
- rettifica;
- prenota/rilascia dove consentito;
- storico movimenti.

### Movimenti

Registro append-only:

- data;
- tipo;
- SKU;
- quantità;
- origine;
- destinazione;
- riferimento;
- utente.

Filtri per data, prodotto, magazzino, tipo e riferimento.

### Ordini clienti

Workflow visuale:

- draft;
- confirmed;
- allocated;
- picking;
- packed;
- shipped;
- cancelled.

La pagina dettaglio deve mostrare righe, disponibilità, azioni consentite e blocchi motivati.

### Ordini fornitori

Funzioni:

- crea ordine;
- invia/segna inviato;
- ricevi parziale;
- ricevi totale;
- chiudi.

### Report

Report iniziali:

- giacenze per magazzino;
- prodotti sotto scorta;
- movimenti per periodo;
- ordini aperti;
- performance ricezioni/spedizioni.

## Componenti condivisi

Creare componenti riusabili:

- `PageHeader`
- `DataTableToolbar`
- `SearchBox`
- `EmptyState`
- `ErrorState`
- `LoadingOverlay`
- `ConfirmDialog`
- `PermissionView`
- `StatusChip`
- `QuantityField`
- `SkuField`
- `LocationPicker`
- `ProductPicker`

## Pattern dati client

Usare typed HTTP clients per area:

- `ProductsApiClient`
- `WarehousesApiClient`
- `InventoryApiClient`
- `OrdersApiClient`
- `PurchasingApiClient`
- `IdentityApiClient`
- `ReportsApiClient`

Ogni client:

- usa `HttpClient` da DI;
- serializza DTO Contracts;
- gestisce problem details;
- invia token antiforgery sui metodi mutativi;
- non contiene logica business.

## Stato client

Per MVP:

- stato locale nel componente per form e filtri;
- servizi scoped per auth/current user;
- cache leggera solo per lookup stabili come categorie e magazzini.

Evitare state manager complessi finché non serve.

## Validazione UI

La validazione definitiva resta server-side con FluentValidation.

La UI può replicare validazioni semplici per ergonomia:

- required;
- lunghezza;
- range numerico;
- formato SKU.

Gli errori server devono essere mostrati accanto ai campi quando possibile.

## Responsività

Desktop:

- tabelle dense;
- filtri laterali o toolbar;
- drawer persistente.

Tablet:

- drawer collassabile;
- tabelle con colonne prioritarie;
- azioni in menu contestuale.

Mobile:

- supporto base;
- liste card compatte per operazioni semplici;
- non progettare workflow complessi solo per mobile nella prima fase.

## Accessibilità

- contrasto sufficiente;
- focus visibile;
- label form esplicite;
- icone con tooltip;
- bottoni distruttivi con conferma;
- messaggi di errore testuali, non solo colore.

## Stile

Il tema MudBlazor va orientato a gestionale:

- palette sobria, non monocromatica;
- spaziatura compatta;
- cards solo per widget o elementi ripetuti;
- tabelle e toolbar come superficie principale;
- niente hero/marketing layout.

## Performance Blazor

- virtualizzazione per liste lunghe;
- paginazione server-side;
- debounce su search;
- `@key` nelle liste;
- evitare componenti giganti;
- evitare render inutili su dashboard con auto-refresh;
- caricare dati on demand nei tab.
