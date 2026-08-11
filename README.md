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

1. **Authentication** — Login, Register, Me, Google Login
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

```bash
# Required secrets
dotnet user-secrets init
dotnet user-secrets set "Jwt:Secret" "your-secret-key-minimum-32-characters"
dotnet user-secrets set "LuckyPenny:MediatR:LicenseKey" "your-mediatr-license-key"
dotnet user-secrets set "AiService:ModelStore:ApiKey" "your-ai-api-key"
dotnet user-secrets set "Google:ClientId" "your-google-client-id"
```

Update `src/Api/AirlineTicket.Api/appsettings.json` with your Postgre connection string.

### Run

```bash
# Create database if it does not exist
./database/scripts/create_db.sh "postgres://postgres:Admin@123@localhost:5432/AirlineTicketDb"

# Apply database migrations
./run_ef.sh "postgres://postgres:Admin@123@localhost:5432/AirlineTicketDb"

#  Apply seed core data
dotnet run --project src/Api/AirlineTicket.Api/AirlineTicket.Api.csproj --migrate --seed-core

# Start the API
dotnet run --project src/Api/AirlineTicket.Api/AirlineTicket.Api.csproj
```

API available at `http://localhost:5179`. Scalar docs at `/scalar/v1`.

### Docker

The Docker container automatically applies database migrations and seeds core data on startup. If `ASPNETCORE_ENVIRONMENT` is set to `Development`, it also seeds development sample data.

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
Combines the base compose file with the `docker-compose.db.yml` override to spin up a PostgreSQL container alongside the API.

```bash
# Local (API + Postgres)
docker compose --env-file deploy/docker/.env.local -f deploy/docker/docker-compose.local.yml -f deploy/docker/docker-compose.db.yml up -d --build

# Development (API + Postgres)
docker compose --env-file deploy/docker/.env.dev -f deploy/docker/docker-compose.dev.yml -f deploy/docker/docker-compose.db.yml up -d --build

# Production (API + Postgres)
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

```bash
# Unit tests
dotnet test tests/UnitTest/

# Integration tests
dotnet test tests/IntegrationTest/
```

Target: 80%+ code coverage for core business logic.

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
│       ├── Flights/
│       ├── Interactions/
│       ├── Logs/
│       ├── Notifications/
│       ├── Promotions/
│       └── Users/
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
│   └── database/database_design.md      # Full database schema documentation
└── tests/                               # Unit & Integration tests
```
