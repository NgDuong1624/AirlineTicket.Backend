# Danh sách API hệ thống đặt vé máy bay (Airline Ticket Booking System)

Tài liệu này tổng hợp **đầy đủ** danh sách các API endpoint của hệ thống, được trích xuất trực tiếp từ mã nguồn backend.

---

## 1. Authentication APIs (Xác thực người dùng)

| Method | Endpoint | Description | Auth |
| :--- | :--- | :--- | :--- |
| **POST** | `/api/auth/login` | Đăng nhập (trả về JWT token) | Anonymous |
| **POST** | `/api/auth/google` | Đăng nhập bằng Google | Anonymous |
| **POST** | `/api/auth/register` | Đăng ký tài khoản mới | Anonymous |
| **GET** | `/api/auth/me` | Thông tin tài khoản đang đăng nhập | Authenticated |
| **POST** | `/api/auth/refresh` | Làm mới access token | Anonymous |
| **POST** | `/api/auth/logout` | Đăng xuất và thu hồi session | Anonymous |

---

## 2. Flights APIs (Tìm kiếm chuyến bay - Public)

| Method | Endpoint | Description | Auth |
| :--- | :--- | :--- | :--- |
| **POST** | `/api/flights` | Tìm kiếm chuyến bay | Anonymous |
| **GET** | `/api/airports` | Danh sách/tìm kiếm sân bay | Anonymous |
| **GET** | `/api/airlines` | Danh sách hãng hàng không | Anonymous |
| **GET** | `/api/routes` | Danh sách tuyến bay | Anonymous |

---

## 3. Bookings APIs (Đặt vé - Customer)

| Method | Endpoint | Description | Auth |
| :--- | :--- | :--- | :--- |
| **POST** | `/api/bookings` | Tạo đơn đặt chỗ | Anonymous |
| **GET** | `/api/bookings` | Danh sách tất cả đặt vé | PartnerOrStaff |
| **GET** | `/api/bookings/{id}` | Chi tiết đơn đặt chỗ | Anonymous |
| **GET** | `/api/bookings/my-bookings` | Lịch sử đặt vé người dùng | Authenticated |

---

## 4. Notifications APIs (Thông báo)

| Method | Endpoint | Description | Auth |
| :--- | :--- | :--- | :--- |
| **GET** | `/api/notifications/status` | Trạng thái module thông báo | Anonymous |
| **GET** | `/api/notifications` | Danh sách thông báo (phân trang) | Authenticated |
| **GET** | `/api/notifications/unread-count` | Số lượng thông báo chưa đọc | Authenticated |
| **PUT** | `/api/notifications/{id}/read` | Đánh dấu đã đọc | Authenticated |

---

## 5. AI Chat & Interactions APIs (Trợ lý AI & Tương tác)

| Method | Endpoint | Description | Auth |
| :--- | :--- | :--- | :--- |
| **GET** | `/api/interactions` | Trạng thái module tương tác | Anonymous |
| **POST** | `/api/qa/ask` | Gửi câu hỏi cho AI Travel Assistant | Anonymous |

---

## 6. Staff APIs (Nhân viên hãng bay - Role: PartnerOrStaff)

| Method | Endpoint | Description | Auth |
| :--- | :--- | :--- | :--- |
| **GET** | `/api/staff/flights` | Danh sách chuyến bay (staff portal) | PartnerOrStaff |
| **GET** | `/api/staff/flights/{id}/seats` | Sơ đồ ghế chuyến bay | PartnerOrStaff |
| **POST** | `/api/staff/bookings` | Đặt vé hộ khách hàng | PartnerOrStaff |
| **GET** | `/api/staff/sales` | Bảng bán vé (ticket-sales board) | PartnerOrStaff |

---

## 7. Partner APIs (Đối tác hãng bay - Role: PartnerOnly)

| Method | Endpoint | Description | Auth |
| :--- | :--- | :--- | :--- |
| **GET** | `/api/partner/bookings` | Danh sách đặt vé của đối tác | PartnerOnly |
| **PUT** | `/api/partner/bookings/{id}` | Cập nhật trạng thái đặt vé | PartnerOnly |
| **GET** | `/api/partner/dashboard/sales-summary` | Tổng quan doanh thu | PartnerOnly |
| **GET** | `/api/partner/dashboard/occupancy-rates` | Tỷ lệ lấp đầy | PartnerOnly |
| **GET** | `/api/partner/dashboard/revenue-trends` | Xu hướng doanh thu | PartnerOnly |
| **GET** | `/api/partner/coupons` | Danh sách mã giảm giá | PartnerOnly |
| **POST** | `/api/partner/coupons` | Tạo mã giảm giá mới | PartnerOnly |
| **PUT** | `/api/partner/coupons/{id}` | Cập nhật mã giảm giá | PartnerOnly |
| **GET** | `/api/partner/routes` | Danh sách tuyến bay | PartnerOnly |
| **POST** | `/api/partner/routes` | Tạo tuyến bay mới | PartnerOnly |
| **PUT** | `/api/partner/routes/{id}` | Cập nhật tuyến bay | PartnerOnly |
| **DELETE** | `/api/partner/routes/{id}` | Xóa tuyến bay | PartnerOnly |
| **GET** | `/api/partner/logs` | Nhật ký hệ thống của đối tác | PartnerOnly |
| **GET** | `/api/partner/dashboard` | Thống kê dashboard đối tác | PartnerOnly |

---

## 8. Admin APIs (Quản trị - Role: AdminOnly)

### 8.1. Quản lý Người dùng & Phân quyền
| Method | Endpoint | Description | Auth |
| :--- | :--- | :--- | :--- |
| **GET** | `/api/admin/users` | Danh sách người dùng | AdminOnly |
| **POST** | `/api/admin/users` | Tạo người dùng | AdminOnly |
| **GET** | `/api/admin/users/{id}` | Chi tiết người dùng | AdminOnly |
| **PUT** | `/api/admin/users/{id}` | Cập nhật người dùng | AdminOnly |
| **DELETE** | `/api/admin/users/{id}` | Xóa người dùng | AdminOnly |
| **PATCH** | `/api/admin/users/{id}/status` | Cập nhật trạng thái người dùng | AdminOnly |
| **GET** | `/api/admin/permissions` | Danh sách quyền hạn | AdminOnly |

### 8.2. Quản lý Đặt vé
| Method | Endpoint | Description | Auth |
| :--- | :--- | :--- | :--- |
| **GET** | `/api/admin/bookings` | Danh sách đặt vé | AdminOnly |
| **GET** | `/api/admin/bookings/{id}` | Chi tiết đặt vé | AdminOnly |
| **PUT** | `/api/admin/bookings/{id}` | Cập nhật đặt vé | AdminOnly |
| **PUT** | `/api/admin/bookings/{id}/status` | Cập nhật trạng thái đặt vé | AdminOnly |
| **DELETE** | `/api/admin/bookings/{id}` | Hủy đặt vé | AdminOnly |

### 8.3. Quản lý Khuyến mãi
| Method | Endpoint | Description | Auth |
| :--- | :--- | :--- | :--- |
| **GET** | `/api/admin/coupons` | Danh sách mã giảm giá | AdminOnly |
| **POST** | `/api/admin/coupons` | Tạo mã giảm giá | AdminOnly |
| **PUT** | `/api/admin/coupons/{id}` | Cập nhật mã giảm giá | AdminOnly |
| **DELETE** | `/api/admin/coupons/{id}` | Xóa mã giảm giá | AdminOnly |

### 8.4. Quản lý Hạ tầng bay
| Method | Endpoint | Description | Auth |
| :--- | :--- | :--- | :--- |
| **GET** | `/api/admin/airports` | Danh sách sân bay | AdminOnly |
| **POST** | `/api/admin/airports` | Tạo sân bay | AdminOnly |
| **PUT** | `/api/admin/airports/{id}` | Cập nhật sân bay | AdminOnly |

### 8.5. Hệ thống & Dashboard
| Method | Endpoint | Description | Auth |
| :--- | :--- | :--- | :--- |
| **GET** | `/api/admin/logs` | Nhật ký hệ thống | AdminOnly |
| **GET** | `/api/admin/dashboard` | Thống kê dashboard admin | AdminOnly |
| **GET** | `/api/admin/settings` | Cài đặt hệ thống | AdminOnly |
| **PUT** | `/api/admin/settings` | Cập nhật cài đặt hệ thống | AdminOnly |

---

## 9. Health Checks

| Method | Endpoint | Description | Auth |
| :--- | :--- | :--- | :--- |
| **GET** | `/api/health/live` | Kiểm tra liveness | Anonymous |
| **GET** | `/api/health` | Kiểm tra readiness | Anonymous |

### 9.2. Quản lý Quyền hạn
| Method | Endpoint | Description |
| :--- | :--- | :--- |
| **GET** | `/api/admin/permissions` | Danh sách quyền hệ thống |
| **GET** | `/api/admin/user-permissions/{userId}` | Danh sách quyền chi tiết của user |
| **POST** | `/api/admin/user-permissions/{userId}` | Gán quyền (permissionId, airlineId?, airportCode?, scopeDescription?) |
| **DELETE** | `/api/admin/user-permissions/{userId}/{permissionId}` | Xóa quyền |

### 9.3. Quản lý Sân bay
| Method | Endpoint | Description |
| :--- | :--- | :--- |
| **GET** | `/api/admin/airports` | Danh sách sân bay |
| **POST** | `/api/admin/airports` | Tạo sân bay (iataCode, nameEn/Vi, cityEn/Vi, countryCode, timezone, isActive) |
| **PUT** | `/api/admin/airports/{id}` | Cập nhật sân bay |
| **DELETE** | `/api/admin/airports/{id}` | Xóa sân bay |

### 9.4. Quản lý Hãng hàng không
| Method | Endpoint | Description |
| :--- | :--- | :--- |
| **GET** | `/api/admin/airlines` | Danh sách hãng bay |
| **POST** | `/api/admin/airlines` | Tạo hãng bay (iataCode, name, logoUrl?, baseCountry?, isActive?) |
| **PUT** | `/api/admin/airlines/{id}` | Cập nhật hãng bay |
| **DELETE** | `/api/admin/airlines/{id}` | Xóa hãng bay |

### 9.5. Quản lý Chuyến bay
| Method | Endpoint | Description |
| :--- | :--- | :--- |
| **GET** | `/api/admin/flights` | Danh sách chuyến bay |
| **POST** | `/api/admin/flights` | Tạo chuyến bay (routeId, airplaneId, flightNumber, basePrice, scheduledDeparture, scheduledArrival) |
| **PUT** | `/api/admin/flights/{id}` | Cập nhật chuyến bay |
| **DELETE** | `/api/admin/flights/{id}` | Xóa chuyến bay |

### 9.6. Quản lý Đặt vé
| Method | Endpoint | Description |
| :--- | :--- | :--- |
| **GET** | `/api/admin/bookings` | Danh sách đặt vé |
| **GET** | `/api/admin/bookings/{id}` | Chi tiết đặt vé |
| **PUT** | `/api/admin/bookings/{id}` | Cập nhật đặt vé |
| **PUT** | `/api/admin/bookings/{id}/status` | Cập nhật trạng thái đặt vé |
| **DELETE** | `/api/admin/bookings/{id}` | Xóa đặt vé |

### 9.7. Quản lý Coupon
| Method | Endpoint | Description |
| :--- | :--- | :--- |
| **GET** | `/api/admin/coupons` | Danh sách mã giảm giá |
| **POST** | `/api/admin/coupons` | Tạo coupon (name, promoCode, discountType, discountValue, maxUsage, startDate, endDate) |
| **PUT** | `/api/admin/coupons/{id}` | Cập nhật coupon |
| **DELETE** | `/api/admin/coupons/{id}` | Xóa coupon |

### 9.8. Quản lý Campaign
| Method | Endpoint | Description |
| :--- | :--- | :--- |
| **GET** | `/api/admin/campaigns` | Danh sách chiến dịch |
| **POST** | `/api/admin/campaigns` | Tạo campaign (title, bannerUrl?, content?, startDate, endDate, isFeatured?) |
| **PUT** | `/api/admin/campaigns/{id}` | Cập nhật campaign |
| **DELETE** | `/api/admin/campaigns/{id}` | Xóa campaign |

### 9.9. Dashboard & Settings
| Method | Endpoint | Description |
| :--- | :--- | :--- |
| **GET** | `/api/admin/dashboard` | Thống kê tổng quan hệ thống |
| **GET** | `/api/admin/settings` | Xem cấu hình hệ thống |
| **PUT** | `/api/admin/settings` | Cập nhật cấu hình |

### 9.10. Nhật ký
| Method | Endpoint | Description |
| :--- | :--- | :--- |
| **GET** | `/api/admin/logs?pageNumber=&pageSize=` | Xem log hệ thống (phân trang) |

### 9.11. Quản lý Dòng máy bay (Aircraft Models)
| Method | Endpoint | Description |
| :--- | :--- | :--- |
| **GET** | `/api/admin/aircraft-models` | Danh sách dòng máy bay |
| **GET** | `/api/admin/aircraft-models/{id}` | Chi tiết dòng máy bay và sơ đồ ghế mẫu |
| **POST** | `/api/admin/aircraft-models` | Tạo dòng máy bay (name, manufacturer, totalSeats, seatTemplates) |
| **PUT** | `/api/admin/aircraft-models/{id}` | Cập nhật dòng máy bay |
| **DELETE** | `/api/admin/aircraft-models/{id}` | Xóa dòng máy bay |

---

## 10. Partner APIs (Đối tác hãng bay - Role: PartnerOnly)

### 10.1. Quản lý Tuyến bay
| Method | Endpoint | Description |
| :--- | :--- | :--- |
| **GET** | `/api/partner/routes` | Danh sách tuyến bay (scoped by AirlineId) |
| **POST** | `/api/partner/routes` | Tạo tuyến bay (originAirportId, destinationAirportId, distanceKm?, estimatedDurationMinutes?) |
| **PUT** | `/api/partner/routes/{id}` | Cập nhật tuyến bay |
| **DELETE** | `/api/partner/routes/{id}` | Xóa tuyến bay |

### 10.2. Quản lý Đội bay
| Method | Endpoint | Description |
| :--- | :--- | :--- |
| **GET** | `/api/partner/airplanes` | Danh sách máy bay (scoped by AirlineId) |
| **POST** | `/api/partner/airplanes` | Tạo máy bay (aircraftModelId?, model, registrationNumber, totalCapacity) |
| **PUT** | `/api/partner/airplanes/{id}` | Cập nhật máy bay |
| **DELETE** | `/api/partner/airplanes/{id}` | Xóa máy bay |

### 10.3. Quản lý Chuyến bay
| Method | Endpoint | Description |
| :--- | :--- | :--- |
| **GET** | `/api/partner/flights` | Danh sách chuyến bay |
| **POST** | `/api/partner/flights` | Tạo chuyến bay |
| **PUT** | `/api/partner/flights/{id}` | Cập nhật chuyến bay |
| **DELETE** | `/api/partner/flights/{id}` | Xóa chuyến bay |

### 10.4. Quản lý Cấu hình Tàu bay
| Method | Endpoint | Description |
| :--- | :--- | :--- |
| **GET** | `/api/partner/aircraft` | Danh sách cấu hình |
| **POST** | `/api/partner/aircraft` | Tạo cấu hình |
| **PUT** | `/api/partner/aircraft/{id}` | Cập nhật cấu hình |
| **DELETE** | `/api/partner/aircraft/{id}` | Xóa cấu hình |

### 10.5. Quản lý Coupon
| Method | Endpoint | Description |
| :--- | :--- | :--- |
| **GET** | `/api/partner/coupons` | Danh sách coupon hãng bay (scoped) |
| **POST** | `/api/partner/coupons` | Tạo coupon (code, discountType, discountValue, minOrderValue?, maxDiscountAmount?, startDate, endDate, usageLimit?, isActive?, airlineId từ JWT) |
| **PUT** | `/api/partner/coupons/{id}` | Cập nhật coupon |
| **DELETE** | `/api/partner/coupons/{id}` | Xóa coupon |

### 10.6. Quản lý Campaign
| Method | Endpoint | Description |
| :--- | :--- | :--- |
| **GET** | `/api/partner/campaigns` | Danh sách campaign hãng bay (scoped) |
| **POST** | `/api/partner/campaigns` | Tạo campaign (title, bannerUrl?, content?, startDate, endDate, isFeatured?) |
| **PUT** | `/api/partner/campaigns/{id}` | Cập nhật campaign |
| **DELETE** | `/api/partner/campaigns/{id}` | Xóa campaign |

### 10.7. Quản lý Đặt vé
| Method | Endpoint | Description |
| :--- | :--- | :--- |
| **GET** | `/api/partner/bookings` | Danh sách đặt vé |
| **PUT** | `/api/partner/bookings/{id}` | Cập nhật trạng thái (status) |

### 10.8. Quản lý Nhân viên
| Method | Endpoint | Description |
| :--- | :--- | :--- |
| **GET** | `/api/partner/staff` | Danh sách nhân viên (scoped, role Staff) |
| **POST** | `/api/partner/staff` | Tạo nhân viên (email, fullName, phone?, isActive?, password? mặc định) |
| **PUT** | `/api/partner/staff/{id}` | Cập nhật nhân viên |
| **DELETE** | `/api/partner/staff/{id}` | Xóa nhân viên |
| **GET** | `/api/partner/staff/my-airline` | Tất cả thành viên cùng hãng (không lọc role) |

### 10.9. Cài đặt Hãng bay
| Method | Endpoint | Description |
| :--- | :--- | :--- |
| **GET** | `/api/partner/settings` | Xem thông tin hãng (airlineName, address, supportEmail, supportPhone) |
| **PUT** | `/api/partner/settings` | Cập nhật thông tin hãng |

### 10.10. Dashboard & Logs
| Method | Endpoint | Description |
| :--- | :--- | :--- |
| **GET** | `/api/partner/dashboard` | Thống kê doanh thu hãng bay |
| **GET** | `/api/partner/logs?pageNumber=&pageSize=` | Nhật ký hoạt động (phân trang) |

---

## 11. Module Status APIs (Trạng thái module)

| Method | Endpoint | Description |
| :--- | :--- | :--- |
| **GET** | `/api/interactions` | Trạng thái module Interactions |
| **GET** | `/api/notifications` | Trạng thái module Notifications |

---

## 12. Realtime Hubs (SignalR WebSocket)

| Hub | URL | Phương thức | Mô tả |
| :--- | :--- | :--- | :--- |
| **SeatHub** | `/hubs/seats` | `JoinFlight(flightId)`, `LeaveFlight(flightId)` | Theo dõi ghế realtime. Server → Client: `SeatUpdated(flightId, seatNumber, isAvailable)` |
| **SupportChatHub** | `/hubs/support` | `CustomerJoinChat(airlineId, customerName)`, `StaffRegister(airlineId, staffName)`, `SendMessageToAirline(message)`, `SendMessageToCustomer(connectionId, message)` | Live chat Customer ↔ Staff, auto-assign |
| **NotificationHub** | `/hubs/notifications` | Client → Server: (none — server push only) | Đẩy thông báo realtime đến người dùng. Server → Client: `ReceiveNotification(notification)` |
