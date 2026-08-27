# Backend Planning: Collaborative Group Booking & Split Payment (SkyGroup)

## 1. Overview
Backend engine supporting collaborative multi-user checkout sessions, seat holding, partial split payment processing, time-bounded expiration worker, and live room synchronization via SignalR.

---

## 2. Database Schema & Migrations

### 2.1 Schema: `bookings`

#### Table: `bookings.group_bookings`
```sql
CREATE TABLE bookings.group_bookings (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    leader_user_id UUID NOT NULL REFERENCES users.users(id),
    flight_id UUID NOT NULL REFERENCES flights.flights(id),
    return_flight_id UUID NULL REFERENCES flights.flights(id),
    group_name VARCHAR(100) NOT NULL,
    invite_code VARCHAR(12) UNIQUE NOT NULL,
    total_amount DECIMAL(18, 2) NOT NULL DEFAULT 0,
    paid_amount DECIMAL(18, 2) NOT NULL DEFAULT 0,
    currency VARCHAR(3) NOT NULL DEFAULT 'VND',
    status VARCHAR(20) NOT NULL DEFAULT 'Active', -- 'Active', 'FullyPaid', 'Expired', 'Cancelled'
    split_strategy VARCHAR(20) NOT NULL DEFAULT 'ByPassenger', -- 'Equal', 'ByPassenger', 'Custom'
    expires_at TIMESTAMPTZ NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_group_bookings_invite ON bookings.group_bookings(invite_code);
CREATE INDEX idx_group_bookings_status_expiry ON bookings.group_bookings(status, expires_at);
```

#### Table: `bookings.group_members`
```sql
CREATE TABLE bookings.group_members (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    group_booking_id UUID NOT NULL REFERENCES bookings.group_bookings(id) ON DELETE CASCADE,
    user_id UUID NULL REFERENCES users.users(id),
    passenger_name VARCHAR(150) NOT NULL,
    passenger_email VARCHAR(150) NOT NULL,
    passenger_phone VARCHAR(25) NULL,
    seat_number VARCHAR(10) NULL,
    return_seat_number VARCHAR(10) NULL,
    assigned_amount DECIMAL(18, 2) NOT NULL DEFAULT 0,
    paid_amount DECIMAL(18, 2) NOT NULL DEFAULT 0,
    payment_status VARCHAR(20) NOT NULL DEFAULT 'Pending', -- 'Pending', 'Paid', 'Failed'
    joined_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_group_members_group ON bookings.group_members(group_booking_id);
```

---

## 3. Domain Layer

### 3.1 Entities & Enums
- **`GroupBooking`** (Aggregate Root):
  - Properties: `Id`, `LeaderUserId`, `FlightId`, `ReturnFlightId`, `GroupName`, `InviteCode`, `TotalAmount`, `PaidAmount`, `Currency`, `Status`, `SplitStrategy`, `ExpiresAt`.
  - Methods: `AddMember(...)`, `SelectSeat(...)`, `ApplyPayment(...)`, `CancelGroup()`, `CheckFullyPaid()`.
- **`GroupMember`** (Entity):
  - Properties: `Id`, `GroupBookingId`, `UserId`, `PassengerName`, `PassengerEmail`, `SeatNumber`, `AssignedAmount`, `PaidAmount`, `PaymentStatus`.
- **Enums**:
  - `GroupBookingStatus`: `Active`, `FullyPaid`, `Expired`, `Cancelled`.
  - `SplitStrategy`: `Equal`, `ByPassenger`, `Custom`.
  - `MemberPaymentStatus`: `Pending`, `Paid`, `Failed`.

### 3.2 Domain Events
- `GroupBookingCreatedDomainEvent(Guid GroupId, string InviteCode, Guid LeaderId)`
- `MemberJoinedGroupDomainEvent(Guid GroupId, Guid MemberId, string MemberName)`
- `MemberSeatSelectedDomainEvent(Guid GroupId, Guid MemberId, string SeatNumber)`
- `MemberPaymentReceivedDomainEvent(Guid GroupId, Guid MemberId, decimal AmountPaid, decimal RemainingGroupAmount)`
- `GroupBookingCompletedDomainEvent(Guid GroupId, List<Guid> BookingIds)`
- `GroupBookingExpiredDomainEvent(Guid GroupId)`

---

## 4. Application Layer (CQRS & MediatR)

### 4.1 Commands
| Command | Handler | Description |
| :--- | :--- | :--- |
| `CreateGroupBookingCommand` | `CreateGroupBookingCommandHandler` | Initiates group room, creates invite code, holds leader seat. |
| `JoinGroupBookingCommand` | `JoinGroupBookingCommandHandler` | Adds new passenger to group by invite code. |
| `SelectGroupMemberSeatCommand` | `SelectGroupMemberSeatCommandHandler` | Validates seat availability and assigns seat to group member. |
| `ProcessMemberPaymentCommand` | `ProcessMemberPaymentCommandHandler` | Integrates with payment gateway (Stripe/VNPay), marks member paid, checks if entire group is completed. |
| `CancelGroupBookingCommand` | `CancelGroupBookingCommandHandler` | Releases held seats and triggers refunds if applicable. |

### 4.2 Queries
| Query | Handler | Return Type |
| :--- | :--- | :--- |
| `GetGroupBookingByCodeQuery` | `GetGroupBookingByCodeQueryHandler` | `GroupBookingDetailDto` (members, seats, payment progress, remaining time). |
| `GetUserGroupBookingsQuery` | `GetUserGroupBookingsQueryHandler` | `List<GroupBookingSummaryDto>` (active and historical groups). |

---

## 5. Infrastructure & Real-Time (SignalR)

### 5.1 Background Service: `GroupBookingExpirationWorker`
- Runs every 1 minute.
- Scans `bookings.group_bookings` where `status = 'Active'` AND `expires_at < NOW()`.
- Releases all temporarily held seats in `flights.seats`.
- Updates group status to `Expired`.
- Sends SignalR `GroupExpired` and email cancellation notices to all participants.

### 5.2 SignalR Hub: `GroupBookingHub`
- **Route**: `/hubs/group-booking`
- **Room Management**:
  - `JoinGroupRoom(string inviteCode)`: Adds client to `group-{inviteCode}` room.
  - `LeaveGroupRoom(string inviteCode)`: Removes client.
- **Broadcast Events**:
  - `MemberJoined(GroupMemberDto member)`
  - `MemberSeatChanged(string memberId, string seatNumber)`
  - `MemberPaymentCompleted(string memberId, decimal amountPaid, decimal groupRemainingAmount)`
  - `GroupBookingCompleted(string confirmationCode)`
  - `GroupExpired()`

---

## 6. Minimal API Endpoints (`Bookings.Api`)

```csharp
app.MapPost("/api/group-bookings", CreateGroupBooking)
   .RequireAuthorization()
   .WithName("CreateGroupBooking");

app.MapGet("/api/group-bookings/{inviteCode}", GetGroupBookingByCode)
   .WithName("GetGroupBookingByCode");

app.MapPost("/api/group-bookings/{inviteCode}/join", JoinGroupBooking)
   .WithName("JoinGroupBooking");

app.MapPost("/api/group-bookings/{inviteCode}/seats", SelectGroupMemberSeat)
   .WithName("SelectGroupMemberSeat");

app.MapPost("/api/group-bookings/{inviteCode}/pay", ProcessMemberPayment)
   .WithName("ProcessMemberPayment");
```

---

## 7. Execution Tasks
1. Generate EF Core migration for `group_bookings` and `group_members`.
2. Implement Domain entity methods and validation rules.
3. Build MediatR commands, handlers, and payment verification flow.
4. Implement `GroupBookingHub` and `GroupBookingExpirationWorker`.
5. Write integration tests for concurrent member seat picks and split payment completion.
