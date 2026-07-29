# Thiết kế Cơ sở Dữ liệu Hệ thống Bán vé Máy bay

Tài liệu này chi tiết hóa cấu trúc Cơ sở Dữ liệu của hệ thống, được thiết kế theo hướng kiến trúc Modular Monolith để sẵn sàng phân tách khi mở rộng hệ thống.

---

## 1. Sơ đồ Quan hệ Thực thể (ERD)

```mermaid
erDiagram
    %% IDENTITY SCHEMA
    dbo_Users ||--|| dbo_Roles : "has"
    dbo_Roles ||--o{ dbo_RolePermissions : "has"
    dbo_Permissions ||--o{ dbo_RolePermissions : "defines"
    dbo_Users ||--o{ dbo_UserPermissionScopes : "has"
    dbo_Permissions ||--o{ dbo_UserPermissionScopes : "scoped_by"
    dbo_Airlines ||--o{ dbo_UserPermissionScopes : "restricts"
    
    %% FLIGHTS SCHEMA
    dbo_Airlines ||--o{ dbo_Airplanes : "owns"
    dbo_AircraftModels ||--o{ dbo_Airplanes : "defines"
    dbo_AircraftModels ||--o{ dbo_AircraftModelSeatTemplates : "has"
    dbo_Airplanes ||--o{ dbo_AirplaneSeats : "has"
    dbo_Airlines ||--o{ dbo_Routes : "operates"
    dbo_Airports ||--o{ dbo_Routes : "origin"
    dbo_Airports ||--o{ dbo_Routes : "destination"
    dbo_Routes ||--o{ dbo_Flights : "has"
    dbo_Airplanes ||--o{ dbo_Flights : "assigned_to"
    dbo_Flights ||--o{ dbo_FlightSeats : "has"

    %% BOOKINGS SCHEMA
    dbo_Users ||--o{ dbo_Bookings : "makes"
    dbo_Bookings ||--o{ dbo_Passengers : "contains"
    dbo_Bookings ||--o{ dbo_Tickets : "contains"
    dbo_Bookings ||--o{ dbo_Payments : "settles"
    dbo_Passengers ||--o{ dbo_Tickets : "assigned_to"
    dbo_Flights ||--o{ dbo_Tickets : "booked_on"
    dbo_FlightSeats ||--o| dbo_Tickets : "allocated_to"

    %% INTERACTIONS SCHEMA
    dbo_Users ||--o{ dbo_Reviews : "writes"
    dbo_Airlines ||--o{ dbo_Reviews : "reviewed"
    dbo_Flights ||--o{ dbo_Reviews : "reviewed"
    dbo_Users ||--o{ dbo_ChatMessages : "sends"

    %% CMS SCHEMA
    dbo_Categories ||--o{ dbo_Articles : "belongs_to"
    dbo_Users ||--o{ dbo_Articles : "writes"

    %% NOTIFICATIONS SCHEMA
    dbo_NotificationTemplates : "standalone"
    dbo_Users ||--o{ dbo_Notifications : "receives"
```

---

## 2. Chi tiết các Phân hệ & Cấu trúc Bảng

### 2.1. Phân hệ Định danh (`dbo`)
Quản lý người dùng, phân quyền truy cập hệ thống.

#### Bảng `dbo.Users`
Lưu trữ thông tin người dùng (Khách hàng, Đối tác, Quản trị viên).

| Tên Cột | Kiểu Dữ Liệu | Ràng buộc | Mô tả |
| :--- | :--- | :--- | :--- |
| `Id` | UNIQUEIDENTIFIER | PRIMARY KEY | Định danh duy nhất người dùng |
| `Email` | NVARCHAR(255) | UNIQUE, NOT NULL | Địa chỉ Email đăng nhập |
| `EmailConfirmed` | BIT | DEFAULT 0, NOT NULL | Trạng thái xác thực email |
| `PasswordHash` | NVARCHAR(MAX) | NOT NULL | Mật khẩu đã được băm bảo mật |
| `FullName` | NVARCHAR(255) | NOT NULL | Họ và tên đầy đủ |
| `PhoneNumber` | NVARCHAR(20) | NULL | Số điện thoại liên lạc |
| `Role` | INT | FOREIGN KEY -> Roles(Id), NOT NULL | Vai trò người dùng (0: Admin, 1: Staff, 2: Customer) |
| `AvatarUrl` | NVARCHAR(500) | NULL | Đường dẫn ảnh đại diện |
| `LanguagePreference` | NVARCHAR(10) | DEFAULT 'vi' | Ngôn ngữ ưu tiên (vi, en,...) |
| `LastLoginAt` | DATETIME2 | NULL | Thời gian đăng nhập cuối cùng |
| `IsActive` | BIT | DEFAULT 1, NOT NULL | Trạng thái hoạt động tài khoản |
| `IsDeleted` | BIT | DEFAULT 0, NOT NULL | Cờ xóa mềm (Soft Delete) |
| `CreatedAt` | DATETIME2 | DEFAULT GETUTCDATE() | Ngày giờ khởi tạo |
| `UpdatedAt` | DATETIME2 | DEFAULT GETUTCDATE() | Ngày giờ cập nhật mới nhất |

#### Bảng `dbo.Roles`
Danh sách vai trò hệ thống.

| Tên Cột | Kiểu Dữ Liệu | Ràng buộc | Mô tả |
| :--- | :--- | :--- | :--- |
| `Id` | INT | PRIMARY KEY | Định danh duy nhất vai trò |
| `Name` | NVARCHAR(50) | UNIQUE, NOT NULL | Tên vai trò (Admin, Partner, Customer) |
| `Description` | NVARCHAR(255) | NULL | Mô tả chi tiết vai trò |

#### Bảng `dbo.Permissions`
Danh mục các chức năng/quyền hạn trong hệ thống.

| Tên Cột | Kiểu Dữ Liệu | Ràng buộc | Mô tả |
| :--- | :--- | :--- | :--- |
| `Id` | INT | PRIMARY KEY, IDENTITY | Định danh quyền |
| `Code` | NVARCHAR(50) | UNIQUE, NOT NULL | Mã quyền (Ví dụ: 'SELL_TICKET') |
| `Name` | NVARCHAR(100) | NOT NULL | Tên quyền hiển thị |
| `Description` | NVARCHAR(255) | NULL | Mô tả chi tiết quyền |

#### Bảng `dbo.RolePermissions`
Cấu hình quyền mặc định thuộc về từng Role.

| Tên Cột | Kiểu Dữ Liệu | Ràng buộc | Mô tả |
| :--- | :--- | :--- | :--- |
| `RoleId` | INT | FOREIGN KEY -> Roles(Id) | Liên kết vai trò |
| `PermissionId` | INT | FOREIGN KEY -> Permissions(Id) | Liên kết quyền |

#### Bảng `dbo.UserPermissionScopes`
Phân quyền chi tiết theo Trạm bay hoặc Hạn mức/Giới hạn số lượng.

| Tên Cột | Kiểu Dữ Liệu | Ràng buộc | Mô tả |
| :--- | :--- | :--- | :--- |
| `Id` | UNIQUEIDENTIFIER | PRIMARY KEY | Định danh scope |
| `UserId` | UNIQUEIDENTIFIER | FOREIGN KEY -> Users(Id) | Áp dụng cho tài khoản cụ thể |
| `PermissionId` | INT | FOREIGN KEY -> Permissions(Id) | Đi kèm với hành động/quyền nào |
| `AirlineId` | UNIQUEIDENTIFIER | NULL, FOREIGN KEY | Giới hạn theo hãng bay |
| `AirportCode` | VARCHAR(10) | NULL | Giới hạn theo trạm/sân bay (VD: 'SGN') |
| `MaxLimitValue` | INT | NULL | Số lượng tối đa được phép thực hiện |
| `ScopeDescription` | NVARCHAR(255) | NULL | Mô tả phạm vi |
| `CreatedAt` | DATETIME2 | DEFAULT GETUTCDATE() | Thời gian khởi tạo |

---

### 2.2. Phân hệ Chuyến bay (`dbo`)
Quản lý lịch trình, hãng bay, sân bay, máy bay và ghế ngồi chuyến bay.

#### Bảng `dbo.Airlines`
Quản lý các đối tác Hãng hàng không tích hợp hệ thống.

| Tên Cột | Kiểu Dữ Liệu | Ràng buộc | Mô tả |
| :--- | :--- | :--- | :--- |
| `Id` | UNIQUEIDENTIFIER | PRIMARY KEY | Định danh duy nhất hãng bay |
| `IataCode` | NVARCHAR(10) | UNIQUE, NOT NULL | Mã IATA của hãng bay (VD: VN, VJ) |
| `Name` | NVARCHAR(255) | NOT NULL | Tên đầy đủ của hãng bay |
| `LogoUrl` | NVARCHAR(500) | NULL | Đường dẫn ảnh biểu trưng |
| `BaseCountry` | NVARCHAR(100) | NULL | Quốc gia trụ sở chính |
| `ApiEndpoint` | NVARCHAR(500) | NULL | Điểm cuối API kết nối của hãng (B2B) |
| `ApiKey` | NVARCHAR(255) | NULL | Khóa API bảo mật xác thực đối tác |
| `IsActive` | BIT | DEFAULT 1, NOT NULL | Trạng thái tích hợp hoạt động |
| `IsDeleted` | BIT | DEFAULT 0, NOT NULL | Cờ xóa mềm |
| `CreatedAt` | DATETIME2 | DEFAULT GETUTCDATE() | Thời gian tạo lập đối tác |

#### Bảng `dbo.Airports`
Danh sách thông tin sân bay trên thế giới.

| Tên Cột | Kiểu Dữ Liệu | Ràng buộc | Mô tả |
| :--- | :--- | :--- | :--- |
| `Id` | UNIQUEIDENTIFIER | PRIMARY KEY | Định danh duy nhất sân bay |
| `IataCode` | NVARCHAR(10) | UNIQUE, NOT NULL | Mã IATA của sân bay (VD: SGN, HAN) |
| `NameEn` | NVARCHAR(255) | NOT NULL | Tên tiếng Anh của sân bay |
| `NameVi` | NVARCHAR(255) | NOT NULL | Tên tiếng Việt của sân bay |
| `CityEn` | NVARCHAR(100) | NOT NULL | Thành phố tiếng Anh |
| `CityVi` | NVARCHAR(100) | NOT NULL | Thành phố tiếng Việt |
| `CountryCode` | NVARCHAR(10) | NOT NULL | Mã quốc gia theo chuẩn ISO Alpha-2 |
| `Timezone` | NVARCHAR(50) | NOT NULL | Múi giờ sân bay áp dụng |
| `Latitude` | DECIMAL(9, 6) | NULL | Vĩ độ địa lý |
| `Longitude` | DECIMAL(9, 6) | NULL | Kinh độ địa lý |
| `IsActive` | BIT | DEFAULT 1, NOT NULL | Trạng thái hoạt động sân bay |
| `IsDeleted` | BIT | DEFAULT 0, NOT NULL | Cờ xóa mềm |

#### Bảng `dbo.AircraftModels`
Danh mục các dòng/mẫu máy bay trong hệ thống (dùng làm cấu hình mẫu).

| Tên Cột | Kiểu Dữ Liệu | Ràng buộc | Mô tả |
| :--- | :--- | :--- | :--- |
| `Id` | UNIQUEIDENTIFIER | PRIMARY KEY | Định danh duy nhất mẫu máy bay |
| `Name` | NVARCHAR(100) | NOT NULL | Tên mẫu máy bay (VD: Boeing 787-9 Dreamliner) |
| `Manufacturer` | NVARCHAR(100) | NOT NULL | Nhà sản xuất (VD: Boeing, Airbus) |
| `TotalSeats` | INT | NOT NULL | Tổng số ghế theo cấu hình mẫu |
| `IsDeleted` | BIT | DEFAULT 0, NOT NULL | Cờ xóa mềm |

#### Bảng `dbo.AircraftModelSeatTemplates`
Cấu hình sơ đồ ghế mẫu cho từng dòng máy bay.

| Tên Cột | Kiểu Dữ Liệu | Ràng buộc | Mô tả |
| :--- | :--- | :--- | :--- |
| `Id` | UNIQUEIDENTIFIER | PRIMARY KEY | Định danh duy nhất ghế mẫu |
| `AircraftModelId` | UNIQUEIDENTIFIER | FOREIGN KEY -> dbo.AircraftModels(Id) | Thuộc mẫu máy bay nào |
| `SeatNumber` | NVARCHAR(10) | NOT NULL | Số ghế (VD: 1A, 12B) |
| `SeatRow` | NVARCHAR(10) | NOT NULL | Hàng ghế (VD: 1, 12) |
| `SeatColumn` | NVARCHAR(10) | NOT NULL | Cột ghế (VD: A, B) |
| `SeatClass` | INT | NOT NULL | Hạng ghế (0: Economy, 1: PremiumEconomy, 2: Business, 3: FirstClass) |
| `IsExtraLegroom` | BIT | DEFAULT 0, NOT NULL | Ghế có khoảng để chân rộng rãi không |
| `PriceMultiplier` | DECIMAL(18, 2) | DEFAULT 1.0, NOT NULL | Hệ số nhân giá vé cho hạng ghế này |

#### Bảng `dbo.Airplanes`
Hạ tầng đội bay của các hãng hàng không.

| Tên Cột | Kiểu Dữ Liệu | Ràng buộc | Mô tả |
| :--- | :--- | :--- | :--- |
| `Id` | UNIQUEIDENTIFIER | PRIMARY KEY | Định danh duy nhất tàu bay |
| `AirlineId` | UNIQUEIDENTIFIER | FOREIGN KEY -> dbo.Airlines(Id) | Thuộc sở hữu hãng bay nào |
| `AircraftModelId` | UNIQUEIDENTIFIER | NULL, FOREIGN KEY -> dbo.AircraftModels(Id) | Liên kết cấu hình mẫu máy bay |
| `Model` | NVARCHAR(100) | NOT NULL | Dòng máy bay (VD: Boeing 787, Airbus A350) |
| `RegistrationNumber` | NVARCHAR(50) | UNIQUE, NOT NULL | Số đăng ký kiểm soát máy bay |
| `TotalCapacity` | INT | NOT NULL | Tổng tải lượng số ghế |
| `IsDeleted` | BIT | DEFAULT 0, NOT NULL | Cờ xóa mềm |

#### Bảng `dbo.AirplaneSeats`
Sơ đồ ghế thực tế của từng tàu bay cụ thể (được sinh ra từ cấu hình mẫu).

| Tên Cột | Kiểu Dữ Liệu | Ràng buộc | Mô tả |
| :--- | :--- | :--- | :--- |
| `Id` | UNIQUEIDENTIFIER | PRIMARY KEY | Định danh duy nhất ghế tàu bay |
| `AirplaneId` | UNIQUEIDENTIFIER | FOREIGN KEY -> dbo.Airplanes(Id) | Thuộc tàu bay nào |
| `SeatNumber` | NVARCHAR(10) | NOT NULL | Số ghế (VD: 1A, 12B) |
| `SeatRow` | NVARCHAR(10) | NOT NULL | Hàng ghế (VD: 1, 12) |
| `SeatColumn` | NVARCHAR(10) | NOT NULL | Cột ghế (VD: A, B) |
| `SeatClass` | INT | NOT NULL | Hạng ghế (0: Economy, 1: PremiumEconomy, 2: Business, 3: FirstClass) |
| `IsExtraLegroom` | BIT | DEFAULT 0, NOT NULL | Ghế có khoảng để chân rộng rãi không |
| `PriceMultiplier` | DECIMAL(18, 2) | DEFAULT 1.0, NOT NULL | Hệ số nhân giá vé cho hạng ghế này |

#### Bảng `dbo.Routes`
Các tuyến đường bay kết nối giữa các sân bay.

| Tên Cột | Kiểu Dữ Liệu | Ràng buộc | Mô tả |
| :--- | :--- | :--- | :--- |
| `Id` | UNIQUEIDENTIFIER | PRIMARY KEY | Định danh tuyến bay |
| `AirlineId` | UNIQUEIDENTIFIER | FOREIGN KEY -> dbo.Airlines(Id) | Hãng vận hành tuyến bay |
| `OriginAirportId` | UNIQUEIDENTIFIER | FOREIGN KEY -> dbo.Airports(Id) | Sân bay điểm khởi hành |
| `DestinationAirportId` | UNIQUEIDENTIFIER | FOREIGN KEY -> dbo.Airports(Id) | Sân bay điểm đến |
| `DistanceKm` | DECIMAL(10, 2) | NULL | Khoảng cách tuyến bay (km) |
| `EstimatedDurationMinutes` | INT | NULL | Thời gian bay dự kiến (phút) |
| `IsDeleted` | BIT | DEFAULT 0, NOT NULL | Cờ xóa mềm |

#### Bảng `dbo.Flights`
Thông tin chi tiết chuyến bay thực tế theo thời gian.

| Tên Cột | Kiểu Dữ Liệu | Ràng buộc | Mô tả |
| :--- | :--- | :--- | :--- |
| `Id` | UNIQUEIDENTIFIER | PRIMARY KEY | Định danh duy nhất chuyến bay |
| `RouteId` | UNIQUEIDENTIFIER | FOREIGN KEY -> dbo.Routes(Id) | Tuyến đường bay chi tiết |
| `AirplaneId` | UNIQUEIDENTIFIER | FOREIGN KEY -> dbo.Airplanes(Id) | Tàu bay được phân công |
| `FlightNumber` | NVARCHAR(20) | NOT NULL | Số hiệu chuyến bay (VD: VN213) |
| `DepartureTime` | DATETIME2 | NOT NULL | Thời gian cất cánh dự kiến |
| `ArrivalTime` | DATETIME2 | NOT NULL | Thời gian hạ cánh dự kiến |
| `BasePrice` | DECIMAL(18, 2) | NOT NULL | Giá vé cơ sở |
| `Currency` | NVARCHAR(3) | DEFAULT 'USD' | Tiền tệ áp dụng |
| `Status` | INT | DEFAULT 0, NOT NULL | Trạng thái (0: Scheduled, 1: Delayed, 2: Boarding, 3: InAir, 4: Landed, 5: Cancelled) |
| `ExternalId` | NVARCHAR(100) | NULL | ID đồng bộ hệ thống ngoài của đối tác |
| `IsDeleted` | BIT | DEFAULT 0, NOT NULL | Cờ xóa mềm |
| `CreatedAt` | DATETIME2 | DEFAULT GETUTCDATE() | Ngày giờ khởi tạo |
| `UpdatedAt` | DATETIME2 | DEFAULT GETUTCDATE() | Ngày giờ cập nhật |

#### Bảng `dbo.FlightSeats`
Quản lý trạng thái và sơ đồ ghế cụ thể của từng chuyến bay.

| Tên Cột | Kiểu Dữ Liệu | Ràng buộc | Mô tả |
| :--- | :--- | :--- | :--- |
| `Id` | UNIQUEIDENTIFIER | PRIMARY KEY | Định danh duy nhất ghế chuyến bay |
| `FlightId` | UNIQUEIDENTIFIER | FOREIGN KEY -> dbo.Flights(Id) | Thuộc chuyến bay nào |
| `SeatNumber` | NVARCHAR(10) | NOT NULL | Số ghế vật lý (VD: 12A, 1A) |
| `SeatClass` | INT | NOT NULL | Hạng ghế (0: Economy, 1: PremiumEconomy, 2: Business, 3: FirstClass) |
| `PriceOverride` | DECIMAL(18, 2) | NULL | Giá bán riêng cho ghế (nếu có) |
| `IsAvailable` | BIT | DEFAULT 1, NOT NULL | Trạng thái còn trống hay không |
| `IsExtraLegroom` | BIT | DEFAULT 0, NOT NULL | Ghế có khoảng để chân rộng rãi không |

---

### 2.3. Phân hệ Đặt chỗ & Vé (`dbo`)
Quản lý đơn đặt chỗ, thanh toán, thông tin hành khách và vé máy bay điện tử.

#### Bảng `dbo.Bookings`
Thông tin đơn đặt chỗ (Booking/PNR) của khách hàng.

| Tên Cột | Kiểu Dữ Liệu | Ràng buộc | Mô tả |
| :--- | :--- | :--- | :--- |
| `Id` | UNIQUEIDENTIFIER | PRIMARY KEY | Định danh duy nhất đơn đặt chỗ |
| `UserId` | UNIQUEIDENTIFIER | FOREIGN KEY -> dbo.Users(Id) | Người thực hiện đặt vé |
| `PnrCode` | NVARCHAR(10) | UNIQUE, NOT NULL | Mã đặt chỗ PNR quốc tế (VD: XF89QD) |
| `TotalPrice` | DECIMAL(18, 2) | NOT NULL | Tổng tiền đơn hàng |
| `Currency` | NVARCHAR(3) | DEFAULT 'USD' | Tiền tệ thanh toán |
| `Status` | INT | DEFAULT 0, NOT NULL | Trạng thái (0: Pending, 1: Paid, 2: Confirmed, 3: Cancelled, 4: Refunded) |
| `ContactEmail` | NVARCHAR(255) | NOT NULL | Email liên hệ nhận vé điện tử |
| `ContactPhone` | NVARCHAR(20) | NOT NULL | Số điện thoại liên hệ |
| `SpecialRequests` | NVARCHAR(MAX) | NULL | Yêu cầu đặc biệt (suất ăn, xe lăn...) |
| `IsDeleted` | BIT | DEFAULT 0, NOT NULL | Cờ xóa mềm |
| `CreatedAt` | DATETIME2 | DEFAULT GETUTCDATE() | Thời gian tạo đơn |
| `UpdatedAt` | DATETIME2 | DEFAULT GETUTCDATE() | Thời gian cập nhật trạng thái đơn |

#### Bảng `dbo.Passengers`
Thông tin giấy tờ tùy thân của từng hành khách bay.

| Tên Cột | Kiểu Dữ Liệu | Ràng buộc | Mô tả |
| :--- | :--- | :--- | :--- |
| `Id` | UNIQUEIDENTIFIER | PRIMARY KEY | Định danh hành khách |
| `BookingId` | UNIQUEIDENTIFIER | FOREIGN KEY -> dbo.Bookings(Id) | Thuộc mã đơn đặt chỗ nào |
| `FirstName` | NVARCHAR(100) | NOT NULL | Tên đệm và tên của hành khách |
| `LastName` | NVARCHAR(100) | NOT NULL | Họ của hành khách |
| `Gender` | INT | NULL | Giới tính (0: Nam, 1: Nữ, 2: Khác) |
| `DateOfBirth` | DATE | NOT NULL | Ngày tháng năm sinh |
| `Nationality` | NVARCHAR(100) | NULL | Quốc tịch |
| `PassportNumber` | NVARCHAR(50) | NOT NULL | Số hộ chiếu/CCCD |
| `PassportExpiryDate` | DATE | NOT NULL | Ngày hết hạn hộ chiếu |

#### Bảng `dbo.Tickets`
Thông tin vé máy bay điện tử (E-ticket) phát hành cho hành khách.

| Tên Cột | Kiểu Dữ Liệu | Ràng buộc | Mô tả |
| :--- | :--- | :--- | :--- |
| `Id` | UNIQUEIDENTIFIER | PRIMARY KEY | Định danh vé |
| `BookingId` | UNIQUEIDENTIFIER | FOREIGN KEY -> dbo.Bookings(Id) | Liên kết với đơn đặt vé |
| `PassengerId` | UNIQUEIDENTIFIER | FOREIGN KEY -> dbo.Passengers(Id) | Chủ nhân của vé |
| `FlightId` | UNIQUEIDENTIFIER | FOREIGN KEY -> flights.Flights(Id) | Áp dụng cho chuyến bay nào |
| `SeatId` | UNIQUEIDENTIFIER | FOREIGN KEY -> flights.FlightSeats(Id) | Vị trí ghế ngồi |
| `TicketNumber` | NVARCHAR(50) | UNIQUE, NOT NULL | Số vé điện tử (Unique E-ticket Number) |
| `Gate` | NVARCHAR(20) | NULL | Cửa ra tàu bay |
| `BoardingTime` | DATETIME2 | NULL | Thời điểm lên tàu bay |
| `Status` | INT | DEFAULT 0, NOT NULL | Trạng thái (0: Valid, 1: CheckedIn, 2: Used, 3: Cancelled) |

#### Bảng `dbo.Payments`
Lịch sử giao dịch thanh toán vé.

| Tên Cột | Kiểu Dữ Liệu | Ràng buộc | Mô tả |
| :--- | :--- | :--- | :--- |
| `Id` | UNIQUEIDENTIFIER | PRIMARY KEY | Định danh giao dịch |
| `BookingId` | UNIQUEIDENTIFIER | FOREIGN KEY -> dbo.Bookings(Id) | Thanh toán cho đơn hàng nào |
| `TransactionId` | NVARCHAR(100) | UNIQUE, NOT NULL | Mã giao dịch từ cổng thanh toán bên thứ ba |
| `Amount` | DECIMAL(18, 2) | NOT NULL | Số tiền thanh toán thực tế |
| `PaymentMethod` | NVARCHAR(50) | NOT NULL | Cổng thanh toán (Stripe, Paypal, VNPay...) |
| `ProviderStatus` | NVARCHAR(50) | NULL | Trạng thái trả về từ cổng thanh toán |
| `IsSuccessful` | BIT | DEFAULT 0, NOT NULL | Trạng thái thành công hay thất bại |
| `RawResponse` | NVARCHAR(MAX) | NULL | Phản hồi chi tiết (JSON) từ đối tác thanh toán |
| `CreatedAt` | DATETIME2 | DEFAULT GETUTCDATE() | Thời gian thực hiện giao dịch |

---

### 2.4. Phân hệ Khuyến mãi (`dbo`)
Quản lý các chiến dịch marketing, coupon giảm giá sản phẩm vé máy bay.

#### Bảng `dbo.Coupons`
Thông tin mã giảm giá khuyến mãi áp dụng trực tiếp cho giỏ hàng đặt chỗ.

| Tên Cột | Kiểu Dữ Liệu | Ràng buộc | Mô tả |
| :--- | :--- | :--- | :--- |
| `Id` | UNIQUEIDENTIFIER | PRIMARY KEY | Định danh mã giảm giá |
| `Code` | NVARCHAR(50) | UNIQUE, NOT NULL | Ký tự mã giảm giá (VD: SUMMER2026) |
| `Description` | NVARCHAR(500) | NULL | Nội dung mô tả chương trình |
| `DiscountType` | INT | NOT NULL | Kiểu chiết khấu (0: Phần trăm, 1: Số tiền cố định) |
| `DiscountValue` | DECIMAL(18, 2) | NOT NULL | Giá trị được chiết khấu |
| `MinOrderValue` | DECIMAL(18, 2) | NULL | Giá trị đơn đặt vé tối thiểu được áp dụng |
| `MaxDiscountAmount` | DECIMAL(18, 2) | NULL | Giá trị giảm giá tối đa cho kiểu chiết khấu % |
| `StartDate` | DATETIME2 | NOT NULL | Thời gian hiệu lực bắt đầu |
| `EndDate` | DATETIME2 | NOT NULL | Thời gian hết hiệu lực |
| `UsageLimit` | INT | NULL | Số lượng lượt sử dụng tối đa của mã |
| `UsageCount` | INT | DEFAULT 0 | Số lượt đã sử dụng thực tế |
| `IsActive` | BIT | DEFAULT 1 | Trạng thái hoạt động của coupon |
| `IsDeleted` | BIT | DEFAULT 0 | Cờ xóa mềm |

#### Bảng `dbo.Campaigns`
Lưu trữ thông tin quảng cáo banner chiến dịch.

| Tên Cột | Kiểu Dữ Liệu | Ràng buộc | Mô tả |
| :--- | :--- | :--- | :--- |
| `Id` | UNIQUEIDENTIFIER | PRIMARY KEY | Định danh chiến dịch |
| `Title` | NVARCHAR(255) | NOT NULL | Tiêu đề chiến dịch quảng cáo |
| `BannerUrl` | NVARCHAR(500) | NULL | Ảnh quảng cáo chiến dịch |
| `Content` | NVARCHAR(MAX) | NULL | Nội dung giới thiệu chiến dịch |
| `StartDate` | DATETIME2 | NOT NULL | Thời gian bắt đầu chiến dịch |
| `EndDate` | DATETIME2 | NOT NULL | Thời gian kết thúc chiến dịch |
| `IsFeatured` | BIT | DEFAULT 0 | Đánh dấu nổi bật tại trang chủ |
| `IsDeleted` | BIT | DEFAULT 0 | Cờ xóa mềm |

---

### 2.5. Phân hệ Đánh giá & Phản hồi (`dbo`)
Lưu nhận xét thực tế từ phía khách hàng sau trải nghiệm dịch vụ.

#### Bảng `dbo.Reviews`
Đánh giá chất lượng dịch vụ của hãng bay hoặc chuyến bay cụ thể.

| Tên Cột | Kiểu Dữ Liệu | Ràng buộc | Mô tả |
| :--- | :--- | :--- | :--- |
| `Id` | UNIQUEIDENTIFIER | PRIMARY KEY | Định danh đánh giá |
| `UserId` | UNIQUEIDENTIFIER | FOREIGN KEY -> Users(Id) | Người đánh giá |
| `AirlineId` | UNIQUEIDENTIFIER | FOREIGN KEY -> Airlines(Id) | Hãng hàng không được đánh giá |
| `FlightId` | UNIQUEIDENTIFIER | FOREIGN KEY -> Flights(Id) | Chuyến bay cụ thể được đánh giá |
| `Rating` | INT | CHECK (1-5), NOT NULL | Xếp hạng số sao (từ 1 đến 5) |
| `Comment` | NVARCHAR(MAX) | NULL | Nhận xét chi tiết của hành khách |
| `IsVerifiedPurchase` | BIT | DEFAULT 0 | Trạng thái người dùng đã thực hiện bay thực tế |
| `IsHidden` | BIT | DEFAULT 0 | Quản trị viên ẩn bài đánh giá (spam, thô tục) |
| `IsDeleted` | BIT | DEFAULT 0 | Cờ xóa mềm |
| `CreatedAt` | DATETIME2 | DEFAULT GETUTCDATE() | Thời gian đánh giá |

#### Bảng `dbo.ChatMessages`
Lưu trữ tin nhắn trò chuyện giữa khách hàng và nhân viên hỗ trợ.

| Tên Cột | Kiểu Dữ Liệu | Ràng buộc | Mô tả |
| :--- | :--- | :--- | :--- |
| `Id` | UNIQUEIDENTIFIER | PRIMARY KEY | Định danh tin nhắn |
| `AirlineId` | UNIQUEIDENTIFIER | NOT NULL | ID hãng bay liên quan |
| `SenderRole` | NVARCHAR(50) | NOT NULL | Vai trò người gửi (Customer, Staff) |
| `SenderName` | NVARCHAR(255) | NOT NULL | Tên người gửi |
| `CustomerConnectionId` | NVARCHAR(255) | NULL | Connection ID của khách hàng (SignalR) |
| `StaffConnectionId` | NVARCHAR(255) | NULL | Connection ID của nhân viên (SignalR) |
| `Content` | NVARCHAR(MAX) | NOT NULL | Nội dung tin nhắn |
| `SentAt` | DATETIME2 | NOT NULL | Thời gian gửi tin nhắn |

---

### 2.6. Phân hệ CMS Nội dung (`dbo`)
Phân phối bài viết du lịch hàng không, cẩm nang và blog địa điểm.

#### Bảng `dbo.Categories`
Các danh mục nội dung bài viết.

| Tên Cột | Kiểu Dữ Liệu | Ràng buộc | Mô tả |
| :--- | :--- | :--- | :--- |
| `Id` | UNIQUEIDENTIFIER | PRIMARY KEY | Định danh danh mục |
| `Name` | NVARCHAR(100) | NOT NULL | Tên danh mục (VD: Mẹo du lịch, Điểm đến) |
| `Slug` | NVARCHAR(100) | UNIQUE, NOT NULL | Đường dẫn thân thiện SEO của danh mục |
| `IsDeleted` | BIT | DEFAULT 0 | Cờ xóa mềm |

#### Bảng `dbo.Articles`
Lưu trữ thông tin chi tiết các bài báo của trang quản trị nội dung.

| Tên Cột | Kiểu Dữ Liệu | Ràng buộc | Mô tả |
| :--- | :--- | :--- | :--- |
| `Id` | UNIQUEIDENTIFIER | PRIMARY KEY | Định danh bài viết |
| `CategoryId` | UNIQUEIDENTIFIER | FOREIGN KEY -> Categories(Id) | Danh mục bài viết trực thuộc |
| `AuthorId` | UNIQUEIDENTIFIER | FOREIGN KEY -> Users(Id) | Tác giả viết bài |
| `Title` | NVARCHAR(255) | NOT NULL | Tiêu đề bài viết |
| `Slug` | NVARCHAR(255) | UNIQUE, NOT NULL | Đường dẫn thân thiện SEO của bài viết |
| `Summary` | NVARCHAR(500) | NULL | Mô tả ngắn tóm tắt bài viết |
| `Content` | NVARCHAR(MAX) | NOT NULL | Nội dung bài viết chi tiết dạng HTML/Markdown |
| `ThumbnailUrl` | NVARCHAR(500) | NULL | Ảnh thu nhỏ hiển thị bài viết |
| `PublishedAt` | DATETIME2 | NULL | Thời gian xuất bản bài viết lên trang chủ |
| `Status` | INT | DEFAULT 0, NOT NULL | Trạng thái bài viết (0: Nháp, 1: Đã duyệt, 2: Lưu trữ) |
| `ViewCount` | INT | DEFAULT 0 | Tổng lượt xem bài viết |
| `IsDeleted` | BIT | DEFAULT 0 | Cờ xóa mềm |
| `CreatedAt` | DATETIME2 | DEFAULT GETUTCDATE() | Thời điểm tạo bài viết |

---

### 2.7. Phân hệ Nhật ký Hệ thống (`dbo`)
Ghi log hoạt động hệ thống.

#### Bảng `dbo.SystemLogs`
Lưu trữ log hệ thống và lỗi runtime.

| Tên Cột | Kiểu Dữ Liệu | Ràng buộc | Mô tả |
| :--- | :--- | :--- | :--- |
| `Id` | UNIQUEIDENTIFIER | PRIMARY KEY | Định danh log |
| `Level` | NVARCHAR(50) | NOT NULL | Cấp độ log (Info, Warning, Error, Critical) |
| `Message` | NVARCHAR(MAX) | NOT NULL | Nội dung log |
| `Source` | NVARCHAR(255) | NULL | Nguồn ghi log (Application, Module...) |
| `Exception` | NVARCHAR(MAX) | NULL | Chi tiết ngoại lệ |
| `UserId` | UNIQUEIDENTIFIER | NULL | ID người dùng thực hiện (nếu có) |
| `AirlineId` | UNIQUEIDENTIFIER | NULL | ID hãng bay liên quan (nếu có) |
| `IpAddress` | NVARCHAR(50) | NULL | Địa chỉ IP |
| `CreatedAt` | DATETIME2 | DEFAULT GETUTCDATE() | Thời gian ghi log |

---

### 2.8. Phân hệ Thông báo (`dbo`)
Quản lý mẫu thông báo và hàng đợi gửi thông báo (Email, SMS, Push, SignalR).

#### Bảng `dbo.NotificationTemplates`
Lưu trữ các mẫu thông báo (template) có sẵn cho từng loại sự kiện.

| Tên Cột | Kiểu Dữ Liệu | Ràng buộc | Mô tả |
| :--- | :--- | :--- | :--- |
| `Id` | UNIQUEIDENTIFIER | PRIMARY KEY | Định danh template |
| `Code` | NVARCHAR(100) | UNIQUE, NOT NULL | Mã template (VD: 'BOOKING_CONFIRMED', 'FLIGHT_DELAYED') |
| `Subject` | NVARCHAR(255) | NOT NULL | Tiêu đề thông báo |
| `BodyTemplate` | NVARCHAR(MAX) | NOT NULL | Nội dung template (hỗ trợ placeholder) |
| `Language` | NVARCHAR(10) | DEFAULT 'vi' | Ngôn ngữ template |
| `CreatedAt` | DATETIME2 | DEFAULT GETUTCDATE() | Thời gian tạo |

#### Bảng `dbo.Notifications`
Lưu trữ thông báo đã gửi hoặc chờ gửi đến người dùng.

| Tên Cột | Kiểu Dữ Liệu | Ràng buộc | Mô tả |
| :--- | :--- | :--- | :--- |
| `Id` | UNIQUEIDENTIFIER | PRIMARY KEY | Định danh thông báo |
| `UserId` | UNIQUEIDENTIFIER | NULL, FK -> Users(Id) | Người nhận (nếu có tài khoản) |
| `Recipient` | NVARCHAR(255) | NOT NULL | Email hoặc số điện thoại |
| `Subject` | NVARCHAR(255) | NULL | Tiêu đề |
| `Content` | NVARCHAR(MAX) | NOT NULL | Nội dung thông báo |
| `Type` | INT | NOT NULL | Loại: 0: Email, 1: SMS, 2: Push, 3: SignalR |
| `Status` | INT | DEFAULT 0 | Trạng thái: 0: Pending, 1: Sent, 2: Failed |
| `RetryCount` | INT | DEFAULT 0 | Số lần thử gửi lại |
| `ErrorMessage` | NVARCHAR(MAX) | NULL | Thông báo lỗi (nếu gửi thất bại) |
| `SentAt` | DATETIME2 | NULL | Thời điểm gửi thành công |
| `CreatedAt` | DATETIME2 | DEFAULT GETUTCDATE() | Thời gian tạo |

---

## 3. Indexes hiệu năng

Các chỉ mục được tạo nhằm tối ưu tốc độ truy vấn cho các bảng có khối lượng giao dịch lớn.

| Tên Index | Bảng | Cột | Mục đích |
| :--- | :--- | :--- | :--- |
| `IX_Flights_Departure` | flights.Flights | DepartureTime | Tra cứu chuyến bay theo giờ khởi hành |
| `IX_Flights_Route` | flights.Flights | RouteId | Lọc chuyến bay theo tuyến đường |
| `IX_Bookings_Pnr` | dbo.Bookings | PnrCode | Tra cứu đơn đặt chỗ theo mã PNR |
| `IX_Bookings_User` | dbo.Bookings | UserId | Liệt kê đơn đặt chỗ theo người dùng |
| `IX_Tickets_Number` | dbo.Tickets | TicketNumber | Tra cứu vé theo số vé điện tử |
| `IX_Notifications_User` | dbo.Notifications | UserId | Lấy danh sách thông báo của người dùng |

## 4. Soft Delete (Xóa mềm)

Hệ thống áp dụng Soft Delete pattern thông qua SQL Server INSTEAD OF DELETE triggers. Khi thực hiện lệnh DELETE, trigger sẽ tự động chuyển thành UPDATE `IsDeleted = 1` thay vì xóa vật lý.

Các bảng được áp dụng:

| Schema | Bảng |
| :--- | :--- |
| dbo | Users, Bookings, Coupons, Campaigns, Reviews, ChatMessages, Categories, Articles, SystemLogs, NotificationTemplates, Notifications |
| flights | Airlines, Airports, Airplanes, Routes, Flights

Lợi ích của Soft Delete:
- Giữ lại dữ liệu lịch sử cho mục đích kiểm toán (audit) và báo cáo.
- Tránh lỗi vi phạm khóa ngoại (FK violation) khi dữ liệu con đang được tham chiếu.
- Dễ dàng khôi phục (restore) dữ liệu khi cần thiết.
