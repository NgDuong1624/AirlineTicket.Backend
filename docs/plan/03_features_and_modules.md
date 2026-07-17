# Tính năng và Phân hệ

## 1. Phân hệ Khách hàng (B2C - Public APIs)

### 1.1. Tìm kiếm Chuyến bay
- `POST /api/flights` — Tìm kiếm chuyến bay một chiều (bộ lọc: nơi đi/đến, ngày, hạng vé, hãng bay, khoảng giá, số điểm dừng, sắp xếp, tiền tệ)
- `POST /api/flights/round-trip` — Tìm kiếm chuyến bay khứ hồi (thêm ngày về)
- `GET /api/flights/{id}` — Chi tiết chuyến bay
- `GET /api/flights/trending` — Các tuyến bay phổ biến
- `GET /api/flights/{id}/seats` — Sơ đồ ghế chuyến bay

### 1.2. Tra cứu Hạ tầng
- `GET /api/airports?search=` — Danh sách/tìm kiếm sân bay
- `GET /api/airlines` — Danh sách hãng hàng không
- `GET /api/routes` — Danh sách tuyến bay

### 1.3. Đặt vé và Thanh toán
- `POST /api/bookings` — Tạo đơn đặt chỗ mới (AllowAnonymous; passengers list gồm FirstName, LastName, IdentityCard, SeatNumber)
- `GET /api/bookings/{id}` — Xem chi tiết đơn đặt chỗ
- `GET /api/bookings/my-bookings` — Lịch sử đặt vé của người dùng đang đăng nhập (RequireAuth)
- `PUT /api/bookings/{id}` — Cập nhật thông tin đặt vé (passengers, contactEmail, contactPhone)
- `DELETE /api/bookings/{id}` — Hủy đơn đặt chỗ
- `POST /api/bookings/{id}/pay` — Thanh toán đơn hàng (paymentMethod, amount; trả về TransactionId)
- `GET /api/tickets/{id}` — Xem thông tin vé điện tử

### 1.4. Khuyến mãi
- `GET /api/promotions` — Danh sách khuyến mãi đang hoạt động
- `GET /api/promotions/campaigns` — Danh sách chiến dịch (stub — trả về mảng rỗng)
- `POST /api/promotions/apply` — Áp dụng mã giảm giá (PromoCode + FlightId + OriginalAmount → DiscountedAmount)

### 1.5. AI Travel Assistant (Q&A)
- `POST /api/v1/qa/ask` — Gửi câu hỏi cho AI (sử dụng NVIDIA API + DeepSeek model, trả lời các thắc mắc về vé bay và thủ tục)

### 1.6. Xác thực
- `POST /api/auth/login` — Đăng nhập (trả về JWT token)
- `POST /api/auth/register` — Đăng ký tài khoản (email, password, fullName, phone)
- `GET /api/auth/me` — Thông tin tài khoản đang đăng nhập (RequireAuth)

## 2. Phân hệ Đối tác Hãng hàng không (Partner APIs - Role: PartnerOnly)

### 2.1. Quản lý Tuyến bay
- `GET /api/partner/routes` — Danh sách tuyến bay (scoped by AirlineId từ JWT)
- `POST /api/partner/routes` — Tạo tuyến bay mới
- `PUT /api/partner/routes/{id}` — Cập nhật tuyến bay
- `DELETE /api/partner/routes/{id}` — Xóa tuyến bay

### 2.2. Quản lý Đội bay
- `GET /api/partner/airplanes` — Danh sách máy bay (scoped by AirlineId)
- `POST /api/partner/airplanes` — Thêm máy bay mới
- `PUT /api/partner/airplanes/{id}` — Cập nhật máy bay
- `DELETE /api/partner/airplanes/{id}` — Xóa máy bay

### 2.3. Quản lý Chuyến bay
- `GET /api/partner/flights` — Danh sách chuyến bay (stub — pending full scheduling)
- `POST /api/partner/flights` — Tạo chuyến bay (stub)
- `PUT /api/partner/flights/{id}` — Cập nhật chuyến bay (stub)
- `DELETE /api/partner/flights/{id}` — Xóa chuyến bay (stub)

### 2.4. Quản lý Dòng máy bay (Aircraft Models)
- `GET /api/partner/aircraft` — Danh sách cấu hình tàu bay
- `POST /api/partner/aircraft` — Tạo cấu hình tàu bay
- `PUT /api/partner/aircraft/{id}` — Cập nhật cấu hình tàu bay
- `DELETE /api/partner/aircraft/{id}` — Xóa cấu hình tàu bay

### 2.5. Quản lý Coupon riêng hãng
- `GET /api/partner/coupons` — Danh sách mã giảm giá (scoped by AirlineId)
- `POST /api/partner/coupons` — Tạo mã giảm giá (code, discountType, discountValue, minOrderValue, maxDiscountAmount, startDate, endDate, usageLimit)
- `PUT /api/partner/coupons/{id}` — Cập nhật mã giảm giá
- `DELETE /api/partner/coupons/{id}` — Xóa mã giảm giá

### 2.6. Quản lý Campaign riêng hãng
- `GET /api/partner/campaigns` — Danh sách chiến dịch ưu đãi (scoped by AirlineId)
- `POST /api/partner/campaigns` — Tạo chiến dịch (title, bannerUrl, content, startDate, endDate, isFeatured)
- `PUT /api/partner/campaigns/{id}` — Cập nhật chiến dịch
- `DELETE /api/partner/campaigns/{id}` — Xóa chiến dịch

### 2.7. Quản lý Đặt vé
- `GET /api/partner/bookings` — Danh sách đặt vé
- `PUT /api/partner/bookings/{id}` — Cập nhật trạng thái đặt vé (status)

### 2.8. Quản lý Nhân viên
- `GET /api/partner/staff` — Danh sách nhân viên hãng bay (scoped by AirlineId)
- `POST /api/partner/staff` — Tạo nhân viên mới (email, fullName, phone, password mặc định ChangeMe123!)
- `PUT /api/partner/staff/{id}` — Cập nhật thông tin nhân viên
- `DELETE /api/partner/staff/{id}` — Xóa nhân viên
- `GET /api/partner/staff/my-airline` — Xem tất cả nhân viên cùng hãng (không lọc role)

### 2.9. Cài đặt Hãng bay
- `GET /api/partner/settings` — Xem thông tin hãng (airlineName, address, supportEmail, supportPhone)
- `PUT /api/partner/settings` — Cập nhật thông tin hãng

### 2.10. Dashboard & Logs
- `GET /api/partner/dashboard` — Thống kê doanh thu, vé bán ra
- `GET /api/partner/logs` — Nhật ký hoạt động (phân trang: pageNumber, pageSize)

## 3. Phân hệ Nhân viên hãng bay (Staff APIs - Role: PartnerOrStaff)

### 3.1. Quản lý Chuyến bay
- `GET /api/staff/flights?search=` — Danh sách chuyến bay (có tìm kiếm theo từ khóa)
- `GET /api/staff/flights/{id}/seats` — Sơ đồ ghế chuyến bay

### 3.2. Bán vé
- `POST /api/staff/bookings` — Đặt vé hộ khách hàng (contactName, contactEmail, contactPhone + passengers; có kiểm tra seat conflict)
- `GET /api/staff/sales` — Bảng bán vé (ticket-sales board)

## 4. Phân hệ Quản trị (Admin APIs - Role: AdminOnly)

### 4.1. Quản lý Người dùng
- `GET /api/admin/users` — Danh sách người dùng
- `POST /api/admin/users` — Tạo người dùng (email, fullName, phone, roleId, isActive, password mặc định ChangeMe123!)
- `GET /api/admin/users/{id}` — Chi tiết người dùng
- `PUT /api/admin/users/{id}` — Cập nhật người dùng
- `DELETE /api/admin/users/{id}` — Xóa người dùng

### 4.2. Quản lý Quyền hạn
- `GET /api/admin/permissions` — Danh sách quyền hệ thống
- `GET /api/admin/user-permissions/{userId}` — Danh sách quyền chi tiết của người dùng
- `POST /api/admin/user-permissions/{userId}` — Gán quyền cho người dùng (permissionId + airlineId/airportCode scope)
- `DELETE /api/admin/user-permissions/{userId}/{permissionId}` — Xóa quyền của người dùng

### 4.3. Quản lý Sân bay
- `GET /api/admin/airports` — Danh sách sân bay
- `POST /api/admin/airports` — Tạo sân bay (iataCode, nameEn, nameVi, cityEn, cityVi, countryCode, timezone, isActive)
- `PUT /api/admin/airports/{id}` — Cập nhật sân bay
- `DELETE /api/admin/airports/{id}` — Xóa sân bay

### 4.4. Quản lý Hãng hàng không
- `GET /api/admin/airlines` — Danh sách hãng bay
- `POST /api/admin/airlines` — Tạo hãng bay (iataCode, name, logoUrl, baseCountry, isActive)
- `PUT /api/admin/airlines/{id}` — Cập nhật hãng bay
- `DELETE /api/admin/airlines/{id}` — Xóa hãng bay

### 4.5. Quản lý Chuyến bay
- `GET /api/admin/flights` — Danh sách chuyến bay
- `POST /api/admin/flights` — Tạo chuyến bay (routeId, airplaneId, flightNumber, basePrice, scheduledDeparture, scheduledArrival)
- `PUT /api/admin/flights/{id}` — Cập nhật chuyến bay
- `DELETE /api/admin/flights/{id}` — Xóa chuyến bay

### 4.6. Quản lý Đặt vé
- `GET /api/admin/bookings` — Danh sách đặt vé
- `GET /api/admin/bookings/{id}` — Chi tiết đặt vé
- `PUT /api/admin/bookings/{id}` — Cập nhật đặt vé
- `PUT /api/admin/bookings/{id}/status` — Cập nhật trạng thái đặt vé riêng
- `DELETE /api/admin/bookings/{id}` — Xóa đặt vé

### 4.7. Quản lý Coupon
- `GET /api/admin/coupons` — Danh sách mã giảm giá toàn hệ thống
- `POST /api/admin/coupons` — Tạo mã giảm giá (name, promoCode, discountType, discountValue, maxUsage, startDate, endDate)
- `PUT /api/admin/coupons/{id}` — Cập nhật mã giảm giá
- `DELETE /api/admin/coupons/{id}` — Xóa mã giảm giá

### 4.8. Quản lý Campaign
- `GET /api/admin/campaigns` — Danh sách chiến dịch quảng cáo
- `POST /api/admin/campaigns` — Tạo chiến dịch (title, bannerUrl, content, startDate, endDate, isFeatured)
- `PUT /api/admin/campaigns/{id}` — Cập nhật chiến dịch
- `DELETE /api/admin/campaigns/{id}` — Xóa chiến dịch

### 4.9. Dashboard & Settings
- `GET /api/admin/dashboard` — Thống kê tổng quan hệ thống
- `GET /api/admin/settings` — Cấu hình hệ thống
- `PUT /api/admin/settings` — Cập nhật cấu hình hệ thống

### 4.10. Nhật ký Hệ thống
- `GET /api/admin/logs` — Lịch sử hoạt động (phân trang: pageNumber, pageSize)

## 5. Tính năng Realtime (SignalR Hubs)

### 5.1. SeatHub (`/hubs/seats`)
- **Client → Server:**
  - `JoinFlight(flightId)` — Tham gia nhóm theo dõi ghế chuyến bay
  - `LeaveFlight(flightId)` — Rời nhóm
- **Server → Client:** `SeatUpdated(flightId, seatNumber, isAvailable)` — Phát sóng mỗi khi trạng thái ghế thay đổi
- **Ghi chú:** Xác thực JWT qua query string `access_token`; cơ chế chống double-booking sử dụng atomic compare-and-set trên cột IsAvailable, rollback nếu conflict

### 5.2. SupportChatHub (`/hubs/support`)
- **Client → Server:**
  - `CustomerJoinChat(airlineId, customerName)` — Khách hàng bắt đầu phiên chat
  - `StaffRegister(airlineId, staffName)` — Nhân viên đăng ký trực tuyến
  - `SendMessageToAirline(message)` — Khách gửi tin nhắn đến nhân viên đã được gán
  - `SendMessageToCustomer(customerConnectionId, message)` — Nhân viên trả lời khách hàng
- **Server → Client:**
  - `SystemMessage(text)` — Thông báo hệ thống (chào mừng, tìm kiếm nhân viên, lỗi…)
  - `ReceiveMessage(connectionId, role, name, message)` — Tin nhắn từ khách/nhân viên
  - `AgentAssigned(staffName)` — Thông báo đã gán nhân viên cho khách
  - `NewCustomerChat(customerConnectionId, customerName)` — Thông báo cho nhân viên có khách mới
  - `CustomerDisconnected(connectionId, customerName)` — Thông báo khách ngắt kết nối
- **Ghi chú:** Lưu trữ session trong bộ nhớ (ConcurrentDictionary); tự động gán staff cùng hãng bay khi có khách, re-assign khi staff disconnect

## 6. Module Phụ trợ (Status Endpoints)
- `GET /api/v1/interactions` — Trạng thái module Interactions
- `GET /api/v1/notifications` — Trạng thái module Notifications

## 7. Ghi chú Kiến trúc
- Tất cả endpoint được triển khai dưới dạng Minimal API (IEndpoint) và tự động đăng ký qua `AddEndpoints()`.
- Module CMS (`AirlineTicket.Modules.CMS.*`) tồn tại nhưng chỉ phục vụ Dashboard và Settings — không có endpoint Articles hay Reviews.
- Module Interactions ngoài AI Q&A còn có endpoint status; module Reviews chưa được triển khai.
- Authorization sử dụng custom policies: `AdminOnly`, `PartnerOnly`, `PartnerOrStaff`, và policy mặc định `RequireAuthorization()`.
- JWT claim `AirlineId` được dùng để scoping dữ liệu cho Partner APIs.
