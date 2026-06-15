# Lộ trình triển khai (Project Phases Roadmap - Backend Focus)

Lộ trình chi tiết dưới vai trò Backend Project Manager nhằm đảm bảo tiến độ triển khai cho dự án Modular Monolith sử dụng ASP.NET Core 9, SQL Server, và Clean Architecture / CQRS.

---

## Giai đoạn 1: Khởi tạo, Thiết kế & Hạ tầng Core (Tuần 1 - Tuần 4)
- **Thiết kế & Kiến trúc:**
  - Thống nhất cơ sở dữ liệu `init_v1.sql` (hệ quản trị SQL Server) gồm các Schema: `identity`, `flights`, `bookings`, `promotions`, `interactions`, `cms`, `logs`.
  - Thiết kế cấu trúc thư mục Modular Monolith Backend trong `AirlineTicket.Backend`: `src/BuildingBlocks` và `src/Modules/v1/`.
  - Đặc tả API đặc trưng cho từng module bằng OpenAPI/Swagger và quy trình xác thực JWT, RBAC (Role-Based Access Control) kết hợp User Permission Scopes.
- **Hạ tầng Core (BuildingBlocks):**
  - Thiết lập Solution và dự án: `AirlineTicket.BuildingBlocks` và `AirlineTicket.Api` (API Gateway / Host).
  - Triển khai Core BuildingBlocks: CQRS (MediatR), validation (FluentValidation), Logging (Serilog/SystemLogs), Global Exception Handling, và Outbox Pattern cho giao tiếp giữa các module.
  - Cấu hình Docker Compose cho môi trường phát triển (SQL Server, Redis).
  - Thiết lập CI/CD pipeline cơ bản (GitHub Actions compile & run tests).

## Giai đoạn 2: Phát triển Core MVP Modules (Tuần 5 - Tuần 12)
- **Module Identity (Tuần 5 - Tuần 6):**
  - Phát triển tính năng Authentication (Đăng ký, Đăng nhập JWT, Refresh Token).
  - Tích hợp Phân quyền động: Roles (`INT`), Permissions và User Permission Scopes (quản trị theo từng trạm bay AirportCode hoặc hãng bay AirlineId).
- **Module Flights & B2B Integration (Tuần 7 - Tuần 9):**
  - Quản lý hạ tầng bay: Hãng bay (Airlines), Sân bay (Airports), Máy bay (Airplanes), Tuyến bay (Routes).
  - Nghiệp vụ Chuyến bay (Flights) và Sơ đồ ghế (FlightSeats).
  - Xây dựng hệ thống B2B API chuẩn hóa để đối tác tích hợp (đồng bộ lịch trình, cập nhật giá vé và sơ đồ ghế).
- **Module Bookings & Payments (Tuần 10 - Tuần 12):**
  - Luồng giao dịch chính: Tạo đơn đặt chỗ (Bookings), sinh mã PNR code, thông tin hành khách (Passengers).
  - Giữ ghế tạm thời (Seat Locking) với Redis TTL để tránh overbooking.
  - Tích hợp cổng thanh toán bên thứ ba (Stripe/Paypal/VNPay), xử lý Webhook để cập nhật trạng thái thanh toán (Payments) và phát hành Vé điện tử (Tickets/E-ticket).

## Giai đoạn 3: Mở rộng Tính năng & Tối ưu hóa (Tuần 13 - Tuần 18)
- **Module Promotions & Campaign (Tuần 13 - Tuần 14):**
  - Quản lý và áp dụng mã giảm giá (Coupons - dạng phần trăm hoặc số tiền cố định) kèm ràng buộc giá trị đơn hàng tối thiểu và số lượt sử dụng tối đa.
  - Quản lý bài đăng chiến dịch tiếp thị (Campaigns).
- **Module Interactions & Reviews (Tuần 15):**
  - Đánh giá chất lượng hãng bay/chuyến bay (Reviews) từ 1-5 sao.
  - Xác thực người dùng đã hoàn thành bay thực tế (`IsVerifiedPurchase`).
- **Module CMS & Articles (Tuần 16):**
  - Quản lý danh mục (Categories) và bài đăng du lịch, cẩm nang bay (Articles).
- **Tối ưu hóa hiệu năng & Giao tiếp liên Module (Tuần 17 - Tuần 18):**
  - Áp dụng Caching (Redis) cho các API tìm kiếm chuyến bay tần suất cao.
  - Tối ưu hóa truy vấn SQL Server, tạo Indexes đầy đủ trên các cột tìm kiếm (`DepartureTime`, `RouteId`, `PnrCode`, `TicketNumber`).

## Giai đoạn 4: Kiểm thử, Tối ưu & Bàn giao (Tuần 19 - Tuần 22)
- **Kiểm thử chất lượng (Testing):**
  - Đạt tỷ lệ bao phủ Unit Testing tối thiểu 80% cho core logic.
  - Viết Integration Testing cho các luồng nghiệp vụ liên Module (luồng Đặt vé -> Thanh toán -> Phát hành vé).
- **Kiểm thử hiệu năng (Load/Performance Testing):**
  - Sử dụng k6/JMeter để kiểm thử tải API tìm kiếm chuyến bay (mục tiêu 1000+ RPS).
  - Tối ưu luồng lock ghế và thanh toán đồng thời.
- **Giám sát & Triển khai Production:**
  - Thiết lập hạ tầng giám sát: Log tập trung (Elasticsearch/Kibana hoặc Seq), Prometheus & Grafana.
  - Triển khai Production sử dụng Docker Swarm/Kubernetes.