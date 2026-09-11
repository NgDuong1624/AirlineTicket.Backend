# Flights Module

## Overview
The **Flights Module** is the core of the airline ticket system, managing all aspects related to flights, including airlines, airports, aircraft models, physical airplanes, routes, flight schedules, and real-time flight radar tracking. It provides capabilities for searching flights, managing airline operations, handling seat inventory, and broadcasting live flight radar telemetry.

---

## How It Works (Flight Lifecycle)
1. **Configuration**: System administrators and airline partners configure `Airlines`, `Airports`, `AircraftModels`, and `Routes`. `AircraftModels` define seat templates, which are used to generate `AirplaneSeats` when an `Airplane` is created.
2. **Flight Creation**: Airline partners create `Flights` based on defined `Routes` and available `Airplanes`. When a flight is created, `FlightSeats` are automatically generated based on the assigned `Airplane`'s seat configuration.
3. **Automated Status Updates**: Background jobs (`FlightStatusAutomatorJob`, `FlightDelayDetectorJob`) automatically update flight statuses (e.g., `Scheduled` -> `Boarding` -> `InAir` -> `Landed` -> `Delayed`) based on departure/arrival times.
4. **Search & Booking**: Customers search for flights using various criteria. The module provides APIs for searching one-way and round-trip flights, retrieving flight details, and viewing seat maps. Seat reservation is handled by `IFlightSeatRepository` which is consumed by the Bookings module.
5. **Live Flight Radar & Telemetry**: Airborne flights stream GPS coordinates, altitude, speed, and heading. Users can view active flights on the global sky radar map (`GET /api/flights/radar/active`), inspect live telemetry (`GET /api/flights/{id}/telemetry`), or subscribe to WebSocket updates via `FlightTrackerHub` (`/hubs/flight-tracker`).
6. **Gate & Status Operations**: Airline staff and partners can update gate assignments, baggage carousels, delays, and flight statuses in real time via `PATCH /api/flights/{id}/status-gate`.
7. **Cleanup**: `FlightCleanupBackgroundService` ensures that flights are correctly marked as `Landed` or `Cancelled` and performs other maintenance tasks.

---

## Domain Entities & Data Model

### Airport
Represents an airport with its IATA code, location, and timezone.
- `Id` (Guid): Unique identifier.
- `IataCode` (string): Unique 3-letter IATA code (e.g., `HAN`, `SGN`).
- `NameEn` (string): English name.
- `NameVi` (string): Vietnamese name.
- `CityEn` (string): English city name.
- `CityVi` (string): Vietnamese city name.
- `CountryCode` (string): 2-letter country code.
- `Timezone` (string): IANA timezone name (e.g., `Asia/Ho_Chi_Minh`).
- `Latitude` (decimal?): Geographical latitude.
- `Longitude` (decimal?): Geographical longitude.
- `IsActive` (bool): Whether the airport is active.
- `IsDeleted` (bool): Soft delete flag.

### Airline
Represents an airline operating flights.
- `Id` (Guid): Unique identifier.
- `IataCode` (string): Unique 2-letter IATA code (e.g., `VN`, `VJ`).
- `Name` (string): Airline name.
- `LogoUrl` (string?): URL to the airline's logo.
- `BaseCountry` (string?): Country of origin.
- `Address` (string?): Airline's physical address.
- `SupportEmail` (string?): Support email.
- `SupportPhone` (string?): Support phone number.
- `IsActive` (bool): Whether the airline is active.
- `IsDeleted` (bool): Soft delete flag.
- `CreatedAt` (DateTime): UTC timestamp of creation.

### AircraftModel
Defines a type of aircraft and its default seat configuration.
- `Id` (Guid): Unique identifier.
- `Name` (string): Model name (e.g., `Boeing 787-9 Dreamliner`).
- `Manufacturer` (string): Manufacturer (e.g., `Boeing`, `Airbus`).
- `TotalSeats` (int): Total number of seats.
- `IsDeleted` (bool): Soft delete flag.

### AircraftModelSeatTemplate
Defines a seat within an `AircraftModel` for generating `AirplaneSeats`.
- `Id` (Guid): Unique identifier.
- `AircraftModelId` (Guid): FK to the `AircraftModel`.
- `SeatNumber` (string): Seat identifier (e.g., `01A`, `10B`).
- `SeatRow` (string): Row number.
- `SeatColumn` (string): Column letter.
- `SeatClass` (SeatClass): `0` = Economy, `1` = PremiumEconomy, `2` = Business, `3` = First.
- `IsExtraLegroom` (bool): Whether the seat has extra legroom.
- `PriceMultiplier` (decimal): Multiplier for base price.

### Airplane
Represents a specific physical aircraft owned by an `Airline`.
- `Id` (Guid): Unique identifier.
- `AirlineId` (Guid): FK to the owning `Airline`.
- `AircraftModelId` (Guid?): FK to the `AircraftModel` it is based on.
- `Model` (string): Specific model name.
- `RegistrationNumber` (string): Unique registration number (e.g., `VN-A861`).
- `TotalCapacity` (int): Total passenger capacity.
- `IsDeleted` (bool): Soft delete flag.

### AirplaneSeat
Represents a specific seat on a physical `Airplane`.
- `Id` (Guid): Unique identifier.
- `AirplaneId` (Guid): FK to the `Airplane`.
- `SeatNumber` (string): Seat identifier.
- `SeatRow` (string): Row number.
- `SeatColumn` (string): Column letter.
- `SeatClass` (SeatClass): Seat class.
- `IsExtraLegroom` (bool): Whether the seat has extra legroom.
- `PriceMultiplier` (decimal): Multiplier for base price.

### Route
Defines a flight path between two airports for a specific `Airline`.
- `Id` (Guid): Unique identifier.
- `AirlineId` (Guid): FK to the `Airline` operating the route.
- `OriginAirportId` (Guid): FK to the origin `Airport`.
- `DestinationAirportId` (Guid): FK to the destination `Airport`.
- `DistanceKm` (decimal?): Distance in kilometers.
- `EstimatedDurationMinutes` (int?): Estimated flight duration.
- `IsDeleted` (bool): Soft delete flag.

### Flight
Represents a scheduled flight instance.
- `Id` (Guid): Unique identifier.
- `RouteId` (Guid): FK to the `Route`.
- `AirplaneId` (Guid): FK to the `Airplane` used for this flight.
- `FlightNumber` (string): Unique flight number (e.g., `VN213`).
- `DepartureTime` (DateTime): Scheduled departure time.
- `ArrivalTime` (DateTime): Scheduled arrival time.
- `BasePrice` (decimal): Base price for the flight.
- `Currency` (string): Currency code (default: `VND`).
- `Status` (FlightStatus): `0` = Scheduled, `1` = Delayed, `2` = Boarding, `3` = InAir, `4` = Landed, `5` = Cancelled.
- `DepartureGate` (string?): Assigned departure gate.
- `ArrivalGate` (string?): Assigned arrival gate.
- `BaggageCarousel` (string?): Baggage claim carousel.
- `DelayMinutes` (int): Flight delay in minutes.
- `DelayReason` (string?): Reason for delay.
- `IsDeleted` (bool): Soft delete flag.
- `CreatedAt` (DateTime): UTC timestamp of creation.
- `UpdatedAt` (DateTime): UTC timestamp of last update.

### FlightSeat
Represents a specific seat on a scheduled `Flight`. This is the bookable unit.
- `Id` (Guid): Unique identifier.
- `FlightId` (Guid): FK to the `Flight`.
- `SeatNumber` (string): Seat identifier.
- `SeatClass` (SeatClass): Seat class.
- `PriceOverride` (decimal?): Specific price for this seat, overrides `PriceMultiplier`.
- `IsAvailable` (bool): Whether the seat is available for booking.
- `IsExtraLegroom` (bool): Whether the seat has extra legroom.

### FlightPriceHistory
Captures periodic price snapshots for trend evaluation and predictive analytics.
- `Id` (Guid): Unique identifier.
- `FlightId` (Guid): FK to `Flight`.
- `RouteId` (Guid): FK to `Route`.
- `Price` (decimal): Snapshot price.
- `SeatClass` (string): Cabin/seat class (e.g. `Economy`).
- `RecordedAt` (DateTime): UTC timestamp recorded.

---

## API Reference

### Authentication Roles
- **PartnerOrStaff**: Requires authentication as an Airline Partner, Airline Staff, or System Admin.
- **PartnerOnly**: Requires authentication as an Airline Partner.
- **AdminOnly**: Requires authentication as a System Admin.

### Enums

#### FlightStatus
- `0`: Scheduled
- `1`: Delayed
- `2`: Boarding
- `3`: InAir
- `4`: Landed
- `5`: Cancelled

#### SeatClass
- `0`: Economy
- `1`: PremiumEconomy
- `2`: Business
- `3`: First

### Endpoints

#### Public Endpoints (No Authentication Required)
| Method | Path | Description | Input Type | Output Type |
|--------|------|-------------|------------|-------------|
| **GET** | `/api/airports` | Search and list airports. | Query params `search?: string, pageIndex?: number, pageSize?: number` | `PagedResult<AirportDto>` |
| **GET** | `/api/airlines` | List all registered airlines. | None | `AirlineDto[]` |
| **GET** | `/api/routes` | List all flight routes. | Query params `pageIndex?: number, pageSize?: number` | `PagedResult<RouteDto>` |
| **POST** | `/api/flights` | Search for one-way flights by route, date, and filters. | `FlightSearchRequest { originCode: string, destinationCode: string, departDate: string, cabinClass?: string, airlines?: string[], priceRangeMin?: number, priceRangeMax?: number, maxStops?: number, sortBy?: string, currency?: string, pageIndex?: number, pageSize?: number }` | `PagedResult<FlightSearchResponseDto>` |
| **POST** | `/api/flights/round-trip` | Search for round-trip flights. | `RoundTripFlightSearchRequest { originCode: string, destinationCode: string, outboundDate: string, returnDate: string, cabinClass?: string, airlines?: string[], priceRangeMin?: number, priceRangeMax?: number, maxStops?: number, sortBy?: string, currency?: string }` | `RoundTripFlightSearchResponseDto` |
| **GET** | `/api/flights/{id}` | Get detailed information for a specific flight. | Route param `id: Guid` | `FlightDetailDto` |
| **GET** | `/api/flights/trending` | Get list of trending flights/routes. | Query params `pageIndex?: number, pageSize?: number` | `List<TrendingFlightDto>` |
| **GET** | `/api/flights/{id}/seats` | Get seat map and availability for a flight. | Route param `id: Guid` | `List<FlightSeatDto>` |
| **GET** | `/api/flights/radar/active` | Get active airborne flights for live sky radar map. | None | `List<AircraftMapPinDto>` |
| **GET** | `/api/flights/{id}/telemetry` | Get live flight telemetry and GPS coordinates. | Route param `id: Guid` | `FlightTelemetryDto` |
| **GET** | `/api/flights/status/{flightNumber}` | Get flight status, timeline, gates, carousel, delay by flight number. | Route param `flightNumber: string`, Query param `date?: DateTime` | `FlightStatusDetailDto` |

#### Staff Endpoints (`PartnerOrStaff` / `StaffOnly` Role)
| Method | Path | Role | Description | Input Type | Output Type |
|--------|------|------|-------------|------------|-------------|
| **GET** | `/api/staff/flights` | PartnerOrStaff | List flights with seat summary for staff portal. | Query params `search?: string, pageNumber?: number, pageIndex?: number, pageSize?: number` | `PagedResult<StaffFlightDto>` |
| **GET** | `/api/staff/flights/{id}/seats` | PartnerOrStaff | Get seat map for flight (staff view). | Route param `id: Guid` | `List<FlightSeatDto>` |
| **PATCH** | `/api/flights/{id}/status-gate` | PartnerOrStaff | Update flight status, gates, carousel, delay minutes. | `UpdateFlightStatusAndGateRequest { status?: FlightStatus, departureGate?: string, arrivalGate?: string, baggageCarousel?: string, delayMinutes?: int, reason?: string }` | `FlightStatusDetailDto` |

#### Partner Endpoints (`PartnerOnly` Role)
| Method | Path | Description | Input Type | Output Type |
|--------|------|-------------|------------|-------------|
| **GET** | `/api/partner/routes` | List routes managed by the partner's airline. | Query params `pageNumber?: number, pageIndex?: number, pageSize?: number` | `{ items: RouteDto[], totalCount: number }` |
| **POST** | `/api/partner/routes` | Create a new route for the partner's airline. | `PartnerRouteRequest { originAirportId: Guid, destinationAirportId: Guid, distanceKm?: decimal, estimatedDurationMinutes?: int }` | `{ id: Guid }` |
| **PUT** | `/api/partner/routes/{id}` | Update an existing route. | Route param `id: Guid`, `PartnerRouteRequest` | `200 OK` |
| **DELETE** | `/api/partner/routes/{id}` | Delete a route. | Route param `id: Guid` | `204 NoContent` |
| **GET** | `/api/partner/airplanes` | List airplanes owned by the partner's airline. | Query params `pageNumber?: number, pageIndex?: number, pageSize?: number` | `{ items: AirplaneDto[], totalCount: number }` |
| **GET** | `/api/partner/airplanes/models` | List available aircraft models. | Query params `pageNumber?: number, pageIndex?: number, pageSize?: number` | `PagedResult<AircraftModelDto>` |
| **POST** | `/api/partner/airplanes` | Create a new airplane. | `PartnerAirplaneRequest { aircraftModelId?: Guid, model: string, registrationNumber: string, totalCapacity: int }` | `{ id: Guid }` |
| **PUT** | `/api/partner/airplanes/{id}` | Update an airplane. | Route param `id: Guid`, `PartnerAirplaneRequest` | `200 OK` |
| **DELETE** | `/api/partner/airplanes/{id}` | Delete an airplane. | Route param `id: Guid` | `204 NoContent` |
| **GET** | `/api/partner/flights` | List flights operated by the partner's airline. | Query params `search?: string, status?: int, departureDate?: DateTime, pageNumber?: number, pageIndex?: number, pageSize?: number` | `{ items: FlightDto[], totalCount: number }` |
| **POST** | `/api/partner/flights` | Create a new flight for the partner's airline. | `PartnerFlightRequest { routeId: Guid, airplaneId: Guid, flightNumber: string, basePrice: decimal, departureTime: DateTime }` | `{ id: Guid }` |
| **PUT** | `/api/partner/flights/{id}` | Update an existing flight departure time. | Route param `id: Guid`, `PartnerFlightRequest` | `200 OK` |
| **DELETE** | `/api/partner/flights/{id}` | Delete a flight. | Route param `id: Guid` | `204 NoContent` |
| **GET** | `/api/partner/aircraft` | List aircraft configurations (partner). | Query params `pageNumber?: number, pageIndex?: number, pageSize?: number` | `{ items: AircraftDto[], totalCount: number }` |
| **POST** | `/api/partner/aircraft` | Create aircraft configuration. | None (Uses partner airline context) | `{ id: Guid }` |
| **PUT** | `/api/partner/aircraft/{id}` | Update aircraft configuration. | Route param `id: Guid` | `200 OK` |
| **DELETE** | `/api/partner/aircraft/{id}` | Delete aircraft configuration. | Route param `id: Guid` | `204 NoContent` |
| **GET** | `/api/partner/settings` | Get partner airline profile settings. | None | `{ airlineName?: string, address?: string, supportEmail?: string, supportPhone?: string }` |
| **PUT** | `/api/partner/settings` | Update partner airline profile settings. | `PartnerSettingsRequest { airlineName?: string, address?: string, supportEmail?: string, supportPhone?: string }` | `200 OK` |
| **POST** | `/api/flights/admin` | Create a new flight (legacy partner endpoint). | `CreateFlightRequest { routeId: Guid, airplaneId: Guid, flightNumber: string, basePrice: decimal, scheduledDeparture: DateTime, scheduledArrival: DateTime }` | `{ id: Guid }` |

#### Admin Endpoints (`AdminOnly` Role)
| Method | Path | Description | Input Type | Output Type |
|--------|------|-------------|------------|-------------|
| **GET** | `/api/admin/airports` | Paginated list of all airports. | Query params `search?: string, pageIndex?: number, pageSize?: number` | `PagedResult<AirportDto>` |
| **POST** | `/api/admin/airports` | Create a new airport. | `AdminAirportRequest { iataCode: string, nameEn: string, nameVi: string, cityEn: string, cityVi: string, countryCode: string, timezone: string, isActive?: bool }` | `{ id: Guid }` |
| **PUT** | `/api/admin/airports/{id}` | Update an airport. | Route param `id: Guid`, `AdminAirportRequest` | `200 OK` |
| **DELETE** | `/api/admin/airports/{id}` | Delete an airport. | Route param `id: Guid` | `204 NoContent` |
| **GET** | `/api/admin/airlines` | Paginated list of all airlines. | Query params `pageIndex?: number, pageSize?: number` | `PagedResult<AirlineDto>` |
| **POST** | `/api/admin/airlines` | Create a new airline. | `AdminAirlineRequest { iataCode: string, name: string, logoUrl?: string, baseCountry?: string, isActive?: bool }` | `{ id: Guid }` |
| **PUT** | `/api/admin/airlines/{id}` | Update an airline. | Route param `id: Guid`, `AdminAirlineRequest` | `200 OK` |
| **DELETE** | `/api/admin/airlines/{id}` | Delete an airline. | Route param `id: Guid` | `204 NoContent` |
| **GET** | `/api/admin/aircraft-models` | List all aircraft models. | Query params `pageIndex?: number, pageSize?: number` | `PagedResult<AircraftModelDto>` |
| **GET** | `/api/admin/aircraft-models/{id}` | Get details of a specific aircraft model. | Route param `id: Guid` | `AircraftModelDto` |
| **POST** | `/api/admin/aircraft-models` | Create a new aircraft model with seat templates. | `AdminAircraftModelRequest { name: string, manufacturer: string, totalSeats: int, seatTemplates: SeatTemplateDto[] }` | `{ id: Guid }` |
| **PUT** | `/api/admin/aircraft-models/{id}` | Update an aircraft model and its seat templates. | Route param `id: Guid`, `AdminAircraftModelRequest` | `200 OK` |
| **DELETE** | `/api/admin/aircraft-models/{id}` | Delete an aircraft model. | Route param `id: Guid` | `204 NoContent` |
| **GET** | `/api/admin/flights` | Paginated list of all flights. | Query params `pageIndex?: number, pageSize?: number` | `PagedResult<FlightDto>` |
| **POST** | `/api/admin/flights` | Create a new flight with automated seat generation. | `CreateFlightRequest { routeId: Guid, airplaneId: Guid, flightNumber: string, basePrice: decimal, scheduledDeparture: DateTime, scheduledArrival: DateTime }` | `{ id: Guid }` |
| **PUT** | `/api/admin/flights/{id}` | Update a flight. | Route param `id: Guid` | `200 OK` |
| **DELETE** | `/api/admin/flights/{id}` | Delete a flight. | Route param `id: Guid` | `204 NoContent` |

---

## Real-Time Flight Tracking (SignalR)

### Hub Endpoint
- **URL**: `/hubs/flight-tracker`
- **Authentication**: Anonymous

### Client → Server Methods
- `JoinFlightTracking(Guid flightId)`: Subscribe to real-time telemetry updates for a specific flight.
- `LeaveFlightTracking(Guid flightId)`: Unsubscribe from specific flight tracking.
- `JoinGlobalRadar()`: Subscribe to live global sky radar ticks.
- `LeaveGlobalRadar()`: Unsubscribe from global radar ticks.

### Server → Client Events
- `TelemetryUpdated(FlightTelemetryDto telemetry)`: Emitted when GPS coordinates, altitude, speed, or heading are updated.
- `GlobalRadarTick(List<AircraftMapPinDto> activePlanes)`: Emitted every 2 seconds with positions of all active airborne flights.
- `FlightStatusChanged(FlightStatusChangedDto statusUpdate)`: Emitted on flight status transitions (e.g. Delayed, Boarding, InAir, Landed).

---

## Background Services

- **`FlightStatusAutomatorJob`**: Updates flight statuses (`Scheduled` -> `Boarding` -> `InAir` -> `Landed`) based on departure and arrival times.
- **`FlightDelayDetectorJob`**: Detects flights past their scheduled departure that haven't boarded, automatically setting status to `Delayed`.
- **`FlightCleanupBackgroundService`**: Maintenance job ensuring flights past arrival time are marked as `Landed`.
- **`FlightRadarWorker`**: Periodically updates airborne aircraft positions and publishes telemetry events to Redis.
