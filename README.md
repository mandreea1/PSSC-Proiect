# CustomTShirts - DDD Lab

## Echipa

• Andreea

## Domeniul Ales

Custom T-Shirts Ordering System

## Descriere

Sistem de comandă tricouri personalizate implementat folosind Domain-Driven Design (DDD) și arhitectură bazată pe evenimente (Event-Driven). Sistemul gestionează procesul complet de la plasarea comenzii, facturare, până la livrare, folosind microservicii independente care comunică asincron prin Azure Service Bus.

## Bounded Contexts Identificate

1. **Order Context**: Gestionează crearea și plasarea comenzilor de tricouri personalizate
2. **Billing Context**: Se ocupă de emiterea facturilor pentru comenzile plasate
3. **Shipping Context**: Gestionează procesul de livrare a comenzilor facturate

## Event Storming Results

### Eventi identificați:
- **OrderPlaced**: Declanșat când o comandă este plasată
- **InvoiceIssued**: Declanșat când o factură este emisă
- **OrderShipped**: Declanșat când o comandă este expediată

### Fluxul de evenimente:
```
Order → OrderPlaced → Billing → InvoiceIssued → Shipping → OrderShipped
```

## Implementare

### Value Objects

• `Money`: Reprezintă o valoare monetară cu Amount și Currency
• `CustomerId`: Identificator unic pentru client
• `OrderId`: Identificator unic pentru comandă

### Entity States

• **Order**:
  - `Draft`: Comandă în proces de creare
  - `Placed`: Comandă plasată și trimisă spre procesare

• **Invoice**:
  - `Draft`: Factură creată, în așteptare
  - `Issued`: Factură emisă oficial

• **Shipment**:
  - `Pending`: Livrare în așteptare
  - `Shipped`: Comandă expediată

### Operations

1. **Place Order**: Creează și plasează o comandă nouă cu text personalizat pentru tricou
2. **Issue Invoice**: Emite factură pentru comanda plasată
3. **Ship Order**: Procesează și expediază comanda facturată

### Workflow

Event-driven workflow cu comunicare asincronă:
1. Client plasează comandă (POST /orders) → salvează Order cu Status="Placed"
2. Order.Api publică eveniment OrderPlaced
3. Billing.Worker primește OrderPlaced → creează Invoice (Status="Draft") → după 5s actualizează la "Issued" → publică InvoiceIssued
4. Shipping.Worker primește InvoiceIssued → creează Shipment (Status="Pending") → după 5s actualizează la "Shipped" → publică OrderShipped

## Arhitectură

### Microservicii:
- **Order.Api** (port 5294): Publisher-only, nu consumă evenimente
- **Billing.Api** (port 5295): Consumer & Publisher
- **Shipping.Api** (port 5296): Consumer & Publisher

### Baze de date separate:
- OrderDb (SQL Server LocalDB)
- BillingDb (SQL Server LocalDB)
- ShippingDb (SQL Server LocalDB)

### Comunicare asincronă:
- Azure Service Bus Topic: "orders"
- Subscriptions: "billing", "shipping"

## Rulare

### Pregătire:

```powershell
# Build solution
dotnet build CustomTShirts.sln
```

### Pornire automată (3 terminal-uri separate):

```powershell
cd scripts
.\run-with-workers.ps1
```

Scriptul va deschide 3 ferestre PowerShell separate:
- Order.Api (http://localhost:5294/swagger)
- Billing.Api (http://localhost:5295/swagger)
- Shipping.Api (http://localhost:5296/swagger)

### Testare workflow:

1. Accesează http://localhost:5294/swagger
2. POST /orders cu body:
```json
{
  "customerId": "00000000-0000-0000-0000-000000000001",
  "total": 100,
  "customText": "Best Dad 2026"
}
```
3. Observă în terminal-uri:
   - **Order**: Mesaj galben cu detalii comandă
   - **Billing**: Mesaj magenta - Invoice Draft → (5s) → Issued
   - **Shipping**: Mesaj cyan - Shipment Pending → (5s) → Shipped

### Verificare baze de date:

```sql
-- OrderDb
SELECT * FROM Orders

-- BillingDb  
SELECT * FROM Invoices

-- ShippingDb
SELECT * FROM Shipments
```

## Lecții Învățate

### Ce a funcționat bine cu AI

• Implementarea rapidă a logging-ului colorat pentru vizualizare în consolă
• Refactorizarea arhitecturii pentru a respecta principiile DDD (Order ca publisher-only)
• Generarea și aplicarea migrațiilor EF Core pentru schimbări de schemă
• Debugging-ul problemelor cu Service Bus subscriptions

### Limitări ale AI identificate

• Confuzie inițială între console browser și PowerShell terminal
• Erori de sintaxă la folosirea cuvântului rezervat `event` fără @
• Necesitatea de a șterge complet migrațiile Shipping pentru rezolvarea constraint-urilor FK
• Persistența valorilor în Swagger localStorage (comportament normal, nu bug)

### Prompturi Utile

```
"Vreau sa vad in consola cum se schimba statusurile in timp real"
→ A generat console logging cu culori și emojis

"Putem sa facem un delay intre INSERT si UPDATE sa vad schimbarea in DB?"
→ A adăugat Task.Delay(5000) in handlers

"Nu vreau totusi sa fac Order sa fie listener"
→ A ajustat arhitectura: Order publisher-only, valid DDD pattern

"Putem face o coloana in plus la order cu un text personalizat pt tricou"
→ A adăugat CustomText field respectând domeniul business
```

## Design Decisions

### 1. Order ca Publisher-Only
Order bounded context nu ascultă evenimente - este valid în DDD. Order nu trebuie să știe ce se întâmplă downstream (Billing, Shipping). Eventual consistency este asigurată de serviciile respective.

### 2. Status ca String în loc de Int
Pentru lizibilitate în demonstrație și baza de date. Valorile: "Draft", "Placed", "Issued", "Pending", "Shipped" sunt mai clare decât 0, 1, 2, 3.

### 3. Delay de 5 secunde în Handlers
Doar pentru demonstrație - permite verificarea manuală a stărilor intermediare în bază de date. În producție ar fi eliminat.

### 4. Baze de date separate
Fiecare bounded context are propria bază de date, respectând principiul de bounded contexts izolate din DDD.

### 5. CustomText Field
Adăugat pentru a reflecta domeniul business (tricouri personalizate). Demonstrează modeling specific domeniului, nu doar CRUD generic.

## Tehnologii Utilizate

- .NET 10.0
- ASP.NET Core Web API
- Entity Framework Core
- SQL Server LocalDB
- Azure Service Bus
- Swagger/OpenAPI
