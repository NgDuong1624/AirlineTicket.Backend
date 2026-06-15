# Tính năng và Phân hệ

## 1. Phân hệ Khách hàng (B2C)
- **Tìm kiếm và So sánh:** Tìm kiếm chuyến bay linh hoạt theo điểm đi/đến, thời gian. Lọc và so sánh giá, thời gian bay, hãng hàng không.
- **Quản lý Đặt chỗ:** Đặt vé trực tuyến, chọn ghế (`flights.FlightSeats`), mua thêm hành lý. Quản lý vé điện tử (`bookings.Tickets`), yêu cầu hoàn/hủy vé.
- **Hệ thống Đánh giá:** Đọc và viết đánh giá chi tiết về trải nghiệm bay, xếp hạng hãng hàng không, giúp người dùng khác có thêm cơ sở lựa chọn (`interactions.Reviews`).
- **Tin tức và Bài báo:** Chuyên trang cung cấp thông tin du lịch, review điểm đến, tin tức hàng không và các mẹo khi đi máy bay (`cms.Articles`).

## 2. Phân hệ Đối tác Hãng hàng không (B2B API)
- **Đồng bộ Lịch trình:** API chuẩn hóa để hãng bay đẩy dữ liệu chuyến bay, máy bay, và thay đổi lịch trình.
- **Cập nhật Giá vé & Ghế:** API cho phép cập nhật giá vé linh hoạt (`PriceOverride`) và tình trạng ghế trống theo thời gian thực.
- **Quản lý Đặt chỗ:** Đồng bộ thông tin hành khách và xác nhận đặt chỗ giữa hệ thống trung tâm và hệ thống nội bộ của hãng.

## 3. Phân hệ Quản trị (Admin)
- **Phân quyền và Phạm vi:** Phân quyền theo vai trò (Roles), chức năng (Permissions) và phạm vi tác nghiệp (User Permission Scopes) của nhân viên từng trạm bay hoặc hãng bay.
- **Quản lý Hãng bay:** Cấp quyền truy cập API, theo dõi hiệu suất tích hợp của các hãng hàng không.
- **Quản lý Nội dung (CMS):** Tạo, chỉnh sửa, duyệt và xuất bản các bài báo liên quan đến hàng không.
- **Quản lý Khách hàng & Đơn hàng:** Hỗ trợ giải quyết sự cố đặt vé, tra cứu giao dịch.
- **Báo cáo & Phân tích:** Thống kê doanh thu, lưu lượng tìm kiếm, đánh giá chất lượng các hãng hàng không.
- **Nhật ký Hệ thống:** Giám sát lịch sử thao tác của quản trị viên và đối tác thông qua `logs.SystemLogs`.