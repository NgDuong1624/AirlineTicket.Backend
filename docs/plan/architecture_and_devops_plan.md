# AIRLINE TICKET SYSTEM - ARCHITECTURE & DEVOPS PLAN

Tài liệu thiết kế kiến trúc hệ thống và hướng dẫn vận hành DevOps dành cho dự án AirlineTicket (Modular Monolith .NET 10).

---

## PHẦN 1: KIẾN TRÚC HỆ THỐNG (WORKER & REALTIME)

### 1. Cấu trúc thư mục mở rộng (Thêm lớp Worker và Realtime Host)

Để hệ thống hoạt động tối ưu, giảm tải dung lượng kết nối và hỗ trợ scale độc lập trên Docker/K8s, cấu trúc mã nguồn được phân rã thành 3 Host độc lập nằm trong `src/`:

```text
AirlineTicket.Backend/
├── src/
│   ├── Api/AirlineTicket.Api/               # REST API Host (Chỉ chạy HTTP REST)
│   │   ├── Program.cs
│   │   └── ModuleRegistration.cs
│   │
│   ├── Worker/AirlineTicket.Worker/         # Background Service Host (.NET Worker SDK)
│   │   ├── Program.cs                       # Bootstrap, cấu hình cấu trúc nền IHost
│   │   ├── ModuleWorkerRegistration.cs      # Quét và đăng ký Job của từng module
│   │   └── appsettings.json
│   │
│   ├── Realtime/AirlineTicket.SignalR/     # Hub Realtime Host (Quản lý Socket kết nối)
│   │   ├── Program.cs                       # Khởi tạo WebApplication, map Hubs & CORS
│   │   └── Hubs/                            # Nơi tập trung ChatHub, NotificationHub
│   │
│   ├── BuildingBlock/                       # Cơ sở hạ tầng dùng chung (4 projects)
│   └── Modules/v1/                          # 8 Business modules (Mỗi module có 4 lớp)
│       ├── Bookings/
│       ├── CMS/
│       ├── Flights/
│       ├── Interactions/
│       ├── Logs/
│       ├── Notifications/
│       ├── Promotions/
│       └── Users/
├── database/                                # Kịch bản SQL dữ liệu
├── deploy/                                  # Cấu hình Docker & Nginx/Caddy
├── docs/                                    # Tài liệu hệ thống
└── tests/                                   # Hệ thống kiểm thử
```

### 2. Danh sách Background Services (Worker Jobs) cần thiết theo Module
- Module Bookings (Đặt chỗ & Thanh toán)
  + CancelExpiredBookingsJob: Quét bảng đặt chỗ định kỳ mỗi 1 - 5 phút. Tự động hủy các đơn đặt giữ chỗ (Pending) quá hạn 15-30 phút chưa thanh toán, thực hiện hoàn trả số lượng ghế trống về cho module Flights.

  + BookingRefundProcessor: Tiếp nhận lệnh hoàn tiền bất đồng bộ từ hàng đợi (Message Queue). Thực hiện gọi lại (Retry) sang các cổng thanh toán bên thứ ba (VNPay, Momo, Stripe...) nếu xảy ra lỗi mạng cho đến khi thành công.

- Module Notifications (Thông báo)
  + EmailSmsMassSender: Worker tiêu thụ sự kiện (Event Consumer) xử lý bất đồng bộ. "Hứng" các sự kiện gửi Mail xác nhận, OTP SMS, tránh xử lý trực tiếp trên luồng API HTTP; sử dụng dịch vụ thứ ba (SendGrid, Twilio...).

  + FlightDelayNotifierJob: Tự động quét danh sách hành khách thuộc chuyến bay bị thay đổi lịch trình (Delay) để tạo và gửi lệnh thông báo đồng loạt.

- Module Flights (Quản lý chuyến bay)
  + FlightStatusAutomatorJob: Chạy định kỳ mỗi 1 - 5 phút để cập nhật tự động toàn bộ dòng vòng đời bay thực tế:
    Trạng thái 0: Scheduled $\rightarrow$ 2: Boarding: Trước giờ khởi hành (DepartureTime) 40 phút.
    Trạng thái 2: Boarding $\rightarrow$ 3: InAir: Tại đúng thời điểm cất cánh (DepartureTime).
    Trạng thái 3: InAir $\rightarrow$ 4: Landed: Khi hết thời gian bay hoặc máy bay hạ cánh thực tế.

  + FlightDelayDetectorJob: Tự động phát hiện lỗi cất cánh muộn dựa trên dữ liệu hàng không quốc tế. Nếu quá giờ DepartureTime máy bay chưa cất cánh, đổi trạng thái sang 1: Delayed và kích hoạt bắn event qua Message Queue.

  + CloseFlightSalesJob: Tự động khóa sơ đồ ghế (IsLockSeats = 1) và đóng cổng bán vé trực tuyến trước giờ bay 45 - 60 phút.

  + CheckInReminderJob: Gửi thông báo nhắc nhở hành khách thực hiện thủ tục check-in online trước giờ khởi hành 24 giờ.

  + DynamicPricingJob: Chạy định kỳ (ví dụ: 1 giờ/lần) để điều chỉnh BasePrice tự động dựa trên quy luật cung cầu và tỷ lệ lấp đầy ghế (Load Factor).

- Module Promotions (Khuyến mãi)
  + PromotionStatusUpdaterJob: Quét bảng chiến dịch khuyến mãi lúc 00:00 hàng ngày. Chuyển đổi trạng thái tự động: Pending $\rightarrow$ Active (đến ngày bắt đầu) và Active $\rightarrow$ Expired (hết hạn dùng).

- Module Users & Interactions (Tài khoản & Chatbot)
  + CleanExpiredTokensJob: Quét dọn hệ thống ban đêm để xóa bỏ hoàn toàn các Refresh Token đã hết hạn (Expired) hoặc bị thu hồi (Revoked) để tối ưu database.

  + ChatbotTimeoutHandler: Quét kiểm tra phiên chat. Nếu khách hàng không tương tác quá 15-30 phút, tự động đóng phiên chat (Close Session) và lưu trữ lịch sử hội thoại.

- Module Logs (Hệ thống Logs & Báo cáo)
  + LogRetentionCleanerJob: Chạy hàng tuần/tháng nhằm xóa hoặc nén dữ liệu Audit Logs, System Logs cũ quá 30 hoặc 90 ngày.

  + DailySalesReportJob: Chạy tự động vào lúc 23:55 hàng ngày để tính tổng vé bán ra, tổng doanh thu và gửi báo cáo về Telegram/Slack của Ban Quản Trị.

### 3. Tách cấu trúc Realtime (SignalR) & Quy tắc vận hành
- Cấu hình CORS bắt buộc (tại AirlineTicket.SignalR)
  + Vì API Gateway Host và SignalR Hub Host nằm trên 2 domain/port khác biệt, tệp Program.cs của SignalR Host bắt buộc phải mở cấu hình CORS hỗ trợ thông tin định danh:

```c
app.UseCors(builder => builder
    .WithOrigins("http://localhost:3000") // Domain Frontend
    .AllowAnyMethod()
    .AllowAnyHeader()
    .AllowCredentials()); // Bắt buộc để truyền tải Token/Cookie qua Socket
```

- Sử dụng Redis Backplane khi mở rộng (Scale Out)
  + Khi chạy nhiều bản sao (Pod) của dịch vụ Realtime, bắt buộc thêm cấu hình Redis Backplane để đồng bộ thông điệp giữa các Pod:

```c
builder.Services.AddSignalR().AddStackExchangeRedis("CẤU_HÌNH_TRÚC_REDIS");
```

