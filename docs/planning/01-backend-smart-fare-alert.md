# Backend Planning: Smart Fare Alert & Real-time Price Drop Predictor (SkyWatch)

## 1. Overview
Full-stack backend architecture for route fare tracking, historical pricing capture, price drop evaluations via background worker, and real-time SignalR push notifications to connected clients.

---

## 2. Database Schema & Migrations

### 2.1 Schema: `promotions`

#### Table: `promotions.fare_alerts`
```sql
CREATE TABLE promotions.fare_alerts (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL REFERENCES users.users(id) ON DELETE CASCADE,
    origin_airport_id UUID NOT NULL REFERENCES flights.airports(id),
    destination_airport_id UUID NOT NULL REFERENCES flights.airports(id),
    departure_date DATE NOT NULL,
    return_date DATE NULL,
    target_price DECIMAL(18, 2) NOT NULL,
    current_lowest_price DECIMAL(18, 2) NOT NULL,
    last_notified_price DECIMAL(18, 2) NULL,
    currency VARCHAR(3) NOT NULL DEFAULT 'VND',
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    last_checked_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    last_notified_at TIMESTAMPTZ NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    CONSTRAINT uq_fare_alerts_user_route UNIQUE (user_id, origin_airport_id, destination_airport_id, departure_date)
);

CREATE INDEX idx_fare_alerts_active_eval 
ON promotions.fare_alerts(origin_airport_id, destination_airport_id, departure_date) 
WHERE is_active = TRUE;

CREATE INDEX idx_fare_alerts_user 
ON promotions.fare_alerts(user_id, is_active);
```

### 2.2 Schema: `flights`

#### Table: `flights.flight_price_histories`
```sql
CREATE TABLE flights.flight_price_histories (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    flight_id UUID NOT NULL REFERENCES flights.flights(id) ON DELETE CASCADE,
    route_id UUID NOT NULL REFERENCES flights.routes(id) ON DELETE CASCADE,
    price DECIMAL(18, 2) NOT NULL,
    seat_class VARCHAR(20) NOT NULL DEFAULT 'Economy',
    recorded_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_flight_price_history_route_date 
ON flights.flight_price_histories(route_id, recorded_at DESC);
```

---

## 3. Domain Layer

### 3.1 Entities & Enums
- **`FareAlert`** (Aggregate Root in Promotions Module):
  - Properties: `Id`, `UserId`, `OriginAirportId`, `DestinationAirportId`, `DepartureDate`, `ReturnDate`, `TargetPrice`, `CurrentLowestPrice`, `LastNotifiedPrice`, `Currency`, `IsActive`, `LastCheckedAt`, `LastNotifiedAt`.
  - Methods: `UpdateCurrentPrice(decimal newPrice)`, `Deactivate()`, `Activate()`, `RecordNotificationSent(decimal notifiedPrice)`.
- **`FlightPriceHistory`** (Entity in Flights Module):
  - Properties: `Id`, `FlightId`, `RouteId`, `Price`, `SeatClass`, `RecordedAt`.
- **Enums**:
  - `PriceTrendDirection`: `Falling = 1`, `Stable = 2`, `Rising = 3`.
  - `BuyRecommendation`: `BuyNow = 1`, `Wait = 2`, `FairPrice = 3`.

### 3.2 Domain Events
- `FareAlertCreatedDomainEvent(Guid AlertId, Guid UserId, decimal TargetPrice)`
- `FarePriceDroppedDomainEvent(Guid AlertId, Guid UserId, decimal OldPrice, decimal NewPrice, decimal TargetPrice)`

---

## 4. Application Layer (CQRS & MediatR)

### 4.1 Commands
| Command | Handler | Description |
| :--- | :--- | :--- |
| `CreateFareAlertCommand` | `CreateFareAlertCommandHandler` | Validates target price, checks duplicate alerts, saves to DB. |
| `UpdateFareAlertCommand` | `UpdateFareAlertCommandHandler` | Updates target price or toggle active status. |
| `DeleteFareAlertCommand` | `DeleteFareAlertCommandHandler` | Soft/hard deletes user fare alert. |
| `EvaluateFareAlertsCommand` | `EvaluateFareAlertsCommandHandler` | Batches active routes, evaluates price drops with 24h cooldown, records price history snapshots, and fires notifications. |

### 4.2 Queries
| Query | Handler | Return Type |
| :--- | :--- | :--- |
| `GetUserFareAlertsQuery` | `GetUserFareAlertsQueryHandler` | `List<FareAlertDto>` (includes 14-day lowest price trend points). |
| `GetRoutePriceForecastQuery` | `GetRoutePriceForecastQueryHandler` | `PriceForecastDto` (calls External ML Service for trend direction, buy recommendation, confidence score). |

### 4.3 FluentValidation Rules
- `CreateFareAlertCommandValidator`:
  - `OriginAirportId` != `DestinationAirportId`.
  - `DepartureDate` >= `Today`.
  - `TargetPrice` > 0.
  - Maximum 10 active alerts per customer.

---

## 5. Infrastructure & Real-Time

### 5.1 Background Service: `FareWatchBackgroundService`
- Inherits `BackgroundService`.
- Triggers on periodic cron/timer interval (configurable: default 15 minutes).
- **Execution Flow**:
  1. Creates scoped service provider.
  2. Queries distinct active route-date pairs from `promotions.fare_alerts`.
  3. Finds matching flight departures and calculates current lowest seat fare.
  4. Records price snapshot checkpoints into `flights.flight_price_histories`.
  5. Evaluates matching active `fare_alerts`:
     - Compares `CurrentLowestPrice` with `TargetPrice`.
     - Checks notification condition: `CurrentLowestPrice <= TargetPrice` AND (`LastNotifiedAt == null` OR `LastNotifiedAt < DateTime.UtcNow.AddHours(-24)` OR `CurrentLowestPrice < LastNotifiedPrice`).
  6. If condition met:
     - Updates `LastNotifiedPrice = CurrentLowestPrice` and `LastNotifiedAt = DateTime.UtcNow`.
     - Dispatches `FarePriceDroppedDomainEvent`.
     - Dispatches SignalR payload via `IFareAlertHubService`.

### 5.2 External ML Client: `IFarePredictionServiceClient`
- Calls external ML prediction service with historical route data.
- Returns `PriceForecastDto` containing `BuyRecommendation`, `PriceTrendDirection`, and `ConfidenceScore`.

### 5.2 SignalR Hub: `FareAlertHub`
- **Route**: `/hubs/fare-alerts` (requires JWT authentication).
- **Client Mapping**: User ID retrieved from `Context.UserIdentifier`.
- **Hub Methods**:
  - `SubscribeRoute(string routeKey)`: Adds connection to `route-{origin}-{dest}-{date}` group.
  - `UnsubscribeRoute(string routeKey)`: Removes connection.
- **Client Events Sent**:
  - `PriceDropped(FareAlertNotificationDto payload)`: Direct push to specific user.
  - `RoutePriceUpdated(RoutePriceUpdateDto payload)`: Broadcast to route group.

---

## 6. Minimal API Endpoints (`Promotions.Api`)

```csharp
// Fare Alerts Endpoints
app.MapPost("/api/fare-alerts", CreateFareAlert)
   .RequireAuthorization()
   .WithName("CreateFareAlert");

app.MapGet("/api/fare-alerts", GetUserFareAlerts)
   .RequireAuthorization()
   .WithName("GetUserFareAlerts");

app.MapPatch("/api/fare-alerts/{id:guid}", UpdateFareAlert)
   .RequireAuthorization()
   .WithName("UpdateFareAlert");

app.MapDelete("/api/fare-alerts/{id:guid}", DeleteFareAlert)
   .RequireAuthorization()
   .WithName("DeleteFareAlert");

app.MapGet("/api/flights/price-forecast", GetRoutePriceForecast)
   .WithName("GetRoutePriceForecast");
```

---

## 7. Execution Tasks

1. Create migration scripts for `fare_alerts` and `flight_price_histories`.
2. Implement Domain entities, events, and EF Core entity configurations.
3. Implement MediatR CQRS commands, queries, and validators.
4. Implement `FareAlertHub` and `IFareAlertHubService`.
5. Implement `FareWatchBackgroundService` worker and register in DI.
6. Register Minimal API endpoints and add unit/integration tests.
