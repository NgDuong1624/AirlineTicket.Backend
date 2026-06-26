# Tổng quan dự án

## 1. Giới thiệu
Hệ thống bán vé máy bay toàn cầu, đóng vai trò là nền tảng trung gian kết nối các hãng hàng không và khách hàng. Hệ thống cung cấp không gian lưu trữ và API chuẩn hóa cho các hãng bay trên toàn thế giới, đồng thời mang đến cho người dùng một nền tảng toàn diện để tìm kiếm, so sánh và đặt vé.

## 2. Mục tiêu dự án
- **Đối với hãng hàng không:** Cung cấp hạ tầng API mạnh mẽ, ổn định để đồng bộ dữ liệu chuyến bay, ghế ngồi, chương trình khuyến mãi và giá vé theo thời gian thực.
- **Đối với khách hàng:** Tạo ra một nền tảng thân thiện, cung cấp đa dạng lựa chọn chuyến bay, minh bạch về giá cả, chất lượng dịch vụ thông qua hệ thống đánh giá khách quan.
- **Đối với nhân viên hãng bay (Staff):** Cung cấp Staff Portal cho phép đặt vé hộ khách hàng, theo dõi sơ đồ ghế realtime, và quản lý bán vé.
- **Giá trị gia tăng:** Xây dựng hệ thống quản lý nội dung (CMS) cung cấp các bài báo, tin tức và cẩm nang du lịch liên quan đến hàng không để tăng mức độ tương tác của người dùng. Tích hợp AI Travel Assistant trả lời tự động các thắc mắc của khách hàng.

## 3. Phạm vi dự án
- Nền tảng Backend cung cấp RESTful API hiện đại (Minimal APIs).
- Cổng thông tin dành cho Khách hàng (Web Frontend).
- Cổng tích hợp API dành cho Đối tác (Hãng hàng không).
- Staff Portal cho nhân viên hãng bay (đặt vé hộ, bán vé, sơ đồ ghế realtime).
- Hệ thống bài viết và tin tức hàng không (Blog/CMS).
- Hệ thống live chat hỗ trợ khách hàng (Customer ↔ Staff).
- AI Travel Assistant (Q&A) cho các thắc mắc về vé bay và thủ tục.

## 4. Công nghệ sử dụng (Tech Stack)

| Thành phần | Công nghệ |
| :--- | :--- |
| Nền tảng | .NET 10, ASP.NET Core Minimal APIs |
| Cơ sở dữ liệu | SQL Server (schemas-based modular approach) |
| ORM | Entity Framework Core (Code-First + Fluent API) |
| CQRS & Messaging | MediatR (với pipeline behaviors: Logging → Caching → Validation) |
| Validation | FluentValidation |
| Logging | Serilog (Console + File rolling daily) |
| Cache | Redis Cloud (fallback: In-Memory Distributed Cache) |
| Realtime | SignalR (SeatHub + SupportChatHub) |
| AI Service | NVIDIA API (DeepSeek V4 Flash model) |
| Container | Docker + Docker Compose + Nginx |
| API Docs | Scalar API Reference (OpenAPI) |
| Xác thực | JWT Bearer + Dynamic RBAC + Permission Scopes |

## 5. Vai trò người dùng (User Roles)

| Vai trò | Mô tả | Authorization Policy |
| :--- | :--- | :--- |
| **Customer** | Tìm kiếm, đặt vé, thanh toán, xem lịch sử, đánh giá hãng bay, sử dụng AI Q&A | Đăng ký tự do |
| **Partner** | Quản lý tuyến bay, máy bay, chuyến bay, coupon, campaign, nhân viên, cài đặt hãng | `PartnerOnly` |
| **Staff** | Đặt vé hộ khách hàng qua điện thoại, xem sơ đồ ghế realtime, bán vé, live chat hỗ trợ | `AdminOrStaff` |
| **Admin** | Quản trị toàn bộ hệ thống: người dùng, sân bay, hãng bay, quyền hạn, dashboard, cấu hình | `AdminOnly` |

## 6. Tính năng nổi bật
1. **Tìm kiếm chuyến bay** một chiều và khứ hồi với bộ lọc đa chiều (hạng vé, khoảng giá, hãng bay, số điểm dừng).
2. **Đặt vé và thanh toán** trực tuyến với tích hợp cổng thanh toán.
3. **Hệ thống ghế realtime** với SignalR — chống double-booking bằng cơ chế atomic compare-and-set trên SQL Server.
4. **AI Travel Assistant** (Q&A) sử dụng NVIDIA API + DeepSeek model, trả lời tự động các câu hỏi về vé bay và thủ tục.
5. **Live Chat Support** (Customer ↔ Staff) qua SignalR, auto-assign nhân viên theo hãng bay, re-assign khi staff disconnect.
6. **Hệ thống coupon/campaign** cho cả Admin (toàn hệ thống) và Partner (scoped theo hãng bay).
7. **Staff Portal**: đặt vé hộ khách hàng qua điện thoại, bảng bán vé (sales board) với cross-module join.
8. **Soft Delete** toàn hệ thống — sử dụng cột `IsDeleted` kết hợp SQL Server INSTEAD OF DELETE triggers.
9. **Phân quyền động** — Dynamic Permission Policy Provider với User Permission Scopes (giới hạn theo hãng bay, trạm bay, số lượng).
10. **Distributed Caching** với Redis Cloud, MediatR CachingBehavior tự động cache queries.
