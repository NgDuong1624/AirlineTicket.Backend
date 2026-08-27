# Backend Planning: Live Interactive Flight Radar & Real-Time Status Tracker (SkyRadar)

## 1. Overview
Real-time backend service for aircraft trajectory interpolation, flight stage progression, telemetry streaming, and automated gate/delay event notifications via SignalR and background workers.

---

## 2. Database Schema & Migrations

### 2.1 Schema: `flights`

#### Table: `flights.flight_telemetry`
```sql
CREATE TABLE flights.flight_telemetry (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    flight_id UUID NOT NULL REFERENCES flights.flights(id) ON DELETE CASCADE,
    latitude DOUBLE PRECISION NOT NULL,
    longitude DOUBLE PRECISION NOT NULL,
    altitude_feet INTEGER NOT NULL DEFAULT 0,
    speed_knots INTEGER NOT NULL DEFAULT 0,
    heading_degrees INTEGER NOT NULL DEFAULT 0,
    progress_percentage SMALLINT NOT NULL DEFAULT 0,
    estimated_arrival TIMESTAMPTZ NOT NULL,
    recorded_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_flight_telemetry_flight_time 
ON flights.flight_telemetry(flight_id, recorded_at DESC);
```

#### Alter Table: `flights.airports` (Add Coordinates if missing)
```sql
ALTER TABLE flights.airports 
ADD COLUMN IF NOT EXISTS latitude DOUBLE PRECISION,
ADD COLUMN IF NOT EXISTS longitude DOUBLE PRECISION,
ADD COLUMN IF NOT EXISTS timezone VARCHAR(50) DEFAULT 'UTC';
```

#### Alter Table: `flights.flights`
```sql
ALTER TABLE flights.flights 
ADD COLUMN IF NOT EXISTS departure_gate VARCHAR(10),
ADD COLUMN IF NOT EXISTS arrival_gate VARCHAR(10),
ADD COLUMN IF NOT EXISTS baggage_carousel VARCHAR(10),
ADD COLUMN IF NOT EXISTS actual_departure_time TIMESTAMPTZ,
ADD COLUMN IF NOT EXISTS actual_arrival_time TIMESTAMPTZ,
ADD COLUMN IF NOT EXISTS delay_minutes INTEGER DEFAULT 0;
```

---

## 3. Domain Layer

### 3.1 Entities & Value Objects
- **`FlightTelemetry`** (Entity in Flights Module):
  - Properties: `Id`, `FlightId`, `Latitude`, `Longitude`, `AltitudeFeet`, `SpeedKnots`, `HeadingDegrees`, `ProgressPercentage`, `EstimatedArrival`, `RecordedAt`.
- **`Coordinates`** (Value Object):
  - `Latitude`, `Longitude`.
  - Methods: `DistanceTo(Coordinates target)`, `BearingTo(Coordinates target)`.
- **Enums**:
  - `FlightStatus`: `Scheduled`, `CheckInOpen`, `Boarding`, `Departed`, `EnRoute`, `Approaching`, `Landed`, `ArrivedAtGate`, `Delayed`, `Cancelled`.

### 3.2 Domain Events
- `FlightDepartedDomainEvent(Guid FlightId, DateTime ActualDepartureTime)`
- `FlightLandedDomainEvent(Guid FlightId, DateTime ActualArrivalTime, string BaggageCarousel)`
- `FlightGateChangedDomainEvent(Guid FlightId, string OldGate, string NewGate, bool IsDeparture)`
- `FlightDelayedDomainEvent(Guid FlightId, int DelayMinutes, string Reason)`

---

## 4. Application Layer (CQRS & MediatR)

### 4.1 Commands & Handlers
| Command | Handler | Description |
| :--- | :--- | :--- |
| `UpdateFlightStatusAndGateCommand` | `UpdateFlightStatusAndGateCommandHandler` | Manual/Staff update for gate changes, delays, status changes. |
| `RecordFlightTelemetryBatchCommand` | `RecordFlightTelemetryBatchCommandHandler` | Stores calculated position batch into DB and cache. |

### 4.2 Queries & Handlers
| Query | Handler | Return Type |
| :--- | :--- | :--- |
| `GetLiveFlightTelemetryQuery` | `GetLiveFlightTelemetryQueryHandler` | `FlightTelemetryDto` (current position, route coordinates, heading, altitude). |
| `GetActiveAirborneFlightsQuery` | `GetActiveAirborneFlightsQueryHandler` | `List<ActiveFlightMapPinDto>` (all currently airborne flights for radar map). |
| `GetFlightStatusByNumberQuery` | `GetFlightStatusByNumberQueryHandler` | `FlightStatusDetailDto` (timeline, gates, baggage, delay predictions). |

---

## 5. Infrastructure & Real-Time (SignalR)

### 5.1 Background Service: `FlightSimulationTelemetryWorker`
- Runs continuously with `Task.Delay(3000, stoppingToken)`.
- **Interpolation Algorithm**:
  - Queries active flights with status `Departed` or `EnRoute`.
  - Calculates Great-Circle trajectory between Origin Airport and Destination Airport.
  - Computes current position based on elapsed time vs total flight duration.
  - Generates cruising altitude (e.g., 32,000–38,000 ft), speed (~450–520 knots), and heading angle.
  - Writes current snapshot to Redis Cloud cache (`flight:telemetry:{flightId}`).
  - Emits real-time batch update to `FlightTrackerHub`.

### 5.2 SignalR Hub: `FlightTrackerHub`
- **Route**: `/hubs/flight-tracker`
- **Group Management**:
  - `JoinFlightTracking(Guid flightId)`: Adds connection to group `flight-telemetry-{flightId}`.
  - `LeaveFlightTracking(Guid flightId)`: Removes connection.
  - `JoinGlobalRadar()`: Adds connection to `global-sky-radar` group.
- **Broadcast Events**:
  - `TelemetryUpdated(FlightTelemetryDto telemetry)` (Targeted to flight room).
  - `GlobalRadarTick(List<AircraftMapPinDto> activePlanes)` (Broadcast to global radar every 5s).
  - `FlightStatusChanged(FlightStatusChangedDto statusUpdate)` (Broadcast to flight room & booked users).

---

## 6. Minimal API Endpoints (`Flights.Api`)

```csharp
app.MapGet("/api/flights/radar/active", GetActiveAirborneFlights)
   .WithName("GetActiveAirborneFlights");

app.MapGet("/api/flights/{id:guid}/telemetry", GetLiveFlightTelemetry)
   .WithName("GetLiveFlightTelemetry");

app.MapGet("/api/flights/status/{flightNumber}", GetFlightStatusByNumber)
   .WithName("GetFlightStatusByNumber");

app.MapPatch("/api/flights/{id:guid}/status-gate", UpdateFlightStatusAndGate)
   .RequireAuthorization("StaffOrAdminPolicy")
   .WithName("UpdateFlightStatusAndGate");
```

---

## 7. Execution Tasks
1. Write EF Core migrations for `flight_telemetry` table and airport coordinates.
2. Implement Great-Circle calculation utilities in `Domain/Common/GeoMath.cs`.
3. Implement `FlightTrackerHub` and `IFlightTrackerHubService`.
4. Build `FlightSimulationTelemetryWorker` with Redis caching.
5. Create Minimal API endpoints and add unit tests for flight progress interpolation.
