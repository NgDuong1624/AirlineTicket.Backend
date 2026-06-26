# Kiến trúc hệ thống

## 1. Tổng quan kiến trúc
Hệ thống được thiết kế theo kiến trúc **Modular Monolith** kết hợp **Clean Architecture** nhằm đảm bảo khả năng mở rộng, dễ dàng bảo trì và tích hợp với nhiều hệ thống bên ngoài, tuân thủ nguyên tắc Domain-Driven Design (DDD) và CQRS.

Dự án phát triển trên nền tảng **.NET 10** (`net10.0`). Cổng vào duy nhất của hệ thống là `AirlineTicket.Api` (API Gateway / Host), chịu trách nhiệm bootstrap, xử lý middleware, routing, cấu hình xác thực và điều phối realtime.

## 2. Cấu trúc thư mục (Directory Structure)
```
AirlineTicket.Backend/
├── src/
│   ├── Api/
│   │   └── AirlineTicket.Api/          # API Gateway Host
│   │       ├── Program.cs              # Bootstrap, middleware, service registration
│   │       ├── ModuleRegistration.cs   # Danh sách Application assemblies của các modules
│   │       └── Realtime/               # SignalR Hubs (SeatHub, SupportChatHub)
│   ├── BuildingBlock/                  # Hạ tầng dùng chung
│   │   ├── AirlineTicket.BuildingBlocks.Api/          # Endpoints, Middleware, Auth
│   │   ├── AirlineTicket.BuildingBlocks.Application/  # CQRS, Behaviors, Caching, Logging
│   │   ├── AirlineTicket.BuildingBlocks.Domain/       # Entity, IAggregateRoot, IDomainEvent
│   │   └── AirlineTicket.BuildingBlocks.Infrastructure/ # Cache, Logging implementations
│   └── Modules/v1/                     # Các phân hệ nghiệp vụ độc lập
│       ├── Bookings/                   # Quản lý đặt vé, thanh toán, hành khách, vé
│       ├── CMS/                        # Dashboard admin/partner, cài đặt hệ thống
│       ├── Flights/                    # Quản lý hãng bay, sân bay, máy bay, chuyến bay, ghế
│       ├── Interactions/               # Q&A thông minh (AI), đánh giá (Reviews)
│       ├── Logs/                       # Nhật ký hệ thống và thao tác đối tác
│       ├── Notifications/              # Gửi thông báo Email/SMS/Push
│       ├── Promotions/                 # Quản lý coupon và campaign (Admin & Partner)
│       └── Users/                      # Quản lý tài khoản, phân quyền (RBAC, Scopes)
├── database/                           # Script SQL khởi tạo, seed dữ liệu và triggers
├── deploy/                             # Cấu hình Docker, Nginx triển khai hệ thống
├── docs/                               # Tài liệu thiết kế hệ thống
└── tests/                              # Các dự án kiểm thử (Unit / Integration Tests)
```

Mỗi module nghiệp vụ đều được tách biệt chặt chẽ thành 4 tầng dự án riêng:
1. **Domain**: Thực thể lõi (Entities), Enums, Rules và Domain Events. Không phụ thuộc thư viện ngoài.
2. **Application**: Logic nghiệp vụ (Use Cases), CQRS (MediatR Handlers), Validators (FluentValidation), và các DTOs.
3. **Infrastructure**: Kết nối DB (EF Core DbContext), Cấu hình Fluent API, Repositories, và tích hợp dịch vụ ngoài.
4. **Api**: Đăng ký các endpoints Minimal APIs (thực thi interface `IEndpoint`).

## 3. Các thành phần chính (Modules)

### 3.1. Phân hệ Tài khoản & Phân quyền (Users Module)
- Đăng nhập, đăng ký tài khoản khách hàng, lấy thông tin cá nhân.
- Quản lý Admin: CRUD tài khoản, danh sách quyền hạn.
- Phân quyền chi tiết (User Permission Scopes): Gán quyền kèm giới hạn theo hãng bay (`AirlineId`), sân bay (`AirportCode`) hoặc hạn mức (`MaxLimitValue`).
- Quản lý Partner: CRUD tài khoản staff của đối tác hãng bay.

### 3.2. Phân hệ Chuyến bay (Flights Module)
- Public: Xem danh sách sân bay/hãng bay/tuyến bay, tìm kiếm chuyến bay một chiều/khứ hồi, xem sơ đồ ghế.
- Admin: CRUD hãng bay, sân bay, chuyến bay.
- Partner: CRUD tuyến bay khai thác, đội bay (máy bay), thiết lập chuyến bay, cấu hình sơ đồ ghế, cài đặt thông tin hãng.
- Staff: Xem danh sách chuyến bay và sơ đồ ghế.

### 3.3. Phân hệ Đặt vé & Vé (Bookings Module)
- Customer: Tạo booking mới, xem chi tiết booking, xem lịch sử đặt vé, cập nhật/hủy booking, thanh toán (pay) và xem vé điện tử (ticket).
- Admin: Xem danh sách bookings, cập nhật thông tin booking, cập nhật trạng thái booking.
- Partner: Xem danh sách đặt vé, cập nhật trạng thái booking.
- Staff: Đặt vé hộ khách hàng qua điện thoại (staff booking), xem bảng bán vé (sales board).

### 3.4. Phân hệ Khuyến mãi (Promotions Module)
- Admin: CRUD coupon toàn hệ thống, CRUD campaign toàn hệ thống.
- Partner: CRUD coupon riêng của hãng bay, CRUD campaign riêng của hãng bay.
- Public: Xem danh sách khuyến mãi đang chạy, áp dụng mã coupon vào giỏ hàng (`/api/promotions/apply`).

### 3.5. Phân hệ Tương tác & AI (Interactions Module)
- AI Chatbot: Gửi câu hỏi cho AI Travel Assistant (`/api/v1/qa/ask`), tích hợp NVIDIA API chạy model DeepSeek V4 Flash để tư vấn tự động.
- Reviews (sắp triển khai): Đánh giá chuyến bay/hãng bay.
- Interactions Status: Endpoint kiểm tra trạng thái hoạt động phân hệ.

### 3.6. Phân hệ CMS Nội dung (CMS Module)
- Admin Dashboard: Thống kê doanh thu hệ thống, số lượng đặt vé, người dùng mới.
- Partner Dashboard: Thống kê doanh thu, số vé bán ra của hãng bay đối tác.
- Admin Settings: Xem và cập nhật cấu hình hệ thống (tỷ giá, phí dịch vụ).

### 3.7. Phân hệ Logs & Nhật ký (Logs Module)
- Admin: Xem log hệ thống toàn cục và log lỗi runtime (phân trang).
- Partner: Xem lịch sử thao tác của nhân viên hãng bay (phân trang).

### 3.8. Phân hệ Thông báo (Notifications Module)
- Status check: Đảm bảo module chạy ổn định.
- Kênh giao tiếp phụ trợ gửi Email/SMS xác nhận và vé điện tử.

## 4. BuildingBlocks (Hạ tầng dùng chung)
`BuildingBlocks` cung cấp các abstractions và implementations dùng chung cho toàn bộ modules:
- **CQRS**: Interface `ICommand`/`IQuery` và `ICommandHandler`/`IQueryHandler` kế thừa từ MediatR.
- **MediatR Pipeline Behaviors**:
  - `LoggingBehavior`: Tự động log request/response của MediatR Commands/Queries.
  - `CachingBehavior`: Tự động cache kết quả truy vấn dựa trên cấu hình thuộc tính của Query.
  - `ValidationBehavior`: Tự động quét và validate dữ liệu đầu vào sử dụng FluentValidation trước khi chuyển tiếp cho Handler.
- **Middlewares**:
  - `CorrelationMiddleware`: Tự động sinh/nhận `X-Correlation-ID` qua HTTP Headers phục vụ cho việc tracking log.
  - `RequestResponseLoggingMiddleware`: Log chi tiết thông tin request và response của HTTP calls.
- **Xác thực & Phân quyền (Auth)**:
  - JWT Bearer: Xác thực token người dùng, hỗ trợ đọc token từ query string đối với kết nối SignalR WebSockets.
  - `DynamicPermissionPolicyProvider` & `PermissionAuthorizationHandler`: Phân quyền động dựa trên Permission Code.
  - `AirlineResourceHandler`: Đảm bảo Partner/Staff chỉ được thao tác trên tài nguyên (Route, Airplane, Flight, Settings) thuộc về hãng hàng không của mình.
- **Caching**: `ICacheService` trừu tượng hóa dịch vụ cache, hỗ trợ `DistributedCacheService` kết nối tới Redis Cloud và tự động fallback sang `InMemoryCache` khi không có Redis.
- **Logging**: `ILoggingService` viết đè lên Serilog, cấu hình ghi nhật ký ra Console và rolling daily File.

## 5. Tính năng Realtime (SignalR)
- **SeatHub** (`/hubs/seats`): Quản lý kết nối staff/client theo dõi sơ đồ ghế theo phòng (`flight-{flightId}`). Khi một ghế thay đổi trạng thái, server phát sóng sự kiện `SeatUpdated` đến các clients trong phòng.
- **FlightSeatReservation**:
  - Thực thi compare-and-set trực tiếp tại DB (`ExecuteUpdateAsync` kiểm tra `IsAvailable == true`) để đảm bảo tính nguyên tử (atomic updates), ngăn chặn double-booking (overbooking) ở mức hiệu năng cao nhất mà không cần cơ chế row lock/table lock kéo dài.
  - Giải phóng ghế (ReleaseSeats) khi đơn hàng hết hạn hoặc bị hủy.
- **SupportChatHub** (`/hubs/support`):
  - Khách hàng kết nối qua Hub và chọn hãng hàng không muốn hỗ trợ.
  - Nhân viên hãng bay đăng ký trực tuyến (scoped theo `AirlineId`).
  - Hệ thống tự động assign khách hàng cho nhân viên đang rảnh hoặc có số lượng chat ít nhất. Khi nhân viên ngắt kết nối, khách hàng tự động được phân phối lại cho nhân viên khác.

## 6. Cơ sở hạ tầng (Infrastructure)
- **Database**: Hệ quản trị SQL Server, sử dụng ranh giới logical thông qua **Schemas** (`identity`, `flights`, `bookings`, `promotions`, `interactions`, `cms`, `logs`, `notifications`).
- **Cache**: Redis Cloud phục vụ caching tốc độ cao và đồng bộ SignalR scale-out trong tương lai.
- **API Documentation**: Scalar API Reference thay thế cho Swagger UI cổ điển, cho giao diện trực quan và khả năng test API trực tiếp tốt hơn.
- **AI Service**: Tích hợp NVIDIA AI Foundation Models, sử dụng model DeepSeek V4 Flash hỗ trợ tư vấn qua AI Q&A.
- **Containerization**: Triển khai dễ dàng với Docker Multi-Stage build, Docker Compose môi trường dev/local/prod, và Nginx đóng vai trò Reverse Proxy + Load Balancer.
