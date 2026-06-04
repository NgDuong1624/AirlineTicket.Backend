# Danh sách API Endpoints Dự Kiến

Dự án Airline Ticket Backend sẽ cung cấp các nhóm API tương ứng với từng Module. Tất cả API sẽ được prefix với `/api/v1/`.

## 1. Module: Flights (Chuyến bay)

### Sân bay & Tuyến bay
* `GET /api/v1/airports`: Lấy danh sách sân bay (hỗ trợ tìm kiếm, phân trang).
* `GET /api/v1/airports/{id}`: Lấy chi tiết sân bay.
* `GET /api/v1/routes`: Lấy danh sách tuyến bay hiện có.

### Chuyến bay (Flights)
* `GET /api/v1/flights/search`: Tìm kiếm chuyến bay theo điểm đi (Origin), điểm đến (Destination) và ngày bay (Date).
* `GET /api/v1/flights/{id}`: Xem chi tiết một chuyến bay (thông tin máy bay, giờ khởi hành, trạng thái).
* `GET /api/v1/flights/{id}/seats`: Lấy danh sách và sơ đồ ghế (`FlightSeats`) của chuyến bay (kèm theo giá và tình trạng ghế).
* `POST /api/v1/flights`: Tạo chuyến bay mới (Yêu cầu quyền Admin/Staff).

## 2. Module: Bookings (Đặt vé)

### Đặt chỗ (Bookings)
* `POST /api/v1/bookings`: Tạo đơn đặt chỗ mới. Gửi lên danh sách hành khách và các ghế mong muốn.
* `GET /api/v1/bookings/{id}`: Xem chi tiết đơn đặt chỗ (Mã PNR, Tổng tiền, Trạng thái).
* `GET /api/v1/bookings/my-bookings`: Xem lịch sử đặt chỗ của User đang đăng nhập.
* `DELETE /api/v1/bookings/{id}`: Hủy đơn đặt chỗ (áp dụng khi đơn chưa được thanh toán).

### Thanh toán & Phát hành vé (Payments & Tickets)
* `POST /api/v1/bookings/{id}/pay`: Thực hiện thanh toán cho đơn đặt chỗ (giả lập hoặc qua cổng thanh toán).
* `GET /api/v1/tickets/{id}`: Lấy thông tin vé điện tử của hành khách (dùng để render màn hình vé hoặc xuất PDF).

## 3. Module: Users (Người dùng)

### Xác thực (Authentication)
* `POST /api/v1/users/register`: Đăng ký tài khoản khách hàng mới.
* `POST /api/v1/users/login`: Đăng nhập, hệ thống sẽ trả về chuỗi JWT Token.

### Quản lý tài khoản
* `GET /api/v1/users/me`: Lấy thông tin User hiện tại (dựa vào JWT Token).
* `PUT /api/v1/users/me`: Cập nhật thông tin cá nhân.

## 4. Module: Promotions (Khuyến mãi)

### Chiến dịch (Campaigns)
* `GET /api/v1/promotions/campaigns`: Lấy danh sách các sự kiện khuyến mãi đang diễn ra.
* `POST /api/v1/promotions/campaigns`: Tạo chiến dịch khuyến mãi mới (Quyền Admin).

### Mã giảm giá (Coupons)
* `POST /api/v1/promotions/coupons/apply`: Kiểm tra và áp dụng một mã Coupon cho một Booking cụ thể (trả về số tiền được giảm).
* `POST /api/v1/promotions/coupons`: Tạo mã giảm giá mới (Quyền Admin).
