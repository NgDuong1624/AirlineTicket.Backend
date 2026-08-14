# Flights Module

## Overview
The **Flights Module** is the core of the airline ticket system, managing all aspects related to flights, including airlines, airports, aircraft, routes, and flight schedules. It provides functionalities for searching flights, managing airline operations, and handling seat inventory.

---

## How It Works (Flight Lifecycle)
1. **Configuration**: System administrators and airline partners configure `Airlines`, `Airports`, `AircraftModels`, and `Routes`. `AircraftModels` define seat templates, which are used to generate `AirplaneSeats` when an `Airplane` is created.
2. **Flight Creation**: Airline partners create `Flights` based on defined `Routes` and available `Airplanes`. When a flight is created, `FlightSeats` are automatically generated based on the assigned `Airplane`'s seat configuration.
3. **Automated Status Updates**: Background jobs (`FlightStatusAutomatorJob`, `FlightDelayDetectorJob`) automatically update flight statuses (e.g., `Scheduled` -> `Boarding` -> `InAir` -> `Landed` -> `Delayed`) based on departure/arrival times.
4. **Search & Booking**: Customers search for flights using various criteria. The module provides APIs for searching one-way and round-trip flights, retrieving flight details, and viewing seat maps. Seat reservation is handled by `IFlightSeatRepository` which is consumed by the Bookings module.
5. **Dynamic Pricing (Future)**: A `DynamicPricingJob` is a placeholder for future implementation of dynamic pricing based on factors like load factor and demand.
6. **Cleanup**: `FlightCleanupBackgroundService` ensures that flights are correctly marked as `Landed` or `Cancelled` and performs other maintenance tasks.

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
- `ApiEndpoint` (string?): B2B integration endpoint.
- `ApiKey` (string?): API key for B2B integration.
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
- `AircraftModelId` (Guid?): FK to the `AircraftModel` it's based on.
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
- `Currency` (string): Currency code (default: `USD`).
- `Status` (FlightStatus): `0` = Scheduled, `1` = Delayed, `2` = Boarding, `3` = InAir, `4` = Landed, `5` = Cancelled.
- `ExternalId` (string?): External system ID.
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

---

## API Reference

### Authentication Roles
- **PartnerOrStaff**: Requires authentication as an Airline Admin, Airline Staff, or System Admin.
- **PartnerOnly**: Requires authentication as an Airline Admin.
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
| **GET** | `/api/airports` | Search and list airports. | None (Query param `search?: string`) | `Airport[] { id: string, iataCode: string, nameEn: string, nameVi: string, cityEn: string, cityVi: string, countryCode: string, timezone: string, latitude?: number, longitude?: number, isActive: boolean }` |
| **GET** | `/api/airlines` | List all registered airlines. | None | `{ items: Airline[] { id: string, name: string, iataCode?: string, logoUrl?: string, baseCountry?: string } }` |
| **GET** | `/api/routes` | List all flight routes. | None | `Route[] { id: string, originAirportId: string, destinationAirportId: string, originAirport: Airport, destinationAirport: Airport, distanceKm?: number, estimatedDurationMinutes?: number }` |
| **POST** | `/api/flights` | Search for one-way flights based on origin, destination, date, etc. | `SearchFlightsBody { from: string, to: string, departDate: string, returnDate?: string, passengers: number, cabinClass: string, airlines?: string[], priceRange?: number, stops?: string, sortBy?: string, currency?: string }` | `{ flights: FlightResponse[] { id: string, routeId: string, airplaneId: string, flightNumber: string, departureTime: string, arrivalTime: string, basePrice: number, currency: string, status: number, originCode?: string, destinationCode?: string, airlineName?: string, externalId?: string } }` |
| **POST** | `/api/flights/round-trip` | Search for round-trip flights. | `SearchFlightsBody` | `{ outbound: FlightResponse[], inbound: FlightResponse[] }` |
| **GET** | `/api/flights/{id:guid}` | Get detailed information for a specific flight. | None | `FlightResponse` |
| **GET** | `/api/flights/trending` | Get a list of trending (e.g., cheapest) flights. | None | `FlightResponse[]` |
| **GET** | `/api/flights/{id:guid}/seats` | Get the seat map and availability for a specific flight. | None | `FlightSeat[] { id: string, flightId: string, seatNumber: string, seatClass: number, priceOverride?: number, isAvailable: boolean, isExtraLegroom: boolean }` |

#### Staff Endpoints (`PartnerOrStaff` Role)
| Method | Path | Description | Input Type | Output Type |
|--------|------|-------------|------------|-------------|
| **POST** | `/api/flights/admin` | Create a new flight. | `FlightAdminPayload { routeId: string, airplaneId: string, flightNumber: string, departureTime: string, arrivalTime?: string, basePrice: number, currency: string, status: number }` | `SystemFlight { id: string, routeId: string, airplaneId: string, flightNumber: string, departureTime: string, arrivalTime: string, basePrice: number, currency: string, status: number }` |
| **GET** | `/api/staff/flights` | List flights with seat availability for staff. | None (Query param `search?: string`) | `StaffFlightListItem[] { id: string, flightNumber: string, originCode: string, destinationCode: string, departureTime: string, arrivalTime: string, basePrice: number, currency: string, status: string, totalSeats: number, availableSeats: number }` |
| **GET** | `/api/staff/flights/{id:guid}/seats` | Get seat map for a specific flight (staff view). | None | `FlightSeat[]` |

#### Partner Endpoints (`PartnerOnly` Role)
| Method | Path | Description | Input Type | Output Type |
|--------|------|-------------|------------|-------------|
| **GET** | `/api/partner/routes` | List routes managed by the partner's airline. | None (Query params `pageIndex: number, pageSize: number`) | `PagedResult<Route> { items: Route[], totalCount: number, pageNumber: number, pageSize: number, totalPages: number, hasNextPage: boolean, hasPreviousPage: boolean }` |
| **POST** | `/api/partner/routes` | Create a new route for the partner's airline. | `Partial<Route> { originAirportId: string, destinationAirportId: string, distanceKm?: number, estimatedDurationMinutes?: number }` | `Route` |
| **PUT** | `/api/partner/routes/{id:guid}` | Update an existing route. | `Partial<Route>` | `Route` |
| **DELETE** | `/api/partner/routes/{id:guid}` | Delete a route. | None | `void` |
| **GET** | `/api/partner/airplanes` | List airplanes owned by the partner's airline. | None (Query params `pageIndex: number, pageSize: number`) | `PagedResult<Airplane> { items: Airplane[], totalCount: number, pageNumber: number, pageSize: number, totalPages: number, hasNextPage: boolean, hasPreviousPage: boolean }` |
| **GET** | `/api/partner/airplanes/models` | List available aircraft models for creating airplanes. | None (Query params `pageIndex: number, pageSize: number`) | `PagedResult<AircraftModel> { items: AircraftModel[], totalCount: number, pageNumber: number, pageSize: number, totalPages: number, hasNextPage: boolean, hasPreviousPage: boolean }` |
| **POST** | `/api/partner/airplanes` | Create a new airplane. | `Partial<Airplane> { model: string, registrationNumber: string, totalCapacity: number }` | `Airplane` |
| **PUT** | `/api/partner/airplanes/{id:guid}` | Update an airplane. | `Partial<Airplane>` | `Airplane` |
| **DELETE** | `/api/partner/airplanes/{id:guid}` | Delete an airplane. | None | `void` |
| **GET** | `/api/partner/flights` | List flights operated by the partner's airline. | None (Query params `pageIndex: number, pageSize: number`) | `PagedResult<SystemFlight>` |
| **POST** | `/api/partner/flights` | Create a new flight for the partner's airline. | `FlightAdminPayload` | `SystemFlight` |
| **PUT** | `/api/partner/flights/{id:guid}` | Update an existing flight. | `Partial<FlightAdminPayload>` | `SystemFlight` |
| **DELETE** | `/api/partner/flights/{id:guid}` | Delete a flight. | None | `void` |
| **GET** | `/api/partner/aircraft` | Alias for listing partner's airplanes. | None (Query params `pageIndex: number, pageSize: number`) | `PagedResult<Airplane>` |
| **POST** | `/api/partner/aircraft` | Create an aircraft (airplane). | `Partial<Airplane>` | `Airplane` |
| **PUT** | `/api/partner/aircraft/{id:guid}` | Update an aircraft. | `Partial<Airplane>` | `Airplane` |
| **DELETE** | `/api/partner/aircraft/{id:guid}` | Delete an aircraft. | None | `void` |
| **GET** | `/api/partner/settings` | Get the partner airline's profile information. | None | `PartnerSettings { companyName?: string, contactEmail?: string, address?: string, airlineName?: string, supportEmail?: string, supportPhone?: string }` |
| **PUT** | `/api/partner/settings` | Update the partner airline's profile information. | `PartnerSettings` | `PartnerSettings` |

#### Admin Endpoints (`AdminOnly` Role)
| Method | Path | Description | Input Type | Output Type |
|--------|------|-------------|------------|-------------|
| **GET** | `/api/admin/airports` | Paginated list of all airports with search capabilities. | None (Query params `pageIndex: number, pageSize: number`) | `PagedResult<Airport>` |
| **POST** | `/api/admin/airports` | Create a new airport. | `Partial<Airport> { iataCode: string, nameEn: string, nameVi: string, cityEn: string, cityVi: string, countryCode: string, timezone: string, latitude?: number, longitude?: number, isActive: boolean }` | `Airport` |
| **PUT** | `/api/admin/airports/{id:guid}` | Update an airport. | `Partial<Airport>` | `Airport` |
| **DELETE** | `/api/admin/airports/{id:guid}` | Soft-delete an airport. | None | `void` |
| **GET** | `/api/admin/airlines` | Paginated list of all airlines. | None (Query params `pageIndex: number, pageSize: number`) | `PagedResult<Airline>` |
| **POST** | `/api/admin/airlines` | Create a new airline. | `Partial<Airline> { iataCode: string, name: string, logoUrl?: string, baseCountry?: string, apiEndpoint?: string, apiKey?: string, address?: string, supportEmail?: string, supportPhone?: string, isActive: boolean }` | `Airline` |
| **PUT** | `/api/admin/airlines/{id:guid}` | Update an airline. | `Partial<Airline>` | `Airline` |
| **DELETE** | `/api/admin/airlines/{id:guid}` | Soft-delete an airline. | None | `void` |
| **GET** | `/api/admin/flights` | Paginated list of all flights. | None (Query params `pageIndex: number, pageSize: number`) | `PagedResult<FlightLog> { items: FlightLog[], totalCount: number, pageNumber: number, pageSize: number, totalPages: number, hasNextPage: boolean, hasPreviousPage: boolean }` |
| **POST** | `/api/admin/flights` | Create a new flight (with seat seeding). | `FlightAdminPayload` | `SystemFlight` |
| **PUT** | `/api/admin/flights/{id:guid}` | Update a flight. | `Partial<FlightAdminPayload>` | `SystemFlight` |
| **DELETE** | `/api/admin/flights/{id:guid}` | Soft-delete a flight. | None | `void` |
| **GET** | `/api/admin/aircraft-models` | List all aircraft models. | None (Query params `pageIndex: number, pageSize: number`) | `PagedResult<AircraftModel> { items: AircraftModel[], totalCount: number, pageNumber: number, pageSize: number, totalPages: number, hasNextPage: boolean, hasPreviousPage: boolean }` |
| **GET** | `/api/admin/aircraft-models/{id:guid}` | Get details of a specific aircraft model. | None | `AircraftModel { id: string, name: string, manufacturer: string, totalSeats: number }` |
| **POST** | `/api/admin/aircraft-models` | Create a new aircraft model (with seat templates). | `Partial<AircraftModel> { name: string, manufacturer: string, totalSeats: number }` | `AircraftModel` |
| **PUT** | `/api/admin/aircraft-models/{id:guid}` | Update an aircraft model (replaces seat templates). | `Partial<AircraftModel>` | `AircraftModel` |
| **DELETE** | `/api/admin/aircraft-models/{id:guid}` | Soft-delete an aircraft model. | None | `void` |

---

## Background Services

The Flights module includes several background services to automate flight operations:

- **`FlightStatusAutomatorJob`**: Updates flight statuses (e.g., from `Scheduled` to `Boarding`, `InAir`, `Landed`) based on real-time clock and scheduled times.
- **`FlightDelayDetectorJob`**: Identifies flights that have passed their scheduled departure time but are still marked as `Scheduled` or `Boarding`, and automatically sets their status to `Delayed`.
- **`CloseFlightSalesJob`**: (Stub) Intended to identify flights nearing departure to close ticket sales.
- **`CheckInReminderJob`**: (Stub) Intended to send check-in reminders to passengers for upcoming flights.
- **`DynamicPricingJob`**: (Stub) Placeholder for implementing dynamic pricing algorithms based on factors like demand and load factor.
- **`FlightCleanupBackgroundService`**: Periodically cleans up flight data, ensuring flights past their arrival time are marked as `Landed`.
- **`FlightGenerationBackgroundService`**: Automatically generates new flights for upcoming days based on existing routes and aircraft, ensuring a continuous supply of flight data.