# Cấu trúc Cơ sở dữ liệu (Database Schema)

Hệ thống **Airline Ticket** sử dụng mô hình cơ sở dữ liệu quan hệ (SQL Server) được tổ chức theo các **Schema** riêng biệt cho từng Module (theo kiến trúc Modular Monolith).

## 1. Schema `flights`
Quản lý dữ liệu lõi về hoạt động hàng không.
- **Airports (Sân bay)**: `Id`, `IataCode` (Unique), `Name`, `City`, `Country`, `Timezone`.
- **Airlines (Hãng bay)**: `Id`, `IataCode`, `Name`.
- **Airplanes (Máy bay)**: `Id`, `AirlineId`, `Model`, `RegistrationNumber`, `TotalCapacity`.
- **AirplaneSeats (Cấu hình ghế mẫu)**: `Id`, `AirplaneId`, `SeatNumber` (VD: 1A, 1B), `SeatClass`, `PriceMultiplier` (Hệ số nhân giá).
- **Routes (Tuyến bay)**: `Id`, `AirlineId`, `OriginAirportId`, `DestinationAirportId`.
- **Flights (Chuyến bay thực tế)**: `Id`, `RouteId`, `AirplaneId`, `FlightNumber`, `BasePrice` (Giá gốc), `ScheduledDeparture`, `ScheduledArrival`, `ActualDeparture`, `ActualArrival`, `Status`.
- **FlightSeats (Ghế của chuyến bay cụ thể)**: `Id`, `FlightId`, `SeatNumber`, `SeatClass`, `Price` (Giá thực tế = BasePrice * PriceMultiplier), `IsAvailable`.

**Cơ chế Copy Ghế (Instance Creation):**
Khi một chuyến bay (`Flight`) được tạo ra từ một máy bay (`Airplane`), hệ thống sẽ tự động sao chép toàn bộ danh sách `AirplaneSeats` thành `FlightSeats` cho riêng chuyến bay đó. Điều này giúp lịch sử vé và giá không bị ảnh hưởng nếu cấu hình hoặc hệ số giá của máy bay gốc bị thay đổi trong tương lai.

## 2. Schema `bookings`
Quản lý nghiệp vụ đặt chỗ và bán vé.
- **Bookings (Đơn đặt chỗ)**: `Id`, `UserId`, `PnrCode` (Mã đặt chỗ/Mã PNR), `TotalPrice`, `Status`, `CreatedAt`.
- **Passengers (Hành khách)**: `Id`, `BookingId`, `FirstName`, `LastName`, `DateOfBirth`, `PassportNumber`.
- **Tickets (Vé điện tử)**: `Id`, `BookingId`, `FlightId`, `PassengerId`, `SeatId`, `TicketNumber`, `Status`.
- **Payments (Thanh toán)**: `Id`, `BookingId`, `Amount`, `PaymentMethod`, `Status`, `TransactionId`, `CreatedAt`.

## 3. Schema `users`
Quản lý người dùng và xác thực.
- **Users (Người dùng)**: `Id`, `Email` (Unique), `PasswordHash`, `FullName`, `Phone`, `Role`.

## 4. Schema `promotions`
Quản lý các chiến dịch khuyến mãi và mã giảm giá (Dynamic Pricing).
- **Campaigns (Chiến dịch giảm giá)**: `Id`, `Name`, `Description`, `StartDate`, `EndDate`, `DiscountType`, `DiscountValue`, `TargetAirlineId`, `TargetFlightId`, `IsActive`. Tự động áp dụng giảm giá khi thỏa mãn điều kiện.
- **Coupons (Mã giảm giá)**: `Id`, `Code` (Unique), `DiscountType`, `DiscountValue`, `MaxDiscountAmount`, `MaxUsages`, `CurrentUsages`, `ValidFrom`, `ValidTo`, `IsActive`. Dùng để áp dụng ở bước Checkout.
