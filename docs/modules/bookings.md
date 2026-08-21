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
| Method | Path | Auth | Description | Input Type | Output Type |
|--------|------|------|-------------|------------|-------------|
| **POST** | `/api/bookings` | None | Create a new booking (reserves seats and saves details). | `CreateBookingPayload { flightId: string, contactEmail: string, contactPhone?: string, passengers: Array<{ firstName: string, lastName: string, identityCard: string, seatNumber: string }> }` | `Booking { id: string, userId: string, pnrCode: string, totalPrice: number, currency: string, status: number, contactEmail: string, contactPhone: string, SpecialRequests?: string, passengers: Array<{ firstName: string, lastName: string, identityCard: string, seatNumber: string }>, tickets: Array<{ id: string, ticketNumber: string, seatNumber: string, status: number }> }` |
| **GET** | `/api/bookings/{id}` | None | Get booking details by ID. | None | `Booking { id: string, userId: string, pnrCode: string, totalPrice: number, currency: string, status: number, contactEmail: string, contactPhone: string, SpecialRequests?: string, passengers: Array<{ firstName: string, lastName: string, identityCard: string, seatNumber: string }>, tickets: Array<{ id: string, ticketNumber: string, seatNumber: string, status: number }> }` |
| **GET** | `/api/bookings/search?pnr={pnr}` | None | Search for a booking using PNR code. | None (Query param `pnrCode: string`) | `Booking { id: string, userId: string, pnrCode: string, totalPrice: number, currency: string, status: number, contactEmail: string, contactPhone: string, SpecialRequests?: string, passengers: Array<{ firstName: string, lastName: string, identityCard: string, seatNumber: string }>, tickets: Array<{ id: string, ticketNumber: string, seatNumber: string, status: number }> }` |
| **POST** | `/api/bookings/{id}/pay` | None | Process payment for a pending booking. | `{ paymentMethod: string, amount: number }` | `{ status: string, transactionId: string }` |
| **GET** | `/api/tickets/{id}` | None | Retrieve e-ticket details. | None | `Ticket { id: string, bookingId: string, passengerId: string, flightId: string, seatId: string, ticketNumber: string, gate?: string, boardingTime?: string, status: number }` |
| **GET** | `/api/bookings/user/{id}` | User | Retrieve booking history for a user (owner or privileged). | None (Query params `pageNumber: number, pageSize: number`) | `PagedResult<BookingDto> { items: BookingDto[], totalCount: number, pageNumber: number, pageSize: number }` |
| **GET** | `/api/bookings/user/{id}/stats` | User | Retrieve booking statistics for a user (owner or privileged). | None | `UserBookingStatsDto { totalBookings: number, totalSpent: number, lastMonthSpent: number, lastYearSpent: number }` |

#### Staff Endpoints
| Method | Path | Auth | Description | Input Type | Output Type |
|--------|------|------|-------------|------------|-------------|
| **POST** | `/api/staff/bookings` | PartnerOrStaff | Create a booking on behalf of a customer. | `StaffCreateBookingRequest { flightId: string, contactName: string, contactEmail: string, contactPhone?: string, passengers: Array<{ firstName: string, lastName: string, identityCard: string, seatNumber: string }> }` | `StaffBookingResult { bookingId: string, pnrCode: string, totalPrice: number, seatNumbers: string[] }` |
| **GET** | `/api/staff/sales` | PartnerOrStaff | List ticket sales for the staff's airline. | None (Query params `pageIndex: number, pageSize: number, search?: string, status?: string`) | `PagedResult<StaffSale> { items: StaffSale[], totalCount: number, pageNumber: number, pageSize: number, totalPages: number, hasNextPage: boolean, hasPreviousPage: boolean }` where `StaffSale` is `{ id: string, bookingId: string, passengerName: string, flightNumber: string, route: string, departureAt?: string, seatClass: string, amount: number, status: string, bookedAt: string }` |

#### Partner Endpoints
| Method | Path | Auth | Description | Input Type | Output Type |
|--------|------|------|-------------|------------|-------------|
| **GET** | `/api/partner/bookings` | PartnerOnly | List bookings for the partner's airline. | None (Query params `pageIndex: number, pageSize: number`) | `PagedResult<SystemBooking> { items: SystemBooking[], totalCount: number, pageNumber: number, pageSize: number, totalPages: number, hasNextPage: boolean, hasPreviousPage: boolean }` where `SystemBooking` is `{ id: string, pnrCode: string, totalPrice: number, currency: string, status: number, contactEmail: string, contactPhone: string, createdAt: string, passengers: unknown[], tickets: unknown[] }` |
| **PUT** | `/api/partner/bookings/{id}` | PartnerOnly | Update booking status. | `BookingAdminPayload { flightId: string, passengerName: string, seatNumber: string, status: string }` | `SystemBooking { id: string, pnrCode: string, totalPrice: number, currency: string, status: number, contactEmail: string, contactPhone: string, createdAt: string, passengers: unknown[], tickets: unknown[] }` |
| **GET** | `/api/partner/dashboard/sales-summary` | PartnerOnly | Get sales summary statistics. | None (Query params `fromDate: string, toDate: string`) | `unknown (Sales summary data)` |
| **GET** | `/api/partner/dashboard/occupancy-rates` | PartnerOnly | Get flight occupancy rates. | None (Query params `fromDate: string, toDate: string`) | `unknown (Occupancy rates data)` |
| **GET** | `/api/partner/dashboard/revenue-trends` | PartnerOnly | Get revenue trends over time. | None (Query params `fromDate: string, toDate: string`) | `unknown (Revenue trends data)` |

#### Admin Endpoints
| Method | Path | Auth | Description | Input Type | Output Type |
|--------|------|------|-------------|------------|-------------|
| **GET** | `/api/admin/bookings` | AdminOnly | List all bookings in the system. | None (Query params `pageIndex: number, pageSize: number`) | `PagedResult<SystemBooking> { items: SystemBooking[], totalCount: number, pageNumber: number, pageSize: number, totalPages: number, hasNextPage: boolean, hasPreviousPage: boolean }` |
| **GET** | `/api/admin/bookings/{id}` | AdminOnly | Get details of any booking. | None | `SystemBooking { id: string, pnrCode: string, totalPrice: number, currency: string, status: number, contactEmail: string, contactPhone: string, createdAt: string, passengers: unknown[], tickets: unknown[] }` |
| **PUT** | `/api/admin/bookings/{id}` | AdminOnly | Update booking details. | `BookingAdminPayload { flightId: string, passengerName: string, seatNumber: string, status: string }` | `SystemBooking { id: string, pnrCode: string, totalPrice: number, currency: string, status: number, contactEmail: string, contactPhone: string, createdAt: string, passengers: unknown[], tickets: unknown[] }` |
| **PUT** | `/api/admin/bookings/{id}/status` | AdminOnly | Update booking status. | `{ status: string }` | `SystemBooking { id: string, pnrCode: string, totalPrice: number, currency: string, status: number, contactEmail: string, contactPhone: string, createdAt: string, passengers: unknown[], tickets: unknown[] }` |
| **DELETE** | `/api/admin/bookings/{id}` | AdminOnly | Cancel a booking. | None | `void` |
