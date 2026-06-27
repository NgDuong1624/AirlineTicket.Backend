# AirlineTicket Backend

A modular monolith backend system for airline ticket booking, built on **.NET 10** with **Clean Architecture**, **CQRS**, and **Domain-Driven Design**.

---

## Architecture

The system follows a **Modular Monolith** architecture with 8 independent business modules, each adhering to Clean Architecture with 4 layers:

| Layer | Responsibility |
|-------|---------------|
| **Domain** | Core entities, enums, domain events. Zero external dependencies. |
| **Application** | Business logic (use cases), CQRS handlers (MediatR), validators (FluentValidation), DTOs. |
| **Infrastructure** | Database access (EF Core DbContext), Fluent API config, repositories, external service integrations. |
| **Api** | Minimal API endpoints (implements `IEndpoint` interface). |

### Modules

| Module | Schema | Description |
|--------|--------|-------------|
| **Users** | `identity` | Authentication (JWT), user management, RBAC, dynamic permission scopes |
| **Flights** | `flights` | Airlines, airports, airplanes, routes, flights, seat maps |
| **Bookings** | `bookings` | Booking lifecycle, passengers, payments, e-tickets |
| **Promotions** | `promotions` | Coupons (percentage/fixed), campaigns (admin & partner scoped) |
| **Interactions** | `interactions` | AI Travel Assistant (NVIDIA API + DeepSeek V4 Flash), reviews |
| **CMS** | `cms` | Admin/Partner dashboards, system settings, articles |
| **Logs** | `logs` | System logs, partner activity logs (paginated) |
| **Notifications** | `notifications` | Email, SMS, Push notification templates and delivery |

### BuildingBlocks (Shared Infrastructure)

| Component | Purpose |
|-----------|---------|
| **CQRS** | `ICommand`/`IQuery` + `ICommandHandler`/`IQueryHandler` via MediatR |
| **LoggingBehavior** | Auto-logs MediatR request/response |
| **CachingBehavior** | Auto-caches query results based on query attributes |
| **ValidationBehavior** | Auto-validates input via FluentValidation before handler execution |
| **CorrelationMiddleware** | Generates/propagates `X-Correlation-ID` for request tracing |
| **RequestResponseLoggingMiddleware** | Logs HTTP request/response details |
| **JWT Auth** | Bearer token authentication, supports SignalR query string tokens |
| **DynamicPermissionPolicyProvider** | Dynamic authorization based on permission codes |
| **AirlineResourceHandler** | Ensures Partner/Staff can only access their own airline's resources |
| **ICacheService** | Abstraction over Redis Cloud with in-memory fallback |

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

## Database

**SQL Server** with 8 schemas as bounded contexts:
- `identity`, `flights`, `bookings`, `promotions`, `interactions`, `cms`, `logs`, `notifications`

### Key Design Decisions
- **Soft Delete** — `IsDeleted` column + SQL Server `INSTEAD OF DELETE` triggers on all major tables
- **Performance Indexes** — On `DepartureTime`, `RouteId`, `PnrCode`, `TicketNumber`, `UserId`
- **UUID Primary Keys** — `UNIQUEIDENTIFIER` for all entity IDs

## API Endpoints

The system exposes **100+ REST API endpoints** across 12 sections:

1. **Authentication** — Login, Register, Me
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
- SQL Server 2022+
- Redis (optional — falls back to in-memory cache)
- EF Core CLI: `dotnet tool install --global dotnet-ef`

### Configuration

```bash
# Required secrets
dotnet user-secrets set "Jwt:Secret" "your-secret-key-minimum-32-characters"
dotnet user-secrets set "LuckyPenny:MediatR:LicenseKey" "your-mediatr-license-key"
dotnet user-secrets set "AiService:ModelStore:ApiKey" "your-nvidia-api-key"
```

Update `src/Api/AirlineTicket.Api/appsettings.json` with your SQL Server connection string.

### Run

```bash
# Apply database migrations
./run_ef.sh

# Start the API
dotnet run --project src/Api/AirlineTicket.Api/AirlineTicket.Api.csproj
```

API available at `http://localhost:5179`. Scalar docs at `/scalar/v1`.

### Docker

```bash
# Local (API + SQL Server)
docker-compose -f deploy/docker/docker-compose.local.yml up -d --build

# Development
docker-compose -f deploy/docker/docker-compose.dev.yml up -d --build

# Production (with Nginx reverse proxy)
docker-compose -f deploy/docker/docker-compose.prod.yml up -d --build
```

### Environment Variables (Docker)

| Variable | Description |
|----------|-------------|
| `DB_CONNECTION_STRING` | SQL Server connection string |
| `REDIS_CONNECTION_STRING` | Redis connection string |
| `CORS_ORIGINS` | Allowed CORS origins (frontend URL) |
| `AI_SERVICE_API_KEY` | NVIDIA API key |
| `AI_SERVICE_BASE_URL` | NVIDIA API base URL |
| `AI_SERVICE_MODEL` | AI model name (e.g., DeepSeek V4 Flash) |
| `LUCKY_PENNY_MEDIATR_LICENSE_KEY` | MediatR license key |

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
│   │   └── Realtime/                    # SignalR Hubs
│   ├── BuildingBlock/                   # Shared infrastructure (4 projects)
│   └── Modules/v1/                      # 8 Business modules (4 layers each)
│       ├── Bookings/
│       ├── CMS/
│       ├── Flights/
│       ├── Interactions/
│       ├── Logs/
│       ├── Notifications/
│       ├── Promotions/
│       └── Users/
├── database/                            # SQL scripts
│   ├── init_v1.sql                      # Schema + table creation
│   ├── seed_v1.sql                      # Base seed data
│   ├── seed_aircraft_models.sql         # Aircraft models & seat templates seed data
│   ├── seed_routes_flights.sql          # Route & flight seed data
│   ├── seed_extra_data.sql              # Additional seed data
│   └── triggers_soft_delete_v1.sql      # Soft delete triggers
├── deploy/                              # Docker & Nginx configs
├── docs/                                # Documentation
│   ├── api/api-list.md                  # Complete API endpoint reference
│   ├── database/database_design.md      # Full database schema documentation
│   └── plan/                            # Project planning docs
└── tests/                               # Unit & Integration tests
