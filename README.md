# doc_ai — User Management Microservice

Microservizio REST per la gestione CRUD di utenti (creazione, lettura, aggiornamento, cancellazione), sviluppato in ASP.NET Core come base dimostrativa/di riferimento per servizi CRUD single-entity.

## Tecnologie principali

- **.NET 10 / ASP.NET Core Web API** — runtime e framework applicativo
- **Entity Framework Core 10 (provider SQLite)** — persistenza dati e migrazioni
- **Swashbuckle (Swagger/OpenAPI)** — documentazione e console interattiva delle API
- **xUnit + Microsoft.AspNetCore.Mvc.Testing** — test di integrazione end-to-end sul controller

## Setup e avvio

Prerequisiti: [.NET SDK 10](https://dotnet.microsoft.com/download) installato.

```bash
# Ripristino pacchetti e build dell'intera soluzione
dotnet build

# Avvio dell'API (applica automaticamente le migration EF Core all'avvio)
dotnet run --project src/UserManagement.Api
```

L'API espone Swagger UI su `/swagger` in ambiente di sviluppo. La connection string del database SQLite è configurata in `src/UserManagement.Api/appsettings.json` (chiave `ConnectionStrings:DefaultConnection`).

## Test

```bash
dotnet test
```

Esegue i test di integrazione in `src/UserManagement.Api.Tests`, che avviano l'API in-process (`WebApplicationFactory`) su un database SQLite isolato e temporaneo per ogni esecuzione.

## Struttura delle cartelle principali

```text
UserManagement.slnx
src/
├── UserManagement.Api/            # Progetto del microservizio
│   ├── Controllers/                # Endpoint HTTP (UsersController)
│   ├── Models/                     # Entità di dominio (User)
│   ├── Dtos/                       # Contratti di input/output esposti dalle API
│   ├── Data/                       # DbContext e migration EF Core
│   ├── Repositories/               # Astrazione dell'accesso ai dati
│   ├── Middleware/                 # Gestione centralizzata delle eccezioni
│   └── Program.cs                  # Composizione root (DI, pipeline HTTP)
└── UserManagement.Api.Tests/       # Test di integrazione xUnit
```

## Interfacce esposte

Il servizio espone risorse REST sotto il percorso base `api/v1/users`:

| Metodo | Percorso             | Descrizione                          |
| ------ | -------------------- | ------------------------------------ |
| GET    | `/api/v1/users`      | Elenco di tutti gli utenti           |
| GET    | `/api/v1/users/{id}` | Dettaglio utente per Id              |
| POST   | `/api/v1/users`      | Creazione di un nuovo utente         |
| PUT    | `/api/v1/users/{id}` | Aggiornamento integrale di un utente |
| DELETE | `/api/v1/users/{id}` | Eliminazione di un utente            |

Contratto dati (`Dtos`):

- **Input** (`CreateUserDto` / `UpdateUserDto`): `firstName`, `lastName`, `email` — tutti obbligatori, `email` validata come indirizzo email valido.
- **Output** (`UserDto`): `id`, `firstName`, `lastName`, `email`, `createdAt`, `updatedAt`.

Vincoli applicativi: l'indirizzo `email` è univoco per utente; una creazione o modifica con email già assegnata a un altro utente restituisce `409 Conflict`. Un Id non esistente restituisce `404 Not Found`. Errori di validazione sul payload restituiscono `400 Bad Request`. Le risposte di errore seguono il formato `ProblemDetails`.

L'astrazione `IUserRepository` (in `Repositories/`) disaccoppia il controller dall'accesso dati EF Core; l'implementazione corrente è `UserRepository`, basata su `AppDbContext`.

## Note

- Autenticazione/autorizzazione: **non implementata** — da completare se il servizio deve essere esposto oltre un contesto locale/fidato.
- Ambiente e deploy (containerizzazione, CI/CD, configurazione per ambienti multipli): **da completare**, non presenti nel codice attuale.
- Il database SQLite è pensato per sviluppo/dimostrazione; un utilizzo in produzione richiederebbe valutazione di un provider EF Core diverso (da completare/valutare).
