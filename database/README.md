# Database Initialization

## Tập tin SQL

### `init_v1.sql`
File SQL chính để khởi tạo database cho phiên bản v1 của hệ thống Airline Ticket.

## Nội dung

File `init_v1.sql` tạo:

### 1. Schemas
- `flights` - Quản lý dữ liệu về chuyến bay
- `bookings` - Quản lý đặt chỗ và vé
- `users` - Quản lý người dùng
- `promotions` - Quản lý khuyến mãi

### 2. Bảng (Tables)

#### Schema `flights`
- **Airports** - Sân bay
- **Airlines** - Hãng hàng không
- **Airplanes** - Máy bay
- **AirplaneSeats** - Cấu hình ghế mẫu của máy bay
- **Routes** - Tuyến bay
- **Flights** - Chuyến bay thực tế
- **FlightSeats** - Ghế của chuyến bay cụ thể

#### Schema `bookings`
- **Bookings** - Đơn đặt chỗ
- **Passengers** - Hành khách
- **Tickets** - Vé điện tử
- **Payments** - Thanh toán

#### Schema `users`
- **Users** - Người dùng hệ thống

#### Schema `promotions`
- **Campaigns** - Chiến dịch giảm giá
- **Coupons** - Mã giảm giá

### 3. Indexes
Tạo các chỉ mục (indexes) để tối ưu hóa truy vấn trên các trường thường được tìm kiếm.

### 4. Dữ liệu Mẫu (Seed Data)
- 5 sân bay mẫu (SGN, HAN, DAD, NRT, ICN)
- 3 hãng hàng không mẫu
- 2 máy bay mẫu
- Cấu hình ghế cho máy bay
- Tuyến bay mẫu
- 1 user Admin và 2 user Customer
- 2 mã giảm giá

## Cách sử dụng

### SQL Server Management Studio (SSMS)
1. Mở SQL Server Management Studio
2. Kết nối đến SQL Server instance của bạn
3. Nhấp chuột phải trên **Databases** > **New Database**
4. Nhập tên database: `AirlineTicketDB`
5. Chọn database vừa tạo
6. Mở file `init_v1.sql`
7. Nhấn **Execute** (Ctrl+Shift+E)

### Command Line (SQL Server)
```bash
sqlcmd -S <server_name> -U <username> -P <password> -d AirlineTicketDB -i init_v1.sql
```

### Với Docker (nếu chạy SQL Server trong Docker)
```bash
docker exec -i <container_name> /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P <password> -d AirlineTicketDB -i init_v1.sql
```

## Enum Values Reference

### Flight Status
- `0` - Scheduled (Đã lên lịch)
- `1` - Delayed (Bị hoãn)
- `2` - Boarding (Lên máy bay)
- `3` - InAir (Đang bay)
- `4` - Landed (Đã hạ cánh)
- `5` - Cancelled (Hủy)

### Seat Class
- `0` - Economy (Hạng phổ thông)
- `1` - Business (Hạng thương gia)
- `2` - First (Hạng nhất)

### Booking Status
- `0` - Pending (Chờ thanh toán)
- `1` - Confirmed (Đã xác nhận)
- `2` - Cancelled (Đã hủy)
- `3` - Refunded (Đã hoàn tiền)

### Payment Status
- `0` - Pending (Chờ xử lý)
- `1` - Completed (Hoàn thành)
- `2` - Failed (Thất bại)

### Ticket Status
- `0` - Issued (Đã phát hành)
- `1` - Used (Đã sử dụng)
- `2` - Cancelled (Đã hủy)

### User Roles
- `0` - Customer (Khách hàng)
- `1` - Admin (Quản trị viên)
- `2` - Staff (Nhân viên)

### Discount Type
- `0` - Percentage (Giảm phần trăm)
- `1` - FixedAmount (Giảm số tiền cố định)

## Notes
- Tất cả ID sử dụng UNIQUEIDENTIFIER (GUID)
- Các trường `CreatedAt` và `UpdatedAt` được tự động gán giá trị
- Foreign keys được thiết lập để đảm bảo tính toàn vẹn dữ liệu
- File này có thể chạy lặp lại nhiều lần (sẽ drop các bảng cũ nếu tồn tại)
