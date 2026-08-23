# Microservice Migration & Permission Architecture Plan

## 1. Executive Summary
High-level strategy to decouple cross-module database queries, direct command invocations, and shared table dependencies into independent microservices utilizing an asynchronous, event-driven architecture (RabbitMQ/Kafka) with decentralized permissions.

---

## 2. Identified Coupling Points & Target Solutions

| Source Module | Direct Target | Identified Coupling | Target Event-Driven Solution |
| :--- | :--- | :--- | :--- |
| **CMS** | `Bookings`, `Users`, `Flights`, `Logs` | Direct SQL joins across schemas in `DashboardRepository`. | **Read Model / Analytics Service**: Consume domain events (`BookingCreated`, `UserRegistered`, `FlightScheduled`) into dedicated read models/Elasticsearch/ClickHouse. |
| **Bookings** | `Flights` | Direct join on `flights.Flight` and `flights.Route` in `RevenueRepository`. | **Event-driven Data Projection**: Subscribe to `FlightCreated` / `RouteCreated` to maintain denormalized airline/route IDs in Bookings read-side. |
| **Flights** | `Notifications` | Direct in-process invocation of `SendNotificationCommand`. | **Asynchronous Integration Event**: Publish `FlightCreatedIntegrationEvent` to message broker; `NotificationService` consumes independently. |
| **Users** | `Flights` | `UserDbContext` references `flights.airlines` for partner staff validation. | **Opaque Reference / Local Cache**: Store `AirlineId` as value property; replicate minimal airline metadata via `AirlineRegisteredIntegrationEvent` / Redis cache. |

---

## 3. Asynchronous Event-Driven Architecture

```
[ Clients / Web / Mobile ]
           │
     [ API Gateway ] (JWT Validation & Scope Enforcement)
           │
 ┌─────────┼───────────────────────┬──────────────────────┐
 ▼         ▼                       ▼                      ▼
[Users]  [Flights]            [Bookings]             [Notifications]
   │         │                     │                        ▲
   └─────────┼─────────────────────┼────────────────────────┘
             ▼                     ▼
     ===========================================
               Message Broker (RabbitMQ / Kafka)
     ===========================================
             │                     │
             ▼                     ▼
       [Notifications]      [Analytics / CMS Read-Model]
```

### Event Contracts (Integration Events)
- **`FlightCreatedIntegrationEvent`**: `(FlightId, FlightNumber, AirlineId, RouteId, DepartureTime)`
- **`BookingConfirmedIntegrationEvent`**: `(BookingId, UserId, FlightId, AirlineId, TotalPrice, ConfirmedAt)`
- **`UserRegisteredIntegrationEvent`**: `(UserId, Email, FullName, Role, AirlineId)`
- **`AirlineUpdatedIntegrationEvent`**: `(AirlineId, AirlineName, Status)`

---

## 4. Decentralized Permission & Security Model

### 4.1 Token Design (JWT Claims)
- **Issuer**: `Identity / Users Service`
- **Standard Claims**:
  - `sub`: User ID
  - `role`: Role name (`Admin`, `PartnerStaff`, `Customer`)
  - `airline_id`: Tenant/Airline ID (null for non-partner/admin)
  - `scp` (Scopes): Fine-grained functional permissions

### 4.2 Permission Mapping by Microservice

| Microservice | Scopes / Permissions | Authorization Rule |
| :--- | :--- | :--- |
| **Users Service** | `users:read`, `users:write`, `users:manage` | Validates admin/user self-management |
| **Flights Service** | `flights:read`, `flights:write`, `flights:partner` | Validates `airline_id` claim matches payload for partner operations |
| **Bookings Service** | `bookings:read`, `bookings:write`, `bookings:partner:analytics` | Enforces tenant isolation using token `airline_id` |
| **CMS / Analytics** | `cms:admin:read`, `cms:admin:write` | Restricted to `Role=Admin` and platform-level scopes |
| **Notifications** | `notifications:read`, `notifications:write` | Service-to-service execution via broker or mTLS client credentials |

### 4.3 Service-to-Service Security
- Internal broker topics protected by network isolation and SASL/mTLS credentials.
- Gateway handles perimeter authentication; downstream services trust validated identity claims passed via HTTP headers or MTLS.

---

## 5. High-Level Migration Phases

1. **Phase 1: Event Infrastructure Setup**
   - Establish RabbitMQ/Kafka brokers and integration event publishing pipeline (Transactional Outbox Pattern).
2. **Phase 2: Asynchronous Decoupling**
   - Replace in-process `Flights -> Notifications` calls with `FlightCreatedIntegrationEvent`.
3. **Phase 3: Database & Query Decoupling**
   - Replicate airline reference data in `Users` module via events.
   - Build read-model event projections for `Bookings` revenue and `CMS` dashboards.
4. **Phase 4: Permission & Gateway Enforcement**
   - Migrate authorization from database-driven checks to claim-based scope validation at API Gateway and service boundaries.
5. **Phase 5: Physical Service Extraction**
   - Extract independent module codebases into standalone deployable containers/services.
