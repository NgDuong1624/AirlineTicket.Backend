# Danh sách API hệ thống đặt vé máy bay (Airline Ticket Booking System)

Tài liệu này tổng hợp danh sách các API ban đầu cho hệ thống, được tham chiếu từ dự án frontend `AirlineTicket.Web`.

---

## 1. Authentication APIs (Xác thực người dùng)

| Method | Endpoint | Description | Payloads / Query / Response |
| :--- | :--- | :--- | :--- |
| **POST** | `/api/auth/login` | Đăng nhập người dùng (User/Admin/Partner) | Body: `{ email, password }` |
| **POST** | `/api/auth/register` | Đăng ký tài khoản người dùng mới | Body: `{ name, email, password, phone }` |
| **GET** | `/api/auth/me` | Lấy thông tin tài khoản đang đăng nhập | Header: `Authorization: Bearer <token>` |

---

## 2. Flights APIs (Quản lý và tìm kiếm chuyến bay)

| Method | Endpoint | Description | Payloads / Query / Response |
| :--- | :--- | :--- | :--- |
| **POST** | `/api/flights` | Tìm kiếm chuyến bay theo bộ lọc (Nơi đi/đến, ngày đi, hạng vé, giá, hãng,...) | Body: `{ from, to, departDate, cabinClass, airlines, priceRange, stops, sortBy, currency }` |
| **GET** | `/api/flights/{id}` | Lấy chi tiết chuyến bay theo ID | Path Param: `id` |
| **GET** | `/api/flights/trending` | Lấy danh sách các chuyến bay/tuyến đường phổ biến (trending) | Response: Danh sách chuyến bay nổi bật |

---

## 3. Bookings APIs (Quản lý đặt vé)

| Method | Endpoint | Description | Payloads / Query / Response |
| :--- | :--- | :--- | :--- |
| **GET** | `/api/bookings` | Lấy toàn bộ danh sách đặt vé (dành cho nhân viên/hệ thống) | Query: Phân trang, trạng thái |
| **POST** | `/api/bookings` | Tạo một lượt đặt vé mới | Body: `{ flightId, passengers: [...], contactEmail, contactPhone, specialRequests }` |
| **GET** | `/api/bookings/my-bookings` | Lấy lịch sử đặt vé của người dùng hiện tại | Header: `Authorization: Bearer <token>` |
| **GET** | `/api/bookings/{id}` | Lấy thông tin chi tiết một mã đặt vé (mã PNR/ID) | Path Param: `id` |
| **PUT** | `/api/bookings/{id}` | Cập nhật thông tin đặt vé (đổi ghế, hành lý, thông tin khách hàng) | Body: `{ passengers, contactEmail, contactPhone }` |
| **DELETE**| `/api/bookings/{id}` | Yêu cầu hủy đặt vé | Path Param: `id` |

---

## 4. Airports APIs (Quản lý sân bay)

| Method | Endpoint | Description | Payloads / Query / Response |
| :--- | :--- | :--- | :--- |
| **GET** | `/api/airports` | Lấy danh sách sân bay hoặc tìm kiếm theo từ khóa | Query: `query` (tên thành phố, mã sân bay, tên sân bay) |

---

## 5. Promotions APIs (Quản lý khuyến mãi)

| Method | Endpoint | Description | Payloads / Query / Response |
| :--- | :--- | :--- | :--- |
| **GET** | `/api/promotions` | Lấy danh sách tất cả các chương trình ưu đãi hiện có | Response: Mảng danh sách mã giảm giá, khuyến mãi |
| **GET** | `/api/promotions/campaigns` | Lấy danh sách các chiến dịch ưu đãi lớn | Response: Danh sách banner, thông tin chiến dịch |

---

## 6. Admin APIs (Quản lý Admin)

Tất cả các API này yêu cầu xác thực Admin (Role: `Admin`).

| Method | Endpoint | Description | Payloads / Query / Response |
| :--- | :--- | :--- | :--- |
| **GET/POST/PUT/DELETE** | `/api/admin/users` | Quản lý người dùng trong hệ thống (Khách hàng, Đối tác, Admin) | - |
| **GET/POST/PUT/DELETE** | `/api/admin/airlines` | Quản lý thông tin các hãng hàng không | - |
| **GET/POST/PUT/DELETE** | `/api/admin/airports` | Quản lý danh sách các sân bay hoạt động | - |
| **GET/POST/PUT/DELETE** | `/api/admin/permissions` | Cấu hình phân quyền người dùng và vai trò (Roles & Permissions) | - |
| **GET/POST/PUT/DELETE** | `/api/admin/coupons` | Quản lý mã giảm giá trên toàn hệ thống | - |
| **GET/POST/PUT/DELETE** | `/api/admin/campaigns` | Quản lý chiến dịch quảng cáo, khuyến mãi chung | - |
| **GET/POST/PUT/DELETE** | `/api/admin/flights` | Giám sát và quản lý lịch trình các chuyến bay | - |
| **GET/POST/PUT/DELETE** | `/api/admin/bookings` | Quản lý và xử lý tất cả các giao dịch đặt vé của hệ thống | - |
| **GET/PUT** | `/api/admin/settings` | Quản lý các cấu hình hệ thống (tỷ giá, phí dịch vụ, chính sách) | - |
| **GET** | `/api/admin/dashboard` | Lấy thống kê số liệu tổng quan (Doanh thu, số lượt đặt vé, user mới) | - |
| **GET** | `/api/admin/logs` | Xem lịch sử hoạt động hệ thống (System & Audit Logs) | - |

---

## 7. Partner APIs (Quản lý dành cho đối tác/hãng bay)

Yêu cầu xác thực Đối tác (Role: `Partner` hoặc `AirlineStaff`).

| Method | Endpoint | Description | Payloads / Query / Response |
| :--- | :--- | :--- | :--- |
| **GET** | `/api/partner/dashboard` | Thống kê doanh thu, số vé bán ra của hãng bay đối tác | - |
| **GET/POST/PUT/DELETE** | `/api/partner/routes` | Quản lý đường bay đăng ký khai thác của hãng | - |
| **GET/POST/PUT/DELETE** | `/api/partner/airplanes` | Quản lý đội bay/danh sách máy bay | - |
| **GET/POST/PUT/DELETE** | `/api/partner/flights` | Thiết lập lịch trình chuyến bay của hãng đối tác | - |
| **GET/POST/PUT/DELETE** | `/api/partner/coupons` | Quản lý mã giảm giá riêng của hãng hàng không | - |
| **GET/POST/PUT/DELETE** | `/api/partner/campaigns` | Quản lý các chương trình ưu đãi nội bộ của hãng | - |
| **GET/POST/PUT/DELETE** | `/api/partner/bookings` | Quản lý danh sách đặt vé thuộc chuyến bay của hãng | - |
| **GET/POST/PUT/DELETE** | `/api/partner/aircraft` | Quản lý cấu hình sơ đồ ghế và loại tàu bay (Seatmap) | - |
| **GET/POST/PUT/DELETE** | `/api/partner/staff` | Quản lý nhân viên của đối tác | - |
| **GET/PUT** | `/api/partner/settings` | Cài đặt thông tin đối tác và chính sách hãng | - |
| **GET** | `/api/partner/logs` | Nhật ký hoạt động thao tác của nhân viên hãng | - |
