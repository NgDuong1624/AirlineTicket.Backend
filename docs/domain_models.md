# Domain Models & Enums

Tài liệu này mô tả các Enum và nghiệp vụ cốt lõi trong các Domain Models của hệ thống.

## 1. Flight Status (Trạng thái chuyến bay)
Được sử dụng trong thực thể `Flight`.
- `Scheduled` (0): Đã lên lịch, chưa cất cánh.
- `Delayed` (1): Bị hoãn so với lịch trình dự kiến.
- `Boarding` (2): Đang trong quá trình hành khách lên máy bay.
- `InAir` (3): Đang bay trên không (Real-time tracking).
- `Landed` (4): Đã hạ cánh an toàn.
- `Cancelled` (5): Bị hủy.

## 2. Seat Class (Hạng ghế)
Được sử dụng trong `AirplaneSeat` và `FlightSeat`.
- `Economy` (0): Hạng phổ thông. (Hệ số `PriceMultiplier` thường = 1.0)
- `Business` (1): Hạng thương gia. (Hệ số `PriceMultiplier` thường = 1.5 - 2.0)
- `First` (2): Hạng nhất. (Hệ số `PriceMultiplier` thường = 3.0 trở lên)

## 3. Booking Status (Trạng thái đặt chỗ)
- `Pending` (0): Mới tạo, đang chờ thanh toán.
- `Confirmed` (1): Đã thanh toán và xác nhận.
- `Cancelled` (2): Đã hủy (do quá hạn thanh toán hoặc người dùng tự hủy).
- `Refunded` (3): Đã hoàn tiền.

## 4. Payment Status (Trạng thái thanh toán)
- `Pending` (0): Đang chờ xử lý.
- `Completed` (1): Đã thanh toán thành công.
- `Failed` (2): Thanh toán thất bại.

## 5. Ticket Status (Trạng thái vé điện tử)
- `Issued` (0): Đã phát hành (sẵn sàng sử dụng).
- `Used` (1): Đã sử dụng (hành khách đã lên máy bay/check-in).
- `Cancelled` (2): Đã bị hủy.

## 6. User Roles (Phân quyền người dùng)
- `Customer` (0): Khách hàng thông thường đặt vé.
- `Admin` (1): Quản trị viên hệ thống (có quyền quản lý toàn bộ sân bay, chuyến bay).
- `Staff` (2): Nhân viên hãng hàng không.

## 7. Discount Type (Loại giảm giá)
Được sử dụng trong module `Promotions` (`Campaign` và `Coupon`).
- `Percentage` (0): Giảm theo phần trăm (Ví dụ: Giảm 10%).
- `FixedAmount` (1): Giảm số tiền cố định (Ví dụ: Giảm 500,000 VNĐ).
