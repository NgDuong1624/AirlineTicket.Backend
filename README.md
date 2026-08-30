# AirlineTicket Backend

A modular monolith backend system for airline ticket booking, built on **.NET 10** with **Clean Architecture**, **CQRS**, and **Domain-Driven Design**.

---

## Architecture

The system follows a **Modular Monolith** architecture with 8 independent business modules, each adhering to Clean Architecture with 4 layers:

| Layer              | Responsibility                                                                                       |
| ------------------ | ---------------------------------------------------------------------------------------------------- |
| **Domain**         | Core entities, enums, domain events. Zero external dependencies.                                     |
| **Application**    | Business logic (use cases), CQRS handlers (MediatR), validators (FluentValidation), DTOs.            |
| **Infrastructure** | Database access (EF Core DbContext), Fluent API config, repositories, external service integrations. |
| **Api**            | Minimal API endpoints (implements `IEndpoint` interface).                                            |

### Modules

| Module            | Schema          | Description                                                            |
| ----------------- | --------------- | ---------------------------------------------------------------------- |
| **Users**         | `identity`      | Authentication (JWT), user management, RBAC, dynamic permission scopes |
| **Flights**       | `flights`       | Airlines, airports, airplanes, routes, flights, seat maps              |
| **Bookings**      | `bookings`      | Booking lifecycle, passengers, payments, e-tickets                     |
| **Promotions**    | `promotions`    | Coupons (percentage/fixed), campaigns (admin & partner scoped)         |
| **Interactions**  | `interactions`  | AI Travel Assistant, reviews                                           |
| **CMS**           | `cms`           | Admin/Partner dashboards, system settings, articles                    |
| **Logs**          | `logs`          | System logs, partner activity logs (paginated)                         |
| **Notifications** | `notifications` | Email, SMS, Push notification templates and delivery                   |

### BuildingBlocks (Shared Infrastructure)

| Component                            | Purpose                                                             |
| ------------------------------------ | ------------------------------------------------------------------- |
| **CQRS**                             | `ICommand`/`IQuery` + `ICommandHandler`/`IQueryHandler` via MediatR |
| **LoggingBehavior**                  | Auto-logs MediatR request/response                                  |
| **CachingBehavior**                  | Auto-caches query results based on query attributes                 |
| **ValidationBehavior**               | Auto-validates input via FluentValidation before handler execution  |
| **CorrelationMiddleware**            | Generates/propagates `X-Correlation-ID` for request tracing         |
| **RequestResponseLoggingMiddleware** | Logs HTTP request/response details                                  |
| **JWT Auth**                         | Bearer token authentication, supports SignalR query string tokens   |
| **DynamicPermissionPolicyProvider**  | Dynamic authorization based on permission codes                     |
| **AirlineResourceHandler**           | Ensures Partner/Staff can only access their own airline's resources |
| **ICacheService**                    | Abstraction over Redis Cloud with in-memory fallback                |

## Real-time (SignalR)

### SeatHub (`/hubs/seats`)

- Groups clients by `flight-{flightId}` rooms
- Broadcasts `SeatUpdated(flightId, seatNumber, isAvailable)` on seat status changes
- **Anti-double-booking:** Atomic `ExecuteUpdateAsync` with `WHERE IsAvailable = 1` — no row/table locks

### SupportChatHub (`/hubs/support`)

- Customer ↔ Staff live chat scoped by airline
- Auto-assigns customers to least-busy staff of the same airline
- Re-assigns customers when staff disconnects
- In-memory session storage via `ConcurrentDictionary`

### NotificationHub (`/hubs/notifications`)

- Real-time notification delivery to authenticated users
- Requires JWT authentication (supports token in query string for WebSockets)

## Database

**PostgreSQL** with 8 schemas as bounded contexts.

For comprehensive details about the database schemas, seed data structure, and step-by-step setup guides, refer to the [Database README](database/README.md).

- `users` (for identity)
- `flights` (for flights module)
- `bookings` (for bookings module)
- `promotions` (for promotions module)
- `interactions` (for interactions module)
- `cms` (for CMS module)
- `logs` (for logs module)
- `notifications` (for notifications module)

### Seed Data

The database includes a modular seeding system to populate initial data. Seed files are organized into `core`, `development`, and `test` categories.

- **`core/`**: Essential reference data (roles, permissions, airlines, airports, aircraft models, notification templates, system admin user). This data is critical for the system's basic operation.
- **`development/`**: Sample data for local development and demonstration purposes (sample users, airline admins/staff, airplanes, routes, flights, bookings, promotions, CMS content, reviews).
- **`test/`**: Minimal data sets specifically designed for integration and unit testing.

### Key Design Decisions

- **Soft Delete** — `IsDeleted` column + PostgreSQL triggers (if applicable) or application-level handling
- **Performance Indexes** — On `DepartureTime`, `RouteId`, `PnrCode`, `TicketNumber`, `UserId`
- **UUID Primary Keys** — `UUID` for all entity IDs

## API Endpoints

The system exposes **100+ REST API endpoints** across 12 sections:

1. **Authentication** — Login, Register, Me (with booking spending stats), Google Login, Update Language
2. **Flights (Public)** — Search (one-way, round-trip), Details, Trending, Seats
3. **Airports, Airlines & Routes** — List, Search
4. **Bookings** — Create, Pay, History, Update, Cancel
5. **Tickets** — E-ticket details
6. **Promotions** — List, Apply coupon
7. **AI Chat** — Q&A with AI Travel Assistant
8. **Staff** — Flight list, Seat map, Telesales booking, Sales board
9. **Admin** — Users, Permissions, Airports, Airlines, Flights, Bookings, Coupons, Campaigns, Dashboard, Settings, Logs
10. **Partner** — Routes, Airplanes, Flights, Aircraft, Coupons, Campaigns, Bookings, Staff, Settings, Dashboard, Logs
11. **Module Status** — Health checks for Interactions and Notifications
12. **SignalR Hubs** — SeatHub, SupportChatHub

Full interactive API documentation available at `/scalar/v1` when running.

## Setup

### Prerequisites

- .NET 10 SDK
- Postgres 16+
- Redis (optional — falls back to in-memory cache)
- EF Core CLI: `dotnet tool install --global dotnet-ef`

### Configuration

The backend uses a shared configuration pattern (`appsettings.shared.json` and `appsettings.shared.Development.json` at `src/`) linked into the 3 runnable hosts (`Api`, `SignalR`, `Worker`).

1. Copy or update `src/appsettings.shared.Development.json` with your secrets and development configurations:
```json
{
  "Jwt": {
    "Secret": "your-secret-key-minimum-32-characters"
  },
  "LuckyPenny": {
    "MediatR": {
      "LicenseKey": "your-mediatr-license-key"
    }
  },
  "AiService": {
    "ModelStore": {
      "ApiKey": "your-ai-api-key"
    }
  },
  "Google": {
    "ClientId": "your-google-client-id"
  }
}
```

2. Update database and Redis connection strings in `src/appsettings.shared.json` (or override per project in `appsettings.json`).

### Run (Local .NET)

```bash
# 1. Create database if it does not exist
./database/scripts/create_db.sh "postgres://postgres:Admin@123@localhost:5432/AirlineTicketDb"

# 2. Apply database migrations
./run_ef.sh "postgres://postgres:Admin@123@localhost:5432/AirlineTicketDb"

# 3. Apply seed core data
dotnet run --project src/Api/AirlineTicket.Api/AirlineTicket.Api.csproj --migrate --seed-core

# 4. Start the 3 backend host services (in separate terminals):

# Terminal 1: API Host (Port 5179)
dotnet run --project src/Api/AirlineTicket.Api/AirlineTicket.Api.csproj

# Terminal 2: SignalR Realtime Hubs (Port 5084)
dotnet run --project src/Realtime/AirlineTicket.SignalR/AirlineTicket.SignalR.csproj

# Terminal 3: Background Worker Jobs
dotnet run --project src/Workers/AirlineTicket.Worker/AirlineTicket.Worker.csproj
```

- API Gateway & REST endpoints: `http://localhost:5179` (Scalar docs at `/scalar/v1`)
- Realtime SignalR Hubs: `http://localhost:5084` (`/hubs/seats`, `/hubs/support`, `/hubs/notifications`, `/hubs/fare-alerts`)
- Background Workers: Runs automated jobs (refunds, cancellations, delay notifier, dynamic pricing, log retention)

### Docker

The Docker setup builds all 3 services (`airlineticket-api`, `airlineticket-signalr`, `airlineticket-worker`) in a multi-container stack.
The API container automatically applies database migrations and seeds core data on startup (and seeds development demo data when `ASPNETCORE_ENVIRONMENT=Development`).

You have two options for running the application with Docker:

**Option A: Connect to an external database**
Requires database environment variables (`DB_HOST`, `DB_PASSWORD`, etc.) to be set in your `.env` file.

```bash
# Local
docker compose --env-file deploy/docker/.env.local -f deploy/docker/docker-compose.local.yml up -d --build

# Development
docker compose --env-file deploy/docker/.env.dev -f deploy/docker/docker-compose.dev.yml up -d --build

# Production
docker compose --env-file deploy/docker/.env.prod -f deploy/docker/docker-compose.prod.yml up -d --build
```

**Option B: Create a local PostgreSQL database container**
Combines the base compose file with the `docker-compose.db.yml` override to spin up a PostgreSQL container alongside all 3 backend services.

```bash
# Local (API + SignalR + Worker + Postgres)
docker compose --env-file deploy/docker/.env.local -f deploy/docker/docker-compose.local.yml -f deploy/docker/docker-compose.db.yml up -d --build

# Development (API + SignalR + Worker + Postgres)
docker compose --env-file deploy/docker/.env.dev -f deploy/docker/docker-compose.dev.yml -f deploy/docker/docker-compose.db.yml up -d --build

# Production (API + SignalR + Worker + Postgres)
docker compose --env-file deploy/docker/.env.prod -f deploy/docker/docker-compose.prod.yml -f deploy/docker/docker-compose.db.yml up -d --build
```

### Environment Variables (Docker)

| Variable                          | Description                                                        |
| --------------------------------- | ------------------------------------------------------------------ |
| `DB_HOST`                         | Database host (use `host.docker.internal` for external DB on host) |
| `DB_PORT`                         | Database port (default: 5432)                                      |
| `DB_NAME`                         | Database name (default: AirlineTicketDb)                           |
| `DB_USER`                         | Database user (default: postgres)                                  |
| `DB_PASSWORD`                     | Database password                                                  |
| `DB_POOLING`                      | Enable connection pooling (default: true)                          |
| `DB_MIN_POOL_SIZE`                | Minimum connection pool size (default: 10)                         |
| `DB_MAX_POOL_SIZE`                | Maximum connection pool size (default: 100)                        |
| `REDIS_CONNECTION_STRING`         | Redis connection string                                            |
| `CORS_ORIGINS`                    | Allowed CORS origins (frontend URL)                                |
| `AI_SERVICE_API_KEY`              | AI API key                                                         |
| `AI_SERVICE_BASE_URL`             | AI API base URL                                                    |
| `AI_SERVICE_MODEL`                | AI model name                                                      |
| `GOOGLE_CLIENT_ID`                | Google OAuth Client ID                                             |
| `LUCKY_PENNY_MEDIATR_LICENSE_KEY` | MediatR license key                                                |

## Testing

### Prerequisites for Integration Tests

- **Docker Desktop / Docker Engine** running (required by **Testcontainers** to spin up isolated PostgreSQL 16 and Redis 7 containers).

### Test Commands

```bash
# Run all unit tests
dotnet test tests/UnitTest/

# Run all integration tests (uses Testcontainers: PostgreSQL + Redis)
dotnet test tests/IntegrationTest/

# Run individual module integration tests
dotnet test tests/IntegrationTest/Modules/v1/Users/AirlineTicket.Modules.Users.Api.IntegrationTests/
dotnet test tests/IntegrationTest/Modules/v1/Flights/AirlineTicket.Modules.Flights.Api.IntegrationTests/
dotnet test tests/IntegrationTest/Modules/v1/Bookings/AirlineTicket.Modules.Bookings.Api.IntegrationTests/
dotnet test tests/IntegrationTest/Modules/v1/Promotions/AirlineTicket.Modules.Promotions.Api.IntegrationTests/
dotnet test tests/IntegrationTest/Modules/v1/Interactions/AirlineTicket.Modules.Interactions.Api.IntegrationTests/
dotnet test tests/IntegrationTest/Modules/v1/CMS/AirlineTicket.Modules.CMS.Api.IntegrationTests/
dotnet test tests/IntegrationTest/Modules/v1/Logs/AirlineTicket.Modules.Logs.Api.IntegrationTests/
dotnet test tests/IntegrationTest/Modules/v1/Notifications/AirlineTicket.Modules.Notifications.Api.IntegrationTests/

# API Testing (Postman / Newman)
newman run tests/ApisTest/AirlineTicket.postman_collection.json -e tests/ApisTest/AirlineTicket.postman_environment.json
```

### Integration Test Architecture

- **Testcontainers**: Automated PostgreSQL (`postgres:16-alpine`) and Redis (`redis:7-alpine`) lifecycle per test run.
- **CustomWebApplicationFactory**: In-memory test host with auto schema migrations (`DatabaseInitializer.MigrateAsync`) and core seed data (`DatabaseInitializer.SeedCoreAsync`).
- **Test Authentication Handler**: Header-based mock authentication supporting multi-role scopes (`Admin`, `Customer`, `Partner`, `Staff`) and custom permissions.

Target: 80%+ code coverage for core business logic.

### Postman API Test Suite (`tests/ApisTest/`)

- **Collection**: `tests/ApisTest/AirlineTicket.postman_collection.json`
- **Environment**: `tests/ApisTest/AirlineTicket.postman_environment.json`
- **Modules**:
  - `01. Users & Authentication` (4-role JWT login, auto token capture, refresh token rotation, logout, admin user management, partner staff)
  - `02. Flights & Infrastructure` (Airports, airlines, aircraft models, airplanes, routes, public flight search, partner & admin flight operations)
  - `03. Bookings & Payments` (Public booking lifecycle, PNR lookup, payments, e-tickets, staff sales, partner & admin dashboards)
  - `04. Promotions` (Coupons & campaigns for public, partner, and admin)
  - `05. CMS & System Settings` (System settings, admin/partner dashboard metrics)
  - `06. Notifications` (User notifications, read status, admin templates)
  - `07. Logs & Auditing` (System & partner audit logs)
  - `08. Interactions & QA Assistant` (Health checks, AI Travel Assistant Q&A)
  - `09. Support Chat` (SignalR WebSocket handshake & negotiate)

## Project Structure

```
AirlineTicket.Backend/
├── src/
│   ├── Api/AirlineTicket.Api/           # API Gateway Host
│   │   ├── Program.cs                   # Bootstrap, middleware, DI
│   │   ├── ModuleRegistration.cs        # Module assembly registration
│   │   └── DatabaseInitializer.cs       # Migrations & seeding entry
│   ├── BuildingBlock/                   # Shared infrastructure (4 projects)
│   ├── Modules/v1/                      # 8 Business modules (4 layers each)
│   │   ├── Bookings/
│   │   ├── CMS/
│   │   ├── Flights/
│   │   ├── Interactions/
│   │   ├── Logs/
│   │   ├── Notifications/
│   │   ├── Promotions/
│   │   └── Users/
│   ├── Realtime/AirlineTicket.SignalR/  # SignalR Hubs (seats, support chat, notifications)
│   └── Workers/AirlineTicket.Worker/    # Background workers
├── database/                            # SQL scripts for seeding
│   ├── core/                            # Core reference data
│   ├── development/                     # Sample data for local dev/demo
│   ├── test/                            # Minimal data for integration/unit tests
│   ├── scripts/                         # Seed runner scripts
│   │   ├── run_seed_core.sh             # Run core seeds only
│   │   ├── run_seed_dev.sh              # Run development seeds only
│   │   └── run_seed_test.sh             # Run test seeds only
│   └── run_seeds.sh                     # Master script to run core + dev seeds
├── deploy/                              # Docker & Nginx configs
├── docs/                                # Documentation
│   ├── api/api-list.md                  # Complete API endpoint reference
│   ├── database/database_design.md      # Full database schema documentation
│   └── modules/                         # Module specification documentation
│       ├── bookings.md
│       ├── cms.md
│       ├── flights.md
│       ├── interactions.md
│       ├── logs.md
│       ├── notifications.md
│       ├── promotions.md
│       ├── support-chat.md
│       └── users.md
└── tests/                               # Unit, Integration, Performance & API tests
    ├── ApisTest/                        # Postman collection & environment JSON
    │   ├── AirlineTicket.postman_collection.json
    │   └── AirlineTicket.postman_environment.json
    ├── IntegrationTest/                 # Module API integration tests
    ├── UnitTest/                        # Module application unit tests
    └── PerformanceTest/                 # Load testing & Microbenchmarks
        ├── AirlineTicket.LoadTests/     # NBomber HTTP scenario & load tests
        └── AirlineTicket.Benchmarks/    # BenchmarkDotNet memory & execution benchmarks
```

---

## Performance Testing (SRS Verification)

Performance testing suites evaluate high-throughput scenarios, concurrency bottlenecks, and memory allocation efficiency under load to verify Non-Functional Requirements (SRS).

### 1. Load & Scenario Testing (`AirlineTicket.LoadTests`)

Built with **NBomber 5** to simulate real-world traffic on core REST API endpoints.

- **Scenarios Covered**:
  - `flight_search_load`: Evaluates throughput and latency percentiles on `POST /api/flights` under constant query load.
  - `booking_creation_load`: Validates concurrent ticket reservations and atomic seat locks on `POST /api/bookings` with dynamic seat discovery.
- **Run Load Tests**:
  ```bash
  API_BASE_URL="http://localhost:5179" dotnet run --project tests/PerformanceTest/AirlineTicket.LoadTests/AirlineTicket.LoadTests.csproj
  ```
- **Output**: Generates HTML and console latency distribution reports under `./reports`.

### 2. Micro-benchmarks (`AirlineTicket.Benchmarks`)

Built with **BenchmarkDotNet** for in-process algorithmic analysis and memory allocation diagnostics.

- **Benchmarks Covered**:
  - `FlightPricingBenchmarks`: Compares base fare calculations versus dynamic demand-based pricing algorithms, verifying execution speed and GC allocation overhead (`[MemoryDiagnoser]`).
- **Run Benchmarks**:
  ```bash
  dotnet run -c Release --project tests/PerformanceTest/AirlineTicket.Benchmarks/AirlineTicket.Benchmarks.csproj
  ```
