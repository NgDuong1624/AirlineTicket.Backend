# Luồng nghiệp vụ cốt lõi (Core Workflows)

Tài liệu này trình bày các luồng xử lý nghiệp vụ quan trọng trong hệ thống Airline Ticket Backend.

## 1. Luồng Tìm kiếm Chuyến bay (Flight Search Flow)
1. **Frontend**: Gửi yêu cầu tìm kiếm lên API với các tham số `OriginAirportCode`, `DestinationAirportCode`, `DepartureDate`.
2. **Backend (Flights Module)**:
   - Truy vấn cơ sở dữ liệu tìm các `Routes` phù hợp.
   - Tìm các `Flights` thuộc các Route đó, khởi hành trong khoảng thời gian chỉ định.
   - Lọc bỏ các chuyến bay đã bị hủy (`Cancelled`) hoặc đã cất cánh (`InAir`, `Landed`).
   - *Tính giá động (Dynamic Pricing)*: Giao tiếp với `Promotions Module` để kiểm tra xem có `Campaign` nào đang diễn ra áp dụng cho chuyến bay/hãng bay này không.
3. **Backend**: Trả về danh sách chuyến bay khả dụng kèm theo `BasePrice` và `DiscountedPrice` (nếu có khuyến mãi).
4. **Frontend**: Người dùng click chọn một chuyến bay.
5. **Backend**: API trả về sơ đồ ghế của chuyến bay đó (`FlightSeats`), biểu diễn ghế nào còn trống (`IsAvailable = true`) và tính toán giá vé cuối cùng cho từng ghế.

## 2. Luồng Đặt vé & Thanh toán (Booking & Payment Flow)
Đây là quy trình yêu cầu tính nhất quán dữ liệu (Data Consistency) cao nhất hệ thống.

1. **Frontend**: Gửi Request đặt chỗ bao gồm `FlightId`, danh sách thông tin `Passengers`, các `SeatIds` tương ứng, và `CouponCode` (nếu có).
2. **Backend (Bookings Module)**:
   - **Validate Ghế**: Gọi sang `Flights Module` để đảm bảo tất cả các `SeatId` này vẫn còn trống (`IsAvailable == true`). Nếu có ghế đã bị đặt, báo lỗi ngay lập tức.
   - **Validate Coupon**: Gọi sang `Promotions Module` để kiểm tra mã giảm giá có hợp lệ không (số lượt dùng, hạn dùng). Tính ra số tiền được giảm `DiscountAmount`.
   - **Lock Seats**: Cập nhật các `FlightSeats` này thành `IsAvailable = false` (Tạm giữ ghế để tránh xung đột).
   - **Create Booking**: Tạo bản ghi `Booking` với trạng thái `Pending` kèm theo `TotalPrice` đã trừ khuyến mãi. Sinh một mã PNR ngẫu nhiên (Ví dụ: `A3B89Z`).
   - **Create Passengers**: Lưu danh sách thông tin hành khách vào database.
3. **Frontend**: Chuyển hướng người dùng sang màn hình Nhập phương thức Thanh toán.
4. **User**: Xác nhận thanh toán (bằng thẻ/ví điện tử).
5. **Backend (Bookings Module)**:
   - **Nếu giao dịch thành công**:
     - Tạo bản ghi `Payment` với trạng thái `Completed`.
     - Cập nhật `Booking` thành `Confirmed`.
     - Cấp phát vé điện tử: Tạo các bản ghi `Tickets` với trạng thái `Issued`.
     - Trigger gửi Email xác nhận vé (Background Job).
   - **Nếu giao dịch thất bại hoặc quá hạn thời gian (Timeout)**:
     - Đổi trạng thái `Booking` thành `Cancelled`.
     - **Nhả ghế (Release Seats)**: Phục hồi lại trạng thái `FlightSeats.IsAvailable = true` để người khác có thể đặt.

## 3. Luồng Quản lý Trạng thái Chuyến bay (Flight Status Management)
1. **Admin/Staff**: Thực hiện thao tác cập nhật trạng thái chuyến bay (Ví dụ: Chuyển từ `Scheduled` sang `Delayed` hoặc `Cancelled` do thời tiết).
2. **Backend**: Lưu trạng thái mới vào database bảng `Flights`.
3. **Background Job (Optional)**: Khi trạng thái là Cancelled/Delayed, hệ thống phát ra một Domain Event.
4. **Notification System**: Hệ thống thông báo lắng nghe Event, quét danh sách các `Bookings` đã thanh toán thuộc về chuyến bay đó và gửi hàng loạt Email/SMS xin lỗi, thông báo thay đổi lịch trình tới người dùng.
