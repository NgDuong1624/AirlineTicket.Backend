# Bookings Module

## Overview
The **Bookings Module** manages the transactional lifecycle of flight reservations, passenger details, ticket issuance, and multi-gateway payment processing (Stripe, PayPal, VNPay, MoMo). It coordinates seat reservations with the Flights module, provides staff and partner sales portals, and handles automated ticket generation and cancellation policies.

---

## How It Works (Booking Lifecycle)
1. **Reservation**: The user selects a flight and seat. The system calls `CreateBookingCommand` (`POST /api/bookings`), which invokes `IFlightSeatReservation.ReserveSeatsAsync` (cross-module call to the Flights module) to lock the seats.
2. **Persistence**: If seats are successfully reserved, the booking, passenger details, and tickets are saved in the database with a `Pending` status.
3. **Payment**: The user initiates checkout via `CreateCheckoutSessionCommand` (`POST /api/payments/checkout`). Supported gateways include Stripe, PayPal, VNPay, and MoMo. Payment gateways invoke asynchronous webhook callbacks (`POST /api/payments/webhooks/{provider}`), which update the payment transaction and transition the booking status to `Confirmed`, publishing a `BookingConfirmedEvent`.
4. **Ticket Issuance**: The `BookingConfirmedEventHandler` consumes the event, updates the ticket status to `Issued`, and triggers an email notification containing the e-ticket details.
5. **Auto-Cancellation**: A background job (`CancelExpiredBookingsJob`) runs periodically to cancel any `Pending` bookings not completed within 15 minutes, releasing reserved seats back to inventory.
6. **Refunds & Cancellation**: Customers and administrators can cancel bookings (`DELETE /api/bookings/{id}`), transitioning tickets to `Cancelled` and restoring seat availability.

---

## Domain Entities & Data Model

### Booking
Represents the overall reservation transaction.
- `Id` (Guid): Unique identifier.
- `UserId` (Guid?): The user who made the booking (nullable for guest bookings).
- `PnrCode` (string): Unique 6-character Passenger Name Record code.
- `TotalPrice` (decimal): Total cost of the booking.
- `Currency` (string): Currency code (default: `VND`).
- `Status` (BookingStatus): `0` = Pending, `1` = Paid, `2` = Confirmed, `3` = Cancelled, `4` = Refunded.
- `ContactEmail` (string): Contact email address.
- `ContactPhone` (string): Contact phone number.
- `SpecialRequests` (string?): Optional special requests.
- `IsDeleted` (bool): Soft delete flag.
- `CreatedAt` (DateTime): UTC timestamp of creation.
- `UpdatedAt` (DateTime): UTC timestamp of last update.

### Passenger
Represents a passenger associated with a booking.
- `Id` (Guid): Unique identifier.
- `BookingId` (Guid): FK to the associated `Booking`.
- `FirstName` (string): Passenger's first name.
- `LastName` (string): Passenger's last name.
- `IdentityCard` (string): National ID or passport number.
- `Gender` (int): `0` = Male, `1` = Female, `2` = Other.
- `DateOfBirth` (Date?): Date of birth.
- `Nationality` (string?): Nationality.

### Ticket
Represents an individual flight ticket issued to a passenger.
- `Id` (Guid): Unique identifier.
- `BookingId` (Guid): FK to the associated `Booking`.
- `PassengerId` (Guid): FK to the associated `Passenger`.
- `FlightId` (Guid): FK to the flight in the Flights module.
- `SeatNumber` (string): Assigned seat number.
- `TicketNumber` (string): Unique ticket number.
- `Gate` (string?): Boarding gate.
- `BoardingTime` (DateTime?): Scheduled boarding time.
- `Status` (TicketStatus): `0` = Valid, `1` = Issued, `2` = CheckedIn, `3` = Boarded, `4` = Cancelled.

### Payment
Represents the payment transaction details.
- `Id` (Guid): Unique identifier.
- `BookingId` (Guid): FK to the associated `Booking`.
- `TransactionId` (string): Unique transaction ID from the payment provider.
- `Amount` (decimal): Paid amount.
- `PaymentMethod` (string): Payment method/provider (e.g., `Stripe`, `PayPal`, `VNPay`, `MoMo`).
- `ProviderStatus` (string?): Status returned by payment provider.
- `IsSuccessful` (bool): Whether the payment succeeded.
- `RawResponse` (string?): Raw response or webhook payload from gateway.
- `CreatedAt` (DateTime): UTC timestamp of payment.

---

## API Reference

### Authentication Roles
- **PartnerOrStaff**: Requires authentication as an Airline Partner, Airline Staff, or System Admin.
- **StaffOnly**: Requires authentication as Airline Staff.
- **PartnerOnly**: Requires authentication as an Airline Partner.
- **AdminOnly**: Requires authentication as a System Admin.
- **Authenticated**: Requires a valid JWT token (Customer or privileged role).

### Endpoints

#### Public / Customer Endpoints
| Method | Path | Auth | Description | Input Type | Output Type |
|--------|------|------|-------------|------------|-------------|
| **POST** | `/api/bookings` | None | Create a new booking (reserves seats and saves passenger details). | `CreateBookingRequest { flightId: Guid, contactEmail: string, contactPhone: string, passengers: PassengerDto[] }` | `BookingResultDto` |
| **GET** | `/api/bookings/{id}` | None | Get booking details by ID. | Route param `id: Guid` | `BookingDetailDto` |
| **PUT** | `/api/bookings/{id}` | None | Update booking passenger and contact information. | Route param `id: Guid`, `UpdateBookingRequest { passengers?: PassengerDto[], contactEmail?: string, contactPhone?: string }` | `{ message: string }` |
| **DELETE** | `/api/bookings/{id}` | None | Cancel booking and release reserved seats. | Route param `id: Guid` | `204 NoContent` |
| **GET** | `/api/bookings/search` | None | Search for a booking using PNR code. | Query param `pnrCode: string` | `BookingDetailDto` |
| **GET** | `/api/tickets/{id}` | None | Retrieve e-ticket details. | Route param `id: Guid` | `TicketDto` |
| **POST** | `/api/payments/checkout` | None | Initiate multi-gateway checkout session. | `CheckoutRequest { bookingId: Guid, provider: PaymentProvider, currency: Currency, returnUrl: string, cancelUrl: string }` | `CreatePaymentResult { providerTransactionId: string, provider: PaymentProvider, paymentUrl?: string, clientSecret?: string, orderId?: string }` |
| **GET** | `/api/payments/{bookingId}/status` | None | Query payment transaction status for a booking. | Route param `bookingId: Guid` | `PaymentStatusDto` |
| **POST** | `/api/payments/webhooks/stripe` | None | Stripe asynchronous webhook callback. | Raw request body & `Stripe-Signature` header | `{ received: true }` |
| **POST** | `/api/payments/webhooks/paypal` | None | PayPal asynchronous webhook callback. | Raw request body & `PAYPAL-*` headers | `{ received: true }` |
| **POST** | `/api/payments/webhooks/vnpay` | None | VNPay IPN webhook callback. | Query string / URL-encoded parameters | `{ RspCode: "00", Message: "Confirm Success" }` |
| **POST** | `/api/payments/webhooks/momo` | None | MoMo IPN webhook callback. | Raw request body | `204 NoContent` |

#### User Authenticated Endpoints
| Method | Path | Auth | Description | Input Type | Output Type |
|--------|------|------|-------------|------------|-------------|
| **GET** | `/api/bookings/user/{id}` | Authenticated | Retrieve booking history for a user (owner or privileged staff/admin). | Route param `id: Guid`, Query params `pageNumber?: number, pageSize?: number` | `PagedResult<BookingDto>` |
| **GET** | `/api/bookings/user/{id}/stats` | Authenticated | Retrieve booking statistics for a user (owner or privileged staff/admin). | Route param `id: Guid` | `UserBookingStatsDto` |

#### Staff Endpoints
| Method | Path | Auth | Description | Input Type | Output Type |
|--------|------|------|-------------|------------|-------------|
| **GET** | `/api/bookings` | PartnerOrStaff | List all bookings across the system with filtering. | Query params `pageNumber?: number, pageSize?: number, search?: string, status?: string, date?: DateTime` | `{ items: BookingDto[], totalCount: number }` |
| **POST** | `/api/staff/bookings` | StaffOnly | Create a booking on behalf of a call-in customer. | `StaffCreateBookingRequest { flightId: Guid, contactName: string, contactEmail: string, contactPhone: string, passengers: StaffBookingPassenger[] }` | `StaffBookingResultDto` |
| **GET** | `/api/staff/sales` | StaffOnly | List ticket sales for the staff portal sales board. | Query params `pageIndex?: number, pageSize?: number, search?: string, status?: string` | `StaffSalesResultDto` |

#### Partner Endpoints
| Method | Path | Auth | Description | Input Type | Output Type |
|--------|------|------|-------------|------------|-------------|
| **GET** | `/api/partner/bookings` | PartnerOnly | List partner airline bookings. | Query params `pageIndex?: number, pageSize?: number, search?: string, status?: string, date?: DateTime` | `{ items: BookingDto[], totalCount: number }` |
| **PUT** | `/api/partner/bookings/{id}` | PartnerOnly | Update booking status. | Route param `id: Guid`, `PartnerBookingUpdateRequest { status: string }` | `200 OK` |
| **GET** | `/api/partner/dashboard/sales-summary` | PartnerOnly | Get partner sales summary metrics. | Query params `fromDate?: DateTime, toDate?: DateTime` | `SalesSummaryDto` |
| **GET** | `/api/partner/dashboard/occupancy-rates` | PartnerOnly | Get partner flight occupancy rates. | Query params `fromDate?: DateTime, toDate?: DateTime` | `OccupancyRatesDto` |
| **GET** | `/api/partner/dashboard/revenue-trends` | PartnerOnly | Get partner revenue trends over time. | Query params `fromDate?: DateTime, toDate?: DateTime` | `RevenueTrendsDto` |

#### Admin Endpoints
| Method | Path | Auth | Description | Input Type | Output Type |
|--------|------|------|-------------|------------|-------------|
| **GET** | `/api/admin/bookings` | AdminOnly | List all bookings across the system. | Query params `pageIndex?: number, pageSize?: number, search?: string, status?: string, date?: DateTime` | `{ items: BookingDto[], totalCount: number }` |
| **GET** | `/api/admin/bookings/{id}` | AdminOnly | Get details of any booking. | Route param `id: Guid` | `BookingDetailDto` |
| **PUT** | `/api/admin/bookings/{id}` | AdminOnly | Update booking passenger and contact details. | Route param `id: Guid`, `UpdateBookingRequest` | `200 OK` |
| **PUT** | `/api/admin/bookings/{id}/status` | AdminOnly | Update booking status directly. | Route param `id: Guid`, `UpdateBookingStatusRequest { status: string }` | `200 OK` |
| **DELETE** | `/api/admin/bookings/{id}` | AdminOnly | Cancel a booking. | Route param `id: Guid` | `204 NoContent` |

---

## Real-Time Seat Synchronization (SignalR)

### Hub Endpoint
- **URL**: `/hubs/seats`
- **Authentication**: Anonymous / Optional

### Client → Server Methods
- `JoinFlight(Guid flightId)`: Join real-time updates for a specific flight's seat map.
- `LeaveFlight(Guid flightId)`: Leave the flight seat group.

### Server → Client Events
- `SeatUpdated(Guid flightId, string seatNumber, bool isAvailable)`: Emitted when seat availability changes due to holds or cancellations.
