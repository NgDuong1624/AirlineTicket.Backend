# Airline Ticket Backend System

Hệ thống đặt vé và theo dõi chuyến bay xây dựng trên nền tảng **.NET 10** theo kiến trúc **Modular Monolith** kết hợp **Clean Architecture**.

## Kiến trúc hệ thống (Architecture)
Hệ thống được chia thành 4 Module chính biệt lập, giúp dễ dàng mở rộng và tách thành Microservices trong tương lai:
1. **Flights**: Quản lý chuyến bay, sân bay, máy bay, sơ đồ ghế máy bay và theo dõi trạng thái chuyến bay.
2. **Bookings**: Quản lý đặt vé, thanh toán, hành khách và phát hành vé.
3. **Users**: Quản lý tài khoản người dùng, phân quyền hệ thống.
4. **Promotions**: Quản lý các chiến dịch giảm giá (Campaigns) và mã giảm giá (Coupons), định giá động (Dynamic Pricing).
5. **Interactions**: Cung cấp hệ thống Q&A thông minh, xử lý các thắc mắc của khách hàng về vé và thủ tục bay sử dụng AI Model Store.
6. **CMS**: Quản lý nội dung bài viết, tin tức và trang tĩnh.
7. **Notifications**: Xử lý gửi Email/SMS và thông báo đẩy.
8. **Logs**: Hệ thống ghi nhật ký tập trung và kiểm soát hoạt động.

Mỗi Module tuân thủ chặt chẽ nguyên lý Clean Architecture gồm 4 tầng:
* `Api`: Cung cấp các RESTful Endpoints (Controllers / Minimal APIs).
* `Application`: Chứa logic nghiệp vụ (Use Cases), CQRS Pattern (với MediatR) và xử lý Validate (với FluentValidation).
* `Domain`: Nơi định nghĩa các Core Entities, Enums và Domain Exceptions. Không phụ thuộc vào bất kỳ thư viện ngoài nào.
* `Infrastructure`: Nơi xử lý truy cập cơ sở dữ liệu với Entity Framework Core, cấu hình Fluent API, và kết nối External APIs...

## Cơ sở dữ liệu (Database)
Hệ thống sử dụng **SQL Server** làm cơ sở dữ liệu chính.
Được thiết kế dựa trên cùng một Database nhưng phân vùng bảng dữ liệu bằng **Schemas** để đảm bảo ranh giới độc lập (bounded contexts) cho từng module:
* Schema `flights`
* Schema `bookings`
* Schema `users`
* Schema `promotions`
* Schema `CMS`
* Schema `Notifications`
* Schema `Logs`

## Hướng dẫn cài đặt (Setup Instructions)

### 1. Chuẩn bị Môi trường
* Cài đặt **.NET 10 SDK**.
* Cài đặt **Entity Framework Core CLI**:
  ```bash
  dotnet tool install --global dotnet-ef
  ```
* Chạy SQL Server và đảm bảo cấu hình kết nối chuẩn xác.

### 2. Cấu hình Connection String
Mở file `src/Api/AirlineTicket.Api/appsettings.json` và cập nhật chuỗi kết nối Database phù hợp với môi trường của bạn (Ví dụ IP Server, SQL Authentication với User Id/Password).

### 3. Cấu hình Secret Keys
Ứng dụng yêu cầu hai loại cấu hình bảo mật chính thông qua User Secrets hoặc Biến môi trường:

#### A. MediatR License Key (Lucky Penny)
Bắt buộc cho môi trường Production để sử dụng MediatR.
- Key: `LuckyPenny:MediatR:LicenseKey`
- CLI: `dotnet user-secrets set "LuckyPenny:MediatR:LicenseKey" "your-key"`

#### B. AI Service API Key (Model Store)
Dùng để xác thực dịch vụ AI trong tính năng Q&A (`/api/v1/qa/ask`).
- Key: `AiService:ModelStore:ApiKey`
- CLI: `dotnet user-secrets set "AiService:ModelStore:ApiKey" "your-ai-api-key"`

### 4. Cập nhật Cơ sở dữ liệu (Migrations)
Để tiện lợi, dự án đã có sẵn script tự động chạy các lệnh EF Core. Bạn chỉ cần chạy script sau tại thư mục gốc của Backend:
```bash
./run_ef.sh
```
*(Script này sẽ tự động chạy lệnh tạo Migration và Update Database cho toàn bộ modules).*

### 5. Chạy dự án
Dự án có thể mở trên các IDE (Visual Studio, Rider) thông qua file `AirlineTicket.Backend.sln`.
Hoặc chạy trực tiếp bằng lệnh:
```bash
dotnet run --project src/Api/AirlineTicket.Api/AirlineTicket.Api.csproj
```
Dự án API trung tâm (API Gateway) sẽ tự động nạp tất cả các Controller từ các Modules con. Sau khi chạy, bạn có thể truy cập `/swagger` để xem tài liệu API.

## Hướng dẫn Triển khai (Deploy with Docker)

Dự án đã được trang bị sẵn `Dockerfile` và `docker-compose.yml` để dễ dàng triển khai.

### 1. Triển khai bằng Docker Compose (Khuyên dùng)
Hệ thống sử dụng các file cấu hình Docker Compose theo từng môi trường nằm trong thư mục `deploy/docker`.

**Chạy môi trường Development:**
```bash
docker-compose -f deploy/docker/docker-compose.dev.yml up -d --build
```
Hệ thống sẽ chạy tại cổng **5000** (bạn có thể truy cập http://localhost:5000/swagger).

**Chạy môi trường Production:**
```bash
docker-compose -f deploy/docker/docker-compose.prod.yml up -d --build
```
Hệ thống sẽ chạy tại cổng **80** (mặc định HTTP).

### 2. Triển khai Backend API bằng Docker thuần (Standalone)
Nếu bạn đã có sẵn SQL Server (ví dụ ở máy host hoặc cloud), bạn chỉ cần build image cho Backend API:
```bash
docker build -t airlineticket-backend .
```
Sau đó chạy Container (Nhớ truyền tham số kết nối tới DB thực tế của bạn):
```bash
docker run -d -p 5000:8080 \
  -e "ConnectionStrings__DefaultConnection=Server=YOUR_IP;Database=AirlineTicketDb;User Id=sa;Password=YOUR_PASS;TrustServerCertificate=True" \
  --name airlineticket-api \
  airlineticket-backend
```
