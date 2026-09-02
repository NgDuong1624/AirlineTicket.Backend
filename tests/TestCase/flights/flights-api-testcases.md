# Flights Module API Test Cases

## Module Overview
- **Base Paths**: `/api/airports`, `/api/airlines`, `/api/routes`, `/api/flights`, `/api/staff/flights`, `/api/partner/*`, `/api/admin/*`
- **Authentication**: Public, PartnerOrStaff, PartnerOnly, AdminOnly
- **Enums**: FlightStatus (`0`: Scheduled, `1`: Delayed, `2`: Boarding, `3`: InAir, `4`: Landed, `5`: Cancelled), SeatClass (`0`: Economy, `1`: PremiumEconomy, `2`: Business, `3`: First)

---

## Checklist Summary

- [ ] **GET /api/airports**
  - [ ] `TC-FLT-AIRP-001`: Public list all airports returns 200 OK
  - [ ] `TC-FLT-AIRP-002`: Search airport by IATA code or city name returns matched list
  - [ ] `TC-FLT-AIRP-003`: Inactive/deleted airports excluded from standard list

- [ ] **GET /api/airlines**
  - [ ] `TC-FLT-AIRL-001`: Public list registered active airlines returns 200 OK

- [ ] **GET /api/routes**
  - [ ] `TC-FLT-ROUT-001`: Public list all flight routes returns 200 OK with origin and destination airport details

- [ ] **POST /api/flights** (Search one-way)
  - [ ] `TC-FLT-SRCH-001`: Search one-way flights with valid criteria returns 200 OK with flights array
  - [ ] `TC-FLT-SRCH-002`: Search with non-existent origin/destination returns 200 OK with empty array
  - [ ] `TC-FLT-SRCH-003`: Search with past departure date returns 400 Bad Request
  - [ ] `TC-FLT-SRCH-004`: Filter by cabinClass, airline, and priceRange applies correct filtering

- [ ] **POST /api/flights/round-trip**
  - [ ] `TC-FLT-RND-001`: Search round-trip flights returns outbound and inbound arrays
  - [ ] `TC-FLT-RND-002`: Return date earlier than departure date returns 400 Bad Request

- [ ] **GET /api/flights/{id}**
  - [ ] `TC-FLT-GET-001`: Valid flight GUID returns flight details with status
  - [ ] `TC-FLT-GET-002`: Invalid flight GUID format returns 400 Bad Request
  - [ ] `TC-FLT-GET-003`: Non-existent flight GUID returns 404 Not Found

- [ ] **GET /api/flights/trending**
  - [ ] `TC-FLT-TRND-001`: Retrieve trending/cheapest flights returns 200 OK list

- [ ] **GET /api/flights/{id}/seats**
  - [ ] `TC-FLT-SEAT-001`: Retrieve seat map for flight returns list with availability and price overrides
  - [ ] `TC-FLT-SEAT-002`: Non-existent flight returns 404 Not Found

- [ ] **GET /api/flights/price-forecast**
  - [ ] `TC-FLT-FCST-001`: Query forecast with valid airports & date returns trend and recommendation
  - [ ] `TC-FLT-FCST-002`: Missing mandatory query params returns 400 Bad Request

- [ ] **POST /api/flights/admin** (PartnerOrStaff create flight)
  - [ ] `TC-FLT-STF-001`: Staff creates flight on assigned route returns 201 Created and seeds FlightSeats
  - [ ] `TC-FLT-STF-002`: Staff creates flight with arrival time before departure time returns 400 Bad Request
  - [ ] `TC-FLT-STF-003`: Unauthenticated/Customer role returns 401/403

- [ ] **GET /api/staff/flights**
  - [ ] `TC-FLT-STF-004`: Staff lists flights with remaining seat counts returns 200 OK
  - [ ] `TC-FLT-STF-005`: Customer role access returns 403 Forbidden

- [ ] **GET /api/staff/flights/{id}/seats**
  - [ ] `TC-FLT-STF-006`: Staff gets flight seat map returns 200 OK

- [ ] **Partner Routes & Airplanes Management**
  - [ ] `TC-FLT-PART-001`: Partner lists own routes `GET /api/partner/routes` returns 200 OK
  - [ ] `TC-FLT-PART-002`: Partner creates route `POST /api/partner/routes` returns 201 Created
  - [ ] `TC-FLT-PART-003`: Partner updates route `PUT /api/partner/routes/{id}` returns 200 OK
  - [ ] `TC-FLT-PART-004`: Partner deletes route `DELETE /api/partner/routes/{id}` returns 200/204
  - [ ] `TC-FLT-PART-005`: Partner lists own airplanes `GET /api/partner/airplanes` returns 200 OK
  - [ ] `TC-FLT-PART-006`: Partner creates airplane `POST /api/partner/airplanes` returns 201 Created
  - [ ] `TC-FLT-PART-007`: Partner deletes airplane `DELETE /api/partner/airplanes/{id}` returns 200/204
  - [ ] `TC-FLT-PART-008`: Partner lists aircraft models `GET /api/partner/airplanes/models` returns 200 OK

- [ ] **Partner Flights Management**
  - [ ] `TC-FLT-PART-009`: Partner lists own flights `GET /api/partner/flights` returns 200 OK
  - [ ] `TC-FLT-PART-010`: Partner creates flight `POST /api/partner/flights` auto-assigns partner AirlineId returns 201
  - [ ] `TC-FLT-PART-011`: Partner updates flight `PUT /api/partner/flights/{id}` returns 200 OK
  - [ ] `TC-FLT-PART-012`: Partner deletes flight `DELETE /api/partner/flights/{id}` returns 200/204
  - [ ] `TC-FLT-PART-013`: Partner gets settings `GET /api/partner/settings` returns 200 OK
  - [ ] `TC-FLT-PART-014`: Partner updates settings `PUT /api/partner/settings` returns 200 OK

- [ ] **Admin Airport & Airline Management**
  - [ ] `TC-FLT-ADM-001`: Admin lists paginated airports `GET /api/admin/airports` returns 200 OK
  - [ ] `TC-FLT-ADM-002`: Admin creates airport `POST /api/admin/airports` returns 201 Created
  - [ ] `TC-FLT-ADM-003`: Admin creates airport with duplicate IATA code returns 409 Conflict
  - [ ] `TC-FLT-ADM-004`: Admin updates airport `PUT /api/admin/airports/{id}` returns 200 OK
  - [ ] `TC-FLT-ADM-005`: Admin deletes airport `DELETE /api/admin/airports/{id}` returns 200/204
  - [ ] `TC-FLT-ADM-006`: Admin lists paginated airlines `GET /api/admin/airlines` returns 200 OK
  - [ ] `TC-FLT-ADM-007`: Admin creates airline `POST /api/admin/airlines` returns 201 Created
  - [ ] `TC-FLT-ADM-008`: Admin updates airline `PUT /api/admin/airlines/{id}` returns 200 OK
  - [ ] `TC-FLT-ADM-009`: Admin deletes airline `DELETE /api/admin/airlines/{id}` returns 200/204

- [ ] **Admin Flights & Aircraft Models Management**
  - [ ] `TC-FLT-ADM-010`: Admin lists all flights `GET /api/admin/flights` returns 200 OK
  - [ ] `TC-FLT-ADM-011`: Admin creates flight with seat generation `POST /api/admin/flights` returns 201 Created
  - [ ] `TC-FLT-ADM-012`: Admin updates flight `PUT /api/admin/flights/{id}` returns 200 OK
  - [ ] `TC-FLT-ADM-013`: Admin deletes flight `DELETE /api/admin/flights/{id}` returns 200/204
  - [ ] `TC-FLT-ADM-014`: Admin lists aircraft models `GET /api/admin/aircraft-models` returns 200 OK
  - [ ] `TC-FLT-ADM-015`: Admin creates aircraft model with seat templates `POST /api/admin/aircraft-models` returns 201
  - [ ] `TC-FLT-ADM-016`: Admin updates aircraft model `PUT /api/admin/aircraft-models/{id}` returns 200 OK
  - [ ] `TC-FLT-ADM-017`: Admin deletes aircraft model `DELETE /api/admin/aircraft-models/{id}` returns 200/204

---

## Detailed Test Case Specifications

### TC-FLT-SRCH-001: Search one-way flights
- **Endpoint**: `POST /api/flights`
- **Auth**: None
- **Preconditions**: Flights exist from HAN to SGN on date `2026-09-15`.
- **Request Body**:
  ```json
  {
    "from": "HAN",
    "to": "SGN",
    "departDate": "2026-09-15",
    "passengers": 1,
    "cabinClass": "Economy",
    "currency": "USD"
  }
  ```
- **Assertions**:
  - HTTP Status: `200 OK`
  - Response contains `flights` array.
  - Every flight has `originCode: "HAN"`, `destinationCode: "SGN"`, `basePrice > 0`.

### TC-FLT-SEAT-001: Get seat map for flight
- **Endpoint**: `GET /api/flights/{flightId}/seats`
- **Auth**: None
- **Preconditions**: Flight exists with generated seats.
- **Assertions**:
  - HTTP Status: `200 OK`
  - Response array contains items with `seatNumber`, `seatClass`, `isAvailable`.

### TC-FLT-FCST-001: Price forecast prediction
- **Endpoint**: `GET /api/flights/price-forecast?originAirportId={id1}&destinationAirportId={id2}&departureDate=2026-09-20`
- **Auth**: None
- **Assertions**:
  - HTTP Status: `200 OK`
  - Response contains `trend` (number), `recommendation` (number), `confidenceScore` (number between 0 and 1).

### TC-FLT-PART-010: Partner creates flight
- **Endpoint**: `POST /api/partner/flights`
- **Auth**: Partner JWT (AirlineId `A1`)
- **Request Body**:
  ```json
  {
    "routeId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "airplaneId": "7cb85f64-5717-4562-b3fc-2c963f66afa6",
    "flightNumber": "VN101",
    "departureTime": "2026-10-01T08:00:00Z",
    "arrivalTime": "2026-10-01T10:15:00Z",
    "basePrice": 120.00,
    "currency": "USD",
    "status": 0
  }
  ```
- **Assertions**:
  - HTTP Status: `201 Created`
  - Response contains generated flight ID and matches flight number `VN101`.
  - Seats are generated in `FlightSeats` table.

### TC-FLT-ADM-003: Duplicate airport IATA code
- **Endpoint**: `POST /api/admin/airports`
- **Auth**: Admin JWT
- **Request Body**:
  ```json
  {
    "iataCode": "HAN",
    "nameEn": "Noi Bai Duplicate",
    "nameVi": "Noi Bai Trung",
    "cityEn": "Hanoi",
    "cityVi": "Ha Noi",
    "countryCode": "VN",
    "timezone": "Asia/Ho_Chi_Minh",
    "isActive": true
  }
  ```
- **Assertions**:
  - HTTP Status: `409 Conflict` (or `400 Bad Request` duplicate key).
