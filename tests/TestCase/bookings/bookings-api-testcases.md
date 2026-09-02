# Bookings Module API Test Cases

## Module Overview
- **Base Paths**: `/api/bookings`, `/api/tickets`, `/api/staff/bookings`, `/api/staff/sales`, `/api/partner/bookings`, `/api/partner/dashboard/*`, `/api/admin/bookings`
- **Authentication**: Public, Authenticated User, PartnerOrStaff, PartnerOnly, AdminOnly
- **Enums**: BookingStatus (`0`: Pending, `1`: Paid, `2`: Confirmed, `3`: Cancelled, `4`: Refunded), TicketStatus (`0`: Valid, `1`: Issued, `2`: CheckedIn, `3`: Boarded, `4`: Cancelled)

---

## Checklist Summary

- [ ] **POST /api/bookings** (Public / Customer Create Booking)
  - [ ] `TC-BKG-CRT-001`: Create booking with available seat returns 201/200 with PNR code and status `Pending`
  - [ ] `TC-BKG-CRT-002`: Create booking with already occupied/reserved seat returns 409 Conflict / 400 Bad Request
  - [ ] `TC-BKG-CRT-003`: Create booking with invalid flight ID returns 404 Not Found
  - [ ] `TC-BKG-CRT-004`: Create booking with missing passenger details returns 400 Bad Request
  - [ ] `TC-BKG-CRT-005`: Create multi-passenger booking locks all requested seats atomically

- [ ] **GET /api/bookings/{id}**
  - [ ] `TC-BKG-GET-001`: Get booking by valid ID returns full booking details with tickets and passengers
  - [ ] `TC-BKG-GET-002`: Get booking by non-existent ID returns 404 Not Found
  - [ ] `TC-BKG-GET-003`: Invalid GUID format returns 400 Bad Request

- [ ] **GET /api/bookings/search?pnr={pnr}**
  - [ ] `TC-BKG-PNR-001`: Search with valid 6-char PNR returns matching booking
  - [ ] `TC-BKG-PNR-002`: Search with non-existent PNR returns 404 Not Found
  - [ ] `TC-BKG-PNR-003`: Search with empty or invalid PNR format returns 400 Bad Request

- [ ] **POST /api/bookings/{id}/pay**
  - [ ] `TC-BKG-PAY-001`: Successful payment changes status to `Confirmed`/`Paid` and issues tickets
  - [ ] `TC-BKG-PAY-002`: Pay for already cancelled/expired booking returns 400 Bad Request
  - [ ] `TC-BKG-PAY-003`: Pay for already paid booking returns 400 Bad Request / 409 Conflict
  - [ ] `TC-BKG-PAY-004`: Payment gateway failure keeps booking in `Pending` status

- [ ] **GET /api/tickets/{id}**
  - [ ] `TC-BKG-TCK-001`: Retrieve e-ticket by valid ticket ID returns ticket details
  - [ ] `TC-BKG-TCK-002`: Non-existent ticket ID returns 404 Not Found

- [ ] **GET /api/bookings/user/{id}**
  - [ ] `TC-BKG-USR-001`: Authenticated user retrieves own booking history paginated
  - [ ] `TC-BKG-USR-002`: Non-privileged user accessing another user's booking history returns 403 Forbidden
  - [ ] `TC-BKG-USR-003`: Unauthenticated request returns 401 Unauthorized

- [ ] **GET /api/bookings/user/{id}/stats**
  - [ ] `TC-BKG-UST-001`: Authenticated user gets own booking statistics (total bookings, spent)
  - [ ] `TC-BKG-UST-002`: Unauthorized access to another user's stats returns 403 Forbidden

- [ ] **POST /api/staff/bookings**
  - [ ] `TC-BKG-STF-001`: Staff creates booking on behalf of customer returns booking ID and PNR
  - [ ] `TC-BKG-STF-002`: Staff creating booking for flight not belonging to their airline returns 403 Forbidden
  - [ ] `TC-BKG-STF-003`: Unauthenticated or Customer role returns 401/403

- [ ] **GET /api/staff/sales**
  - [ ] `TC-BKG-STF-004`: Staff gets paginated sales report for own airline returns 200 OK
  - [ ] `TC-BKG-STF-005`: Search and status filters return filtered sales items

- [ ] **GET /api/partner/bookings**
  - [ ] `TC-BKG-PART-001`: Partner lists bookings scoped to partner's airline returns 200 OK
  - [ ] `TC-BKG-PART-002`: Partner cannot view bookings from other airlines

- [ ] **PUT /api/partner/bookings/{id}**
  - [ ] `TC-BKG-PART-003`: Partner updates booking status for own airline booking returns 200 OK
  - [ ] `TC-BKG-PART-004`: Partner updates booking belonging to another airline returns 403/404

- [ ] **Partner Dashboard Analytics Endpoints**
  - [ ] `TC-BKG-PART-005`: `GET /api/partner/dashboard/sales-summary` with date range returns sales metrics
  - [ ] `TC-BKG-PART-006`: `GET /api/partner/dashboard/occupancy-rates` returns flight occupancy percentages
  - [ ] `TC-BKG-PART-007`: `GET /api/partner/dashboard/revenue-trends` returns revenue trends over time

- [ ] **Admin Bookings Management**
  - [ ] `TC-BKG-ADM-001`: Admin lists all bookings system-wide `GET /api/admin/bookings` returns 200 OK
  - [ ] `TC-BKG-ADM-002`: Admin gets booking by ID `GET /api/admin/bookings/{id}` returns 200 OK
  - [ ] `TC-BKG-ADM-003`: Admin updates booking details `PUT /api/admin/bookings/{id}` returns 200 OK
  - [ ] `TC-BKG-ADM-004`: Admin updates booking status `PUT /api/admin/bookings/{id}/status` returns 200 OK
  - [ ] `TC-BKG-ADM-005`: Admin cancels booking `DELETE /api/admin/bookings/{id}` releases seat locks returns 200/204

---

## Detailed Test Case Specifications

### TC-BKG-CRT-001: Create booking happy path
- **Endpoint**: `POST /api/bookings`
- **Auth**: None / Optional Bearer
- **Preconditions**: Flight `FL-100` exists with seat `12A` marked as available (`isAvailable = true`).
- **Request Body**:
  ```json
  {
    "flightId": "4da85f64-5717-4562-b3fc-2c963f66afa6",
    "contactEmail": "passenger@example.com",
    "contactPhone": "+84912345678",
    "passengers": [
      {
        "firstName": "John",
        "lastName": "Doe",
        "identityCard": "001200001234",
        "seatNumber": "12A"
      }
    ]
  }
  ```
- **Assertions**:
  - HTTP Status: `200 OK` or `201 Created`
  - Response contains `id`, `pnrCode` (6 characters alphanumeric), `status: 0` (Pending).
  - Corresponding flight seat `12A` becomes reserved/unavailable.

### TC-BKG-CRT-002: Concurrent seat reservation conflict
- **Endpoint**: `POST /api/bookings`
- **Auth**: None
- **Preconditions**: Flight seat `12A` is already reserved or booked.
- **Request Body**: Same payload requesting seat `12A`.
- **Assertions**:
  - HTTP Status: `409 Conflict` (or `400 Bad Request` with problem details "Seat already reserved").

### TC-BKG-PAY-001: Process payment for pending booking
- **Endpoint**: `POST /api/bookings/{id}/pay`
- **Auth**: None
- **Preconditions**: Booking exists with status `0` (Pending), total price `120.00`.
- **Request Body**:
  ```json
  {
    "paymentMethod": "CreditCard",
    "amount": 120.00
  }
  ```
- **Assertions**:
  - HTTP Status: `200 OK`
  - Response contains `status: "Success"` and non-empty `transactionId`.
  - Booking status transitions to `Confirmed` (`2`) or `Paid` (`1`).
  - Associated tickets status transitions to `Issued` (`1`).

### TC-BKG-PNR-001: Search booking by PNR code
- **Endpoint**: `GET /api/bookings/search?pnr=ABC123`
- **Auth**: None
- **Preconditions**: Booking exists with `pnrCode = "ABC123"`.
- **Assertions**:
  - HTTP Status: `200 OK`
  - Response `pnrCode` is `"ABC123"`.
  - Response includes passenger list and ticket list.

### TC-BKG-ADM-005: Admin cancel booking releases seats
- **Endpoint**: `DELETE /api/admin/bookings/{id}`
- **Auth**: Admin JWT
- **Preconditions**: Booking exists with reserved seat `12A`.
- **Assertions**:
  - HTTP Status: `200 OK` or `204 No Content`
  - Booking status updated to `Cancelled` (`3`).
  - Flight seat `12A` is released (`isAvailable = true`).
