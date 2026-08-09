# Bookings Module

## Overview
The **Bookings Module** manages the transactional lifecycle of flight reservations, passenger details, ticket issuance, and payment processing. It coordinates seat reservations with the Flights module and handles automated ticket generation and payment confirmation.

---

## How It Works (Booking Lifecycle)
1. **Reservation**: The user selects a flight and seat. The system calls `CreateBookingCommand`, which invokes `IFlightSeatReservation.ReserveSeatsAsync` (cross-module call to the Flights module) to lock the seats.
2. **Persistence**: If seats are successfully reserved, the booking, passenger details, and tickets are saved in the database with a `Pending` status.
3. **Payment**: The user processes payment via `PayBookingCommand`. Upon successful payment, the booking status changes to `Confirmed`, and a `BookingConfirmedEvent` is published.
4. **Ticket Issuance**: The `BookingConfirmedEventHandler` consumes the event, updates the ticket status to `Issued`, and triggers a notification (Email) containing the e-ticket details.
5. **Auto-Cancellation**: A background job (`CancelExpiredBookingsJob`) runs periodically to cancel any `Pending` bookings that are not paid within 15 minutes, releasing the reserved seats back to the inventory.
6. **Refunds**: If a booking is cancelled, the `BookingRefundProcessor` background job handles processing refunds.

---

## Domain Entities & Data Model

### Booking
Represents the overall reservation transaction.
- `Id` (Guid): Unique identifier.
- `UserId` (Guid): The user who made the booking.
- `PnrCode` (string): Unique 6-character Passenger Name Record code.
- `TotalPrice` (decimal): Total cost of the booking.
- `Currency` (string): Currency code (default: `USD`).
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
- `Gender` (int): `0` = Male, `1` = Female, `2` = Other.
- `DateOfBirth` (Date): Date of birth.
- `Nationality` (string): Nationality.
- `PassportNumber` (string): Passport number.
- `PassportExpiryDate` (Date): Passport expiration date.

### Ticket
Represents an individual flight ticket issued to a passenger.
- `Id` (Guid): Unique identifier.
- `BookingId` (Guid): FK to the associated `Booking`.
- `PassengerId` (Guid): FK to the associated `Passenger`.
- `FlightId` (Guid): FK to the flight in the Flights module.
- `SeatId` (Guid): FK to the flight seat in the Flights module.
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
- `PaymentMethod` (string): Payment method (e.g., `Stripe`, `CreditCard`).
- `ProviderStatus` (string?): Status returned by the payment provider.
- `IsSuccessful` (bool): Whether the payment succeeded.
- `RawResponse` (string?): Raw JSON response from the payment gateway.
- `CreatedAt` (DateTime): UTC timestamp of payment.

---

## API Reference

### Authentication Roles
- **PartnerOrStaff**: Requires authentication as an Airline Admin, Airline Staff, or System Admin.
- **PartnerOnly**: Requires authentication as an Airline Admin.
- **AdminOnly**: Requires authentication as a System Admin.
- **User**: Requires authentication as a Customer.

### Endpoints

#### Public / Customer Endpoints
| Method | Path | Auth | Description |
|--------|------|------|-------------|
| **POST** | `/api/bookings` | None | Create a new booking (reserves seats and saves details). |
| **GET** | `/api/bookings/{id}` | None | Get booking details by ID. |
| **GET** | `/api/bookings/search?pnr={pnr}` | None | Search for a booking using PNR code. |
| **POST** | `/api/bookings/{id}/pay` | None | Process payment for a pending booking. |
| **GET** | `/api/tickets/{id}` | None | Retrieve e-ticket details. |
| **GET** | `/api/bookings/my-bookings` | User | Retrieve booking history for the logged-in user. |

#### Staff Endpoints
| Method | Path | Auth | Description |
|--------|------|------|-------------|
| **POST** | `/api/staff/bookings` | PartnerOrStaff | Create a booking on behalf of a customer. |
| **GET** | `/api/staff/sales` | PartnerOrStaff | List ticket sales for the staff's airline. |

#### Partner Endpoints
| Method | Path | Auth | Description |
|--------|------|------|-------------|
| **GET** | `/api/partner/bookings` | PartnerOnly | List bookings for the partner's airline. |
| **PUT** | `/api/partner/bookings/{id}` | PartnerOnly | Update booking status. |
| **GET** | `/api/partner/dashboard/sales-summary` | PartnerOnly | Get sales summary statistics. |
| **GET** | `/api/partner/dashboard/occupancy-rates` | PartnerOnly | Get flight occupancy rates. |
| **GET** | `/api/partner/dashboard/revenue-trends` | PartnerOnly | Get revenue trends over time. |

#### Admin Endpoints
| Method | Path | Auth | Description |
|--------|------|------|-------------|
| **GET** | `/api/admin/bookings` | AdminOnly | List all bookings in the system. |
| **GET** | `/api/admin/bookings/{id}` | AdminOnly | Get details of any booking. |
| **PUT** | `/api/admin/bookings/{id}` | AdminOnly | Update booking details. |
| **PUT** | `/api/admin/bookings/{id}/status` | AdminOnly | Update booking status. |
| **DELETE** | `/api/admin/bookings/{id}` | AdminOnly | Cancel a booking. |
