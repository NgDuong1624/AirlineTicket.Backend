# Lộ trình triển khai (Project Phases Roadmap - Backend Focus)

Lộ trình chi tiết dưới vai trò Backend Project Manager nhằm đảm bảo tiến độ triển khai cho dự án Modular Monolith sử dụng ASP.NET Core (.NET 10), SQL Server, và Clean Architecture / CQRS.

---

## Giai đoạn 1: Khởi tạo, Thiết kế & Hạ tầng Core (Tuần 1 - Tuần 4)
- **Thiết kế & Kiến trúc:**
  - Thống nhất cơ sở dữ liệu `init_v1.sql` (hệ quản trị SQL Server) gồm các Schema: `identity`, `flights`, `bookings`, `promotions`, `interactions`, `cms`, `logs`, `notifications`.
  - Thiết kế cấu trúc thư mục Modular Monolith Backend trong `AirlineTicket.Backend`: `src/BuildingBlocks` và `src/Modules/v1/`.
  - Đặc tả API đặc trưng cho từng module bằng OpenAPI/Scalar và quy trình xác thực JWT, Dynamic RBAC kết hợp User Permission Scopes.
- **Hạ tầng Core (BuildingBlocks):**
  - Thiết lập Solution và dự án: `AirlineTicket.BuildingBlocks` và `AirlineTicket.Api` (API Gateway / Host).
  - Triển khai Core BuildingBlocks: CQRS (MediatR), validation (FluentValidation), Logging (Serilog/SystemLogs), Global Exception Handling, và Outbox Pattern cho giao tiếp giữa các module.
  - Thiết lập MediatR Pipeline Behaviors: LoggingBehavior → CachingBehavior → ValidationBehavior.
  - Triển khai CorrelationMiddleware và RequestResponseLoggingMiddleware.
  - Cấu hình Docker Compose cho môi trường phát triển (SQL Server, Redis).
  - Thiết lập Scalar API Reference thay cho Swagger UI truyền thống.
  - Thiết lập CI/CD pipeline cơ bản (GitHub Actions compile & run tests).

## Giai đoạn 2: Phát triển Core MVP Modules (Tuần 5 - Tuần 12)
- **Module Users (Tuần 5 - Tuần 6):**
  - Phát triển tính năng Authentication (Đăng ký, Đăng nhập JWT, Refresh Token).
  - Tích hợp Phân quyền động: Roles, Permissions và User Permission Scopes (quản trị theo từng trạm bay AirportCode hoặc hãng bay AirlineId).
  - Dynamic Permission Policy Provider và AirlineResourceHandler.
- **Module Flights & B2B Integration (Tuần 7 - Tuần 9):**
  - Quản lý hạ tầng bay: Hãng bay (Airlines), Sân bay (Airports), Máy bay (Airplanes), Tuyến bay (Routes).
  - Nghiệp vụ Chuyến bay (Flights) và Sơ đồ ghế (FlightSeats).
  - Tìm kiếm chuyến bay khứ hồi (Round-trip).
  - Xây dựng hệ thống B2B API chuẩn hóa để đối tác tích hợp (đồng bộ lịch trình, cập nhật giá vé và sơ đồ ghế).
- **Module Bookings & Payments (Tuần 10 - Tuần 12):**
  - Luồng giao dịch chính: Tạo đơn đặt chỗ (Bookings), sinh mã PNR code, thông tin hành khách (Passengers).
  - Giữ ghế tạm thời (Seat Locking) với atomic compare-and-set trên SQL Server để tránh overbooking.
  - Tích hợp cổng thanh toán bên thứ ba (Stripe/Paypal/VNPay), xử lý Webhook để cập nhật trạng thái thanh toán (Payments) và phát hành Vé điện tử (Tickets/E-ticket).

## Giai đoạn 2.5: Staff Portal & Realtime (Tuần 10 - Tuần 12)
- **Staff APIs:**
  - Staff flight list + seat map (`/api/staff/flights` và `/api/staff/flights/{id}/seats`).
  - Staff booking — đặt vé hộ khách hàng qua điện thoại (`/api/staff/bookings`).
  - Staff sales board — cross-module join Bookings + Flights (`/api/staff/sales`).
- **SignalR Realtime:**
  - SeatHub — realtime seat availability broadcast (`/hubs/seats`).
  - FlightSeatReservation — atomic compare-and-set chống double-booking, realtime broadcast khi ghế đặt/hủy.
  - SupportChatHub — live chat customer ↔ staff, auto-assign staff theo AirlineId (`/hubs/support`).

## Giai đoạn 3: Mở rộng Tính năng & Tối ưu hóa (Tuần 13 - Tuần 18)
- **Module Promotions & Campaign (Tuần 13 - Tuần 14):**
  - Quản lý và áp dụng mã giảm giá (Coupons - dạng phần trăm hoặc số tiền cố định) kèm ràng buộc giá trị đơn hàng tối thiểu và số lượt sử dụng tối đa.
  - Áp dụng mã giảm giá (`POST /api/promotions/apply`).
  - Quản lý bài đăng chiến dịch tiếp thị (Campaigns).
  - Partner-scoped coupons và campaigns (airline-scoped CRUD).
- **Module Interactions & Reviews (Tuần 15):**
  - Đánh giá chất lượng hãng bay/chuyến bay (Reviews) từ 1-5 sao.
  - Xác thực người dùng đã hoàn thành bay thực tế (`IsVerifiedPurchase`).
  - AI Travel Assistant (Q&A): Tích hợp NVIDIA API với DeepSeek V4 Flash model (`/api/v1/qa/ask`).
- **Module CMS & Dashboard (Tuần 16):**
  - Quản lý nội dung: bài viết, tin tức, dashboard.
  - Admin Dashboard (`/api/admin/dashboard`).
  - Partner Dashboard (`/api/partner/dashboard`).
  - Admin Settings (`/api/admin/settings`).
- **Module Notifications & Logs (Tuần 16 - Tuần 17):**
  - Thiết lập schema `notifications` (NotificationTemplates + Notifications).
  - Admin & Partner logs phân trang (`/api/admin/logs`, `/api/partner/logs`).
- **Tối ưu hóa hiệu năng & Giao tiếp liên Module (Tuần 17 - Tuần 18):**
  - Áp dụng Caching (Redis) cho các API tìm kiếm chuyến bay tần suất cao (MediatR CachingBehavior).
  - Tối ưu hóa truy vấn SQL Server, tạo Indexes trên các cột tìm kiếm (`DepartureTime`, `RouteId`, `PnrCode`, `TicketNumber`).

## Giai đoạn 4: Kiểm thử, Tối ưu & Bàn giao (Tuần 19 - Tuần 22)
- **Kiểm thử chất lượng (Testing):**
  - Đạt tỷ lệ bao phủ Unit Testing tối thiểu 80% cho core logic.
  - Viết Integration Testing cho các luồng nghiệp vụ liên Module (luồng Đặt vé → Thanh toán → Phát hành vé).
- **Kiểm thử hiệu năng (Load/Performance Testing):**
  - Sử dụng k6/JMeter để kiểm thử tải API tìm kiếm chuyến bay (mục tiêu 1000+ RPS).
  - Tối ưu luồng lock ghế và thanh toán đồng thời.
- **Giám sát & Triển khai Production:**
  - Thiết lập hạ tầng giám sát: Log tập trung (Elasticsearch/Kibana hoặc Seq), Prometheus & Grafana.
  - Triển khai Production sử dụng Docker Swarm/Kubernetes.
