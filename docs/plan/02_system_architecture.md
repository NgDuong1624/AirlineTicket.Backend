# Kiến trúc hệ thống

## 1. Tổng quan kiến trúc
Hệ thống được thiết kế theo kiến trúc Modular Monolith nhằm đảm bảo khả năng mở rộng, dễ dàng bảo trì và tích hợp với nhiều hệ thống bên ngoài, tuân thủ nguyên tắc Domain-Driven Design (DDD) và CQRS.

## 2. Các thành phần chính
- **API Gateway:** Điểm vào duy nhất cho mọi request từ Web Client và Đối tác, xử lý xác thực và điều hướng.
- **Module Flights (Quản lý chuyến bay):** Xử lý logic tìm kiếm, lọc và hiển thị thông tin chuyến bay từ các hãng. Cung cấp API cho các hãng bay đồng bộ dữ liệu lịch trình.
- **Module Bookings (Quản lý đặt vé):** Xử lý quy trình đặt vé, thanh toán, quản lý vé và xuất vé.
- **Module Promotions (Khuyến mãi):** Quản lý các chương trình ưu đãi, giảm giá cho khách hàng.
- **Module Users (Quản lý người dùng):** Quản lý tài khoản, xác thực, phân quyền.
- **Module Reviews (Đánh giá):** Quản lý phản hồi, xếp hạng hãng hàng không từ hành khách.
- **Module Articles/CMS (Nội dung):** Quản lý và phân phối các bài báo, tin tức về hàng không và du lịch.
- **Module Notifications (Thông báo):** Xử lý gửi email xác nhận đặt vé thành công, vé điện tử và thông báo thay đổi trạng thái chuyến bay (Real-time qua SignalR/Email).
- **Module Logs (Nhật ký hệ thống):** Thu thập và lưu trữ thông tin log tập trung (`logs.SystemLogs`), phục vụ phân tích lỗi và giám sát hành vi người dùng/quản trị viên.

## 3. Cơ sở hạ tầng
- Cơ sở dữ liệu: Hệ quản trị CSDL quan hệ SQL Server được phân chia logical theo từng schema/module để đảm bảo tính độc lập dữ liệu.
- Entity Framework Core cho truy cập dữ liệu.
- Docker để đóng gói ứng dụng, dễ dàng triển khai đa môi trường (dev, prod).