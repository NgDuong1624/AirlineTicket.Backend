-- =====================================================
-- CORE: Notification Templates (i18n)
-- Schema: notifications
-- Locales: vi, en, zh, ja, ko, fr
-- =====================================================

-- Booking Confirmed templates (multi-language)
INSERT INTO notifications.notification_templates (id, code, subject, body_template, language, created_at) VALUES
(gen_random_uuid(), 'BOOKING_CONFIRMED', 'Booking Confirmation', 'Hello {{PassengerName}}, Your booking (PNR: {{PnrCode}}) has been confirmed. Flight: {{FlightNumber}}.', 'en', NOW()),
(gen_random_uuid(), 'BOOKING_CONFIRMED', 'Xác nhận đặt vé thành công', 'Chào {{PassengerName}}, Đặt chỗ của bạn (Mã: {{PnrCode}}) đã được xác nhận thành công. Chuyến bay: {{FlightNumber}}.', 'vi', NOW()),
(gen_random_uuid(), 'BOOKING_CONFIRMED', '预订确认', '您好 {{PassengerName}}，您的预订（PNR: {{PnrCode}}）已确认。航班：{{FlightNumber}}。', 'zh', NOW()),
(gen_random_uuid(), 'BOOKING_CONFIRMED', '予約確認', '{{PassengerName}} 様、ご予約（PNR: {{PnrCode}}）が確定しました。フライト：{{FlightNumber}}。', 'ja', NOW()),
(gen_random_uuid(), 'BOOKING_CONFIRMED', '예약 확인', '{{PassengerName}} 님, 예약(PNR: {{PnrCode}})이 확인되었습니다. 항공편: {{FlightNumber}}.', 'ko', NOW()),
(gen_random_uuid(), 'BOOKING_CONFIRMED', 'Confirmation de réservation', 'Bonjour {{PassengerName}}, votre réservation (PNR: {{PnrCode}}) a été confirmée. Vol : {{FlightNumber}}.', 'fr', NOW())
ON CONFLICT (code, language) DO NOTHING;

-- Flight Created templates (multi-language)
INSERT INTO notifications.notification_templates (id, code, subject, body_template, language, created_at) VALUES
(gen_random_uuid(), 'FLIGHT_CREATED', 'New Flight Created: {{FlightNumber}}', 'A new flight {{FlightNumber}} from {{Origin}} to {{Destination}} departs at {{DepartureTime}} with base price {{BasePrice}}.', 'en', NOW()),
(gen_random_uuid(), 'FLIGHT_CREATED', 'Chuyến bay mới được tạo: {{FlightNumber}}', 'Chuyến bay mới {{FlightNumber}} từ {{Origin}} đến {{Destination}} khởi hành lúc {{DepartureTime}} với giá cơ bản {{BasePrice}}.', 'vi', NOW()),
(gen_random_uuid(), 'FLIGHT_CREATED', '新航班已创建: {{FlightNumber}}', '新航班 {{FlightNumber}} 从 {{Origin}} 到 {{Destination}} 将于 {{DepartureTime}} 起飞，基础价格为 {{BasePrice}}。', 'zh', NOW()),
(gen_random_uuid(), 'FLIGHT_CREATED', '新しいフライトが作成されました: {{FlightNumber}}', '新しいフライト {{FlightNumber}} ({{Origin}} 発 {{Destination}} 行) は {{DepartureTime}} に出発します。基本料金は {{BasePrice}} です。', 'ja', NOW()),
(gen_random_uuid(), 'FLIGHT_CREATED', '새 항공편 생성됨: {{FlightNumber}}', '새 항공편 {{FlightNumber}}({{Origin}} 출발 {{Destination}} 도착)이 {{DepartureTime}}에 출발합니다. 기본 요금은 {{BasePrice}}입니다.', 'ko', NOW()),
(gen_random_uuid(), 'FLIGHT_CREATED', 'Nouveau vol créé : {{FlightNumber}}', 'Un nouveau vol {{FlightNumber}} de {{Origin}} à {{Destination}} partira le {{DepartureTime}} avec un prix de base de {{BasePrice}}.', 'fr', NOW())
ON CONFLICT (code, language) DO NOTHING;

-- Update existing FLIGHT_CREATED templates to match the new body template
UPDATE notifications.notification_templates
SET body_template = CASE language
    WHEN 'en' THEN 'A new flight {{FlightNumber}} from {{Origin}} to {{Destination}} departs at {{DepartureTime}} with base price {{BasePrice}}.'
    WHEN 'vi' THEN 'Chuyến bay mới {{FlightNumber}} từ {{Origin}} đến {{Destination}} khởi hành lúc {{DepartureTime}} với giá cơ bản {{BasePrice}}.'
    WHEN 'zh' THEN '新航班 {{FlightNumber}} 从 {{Origin}} 到 {{Destination}} 将于 {{DepartureTime}} 起飞，基础价格为 {{BasePrice}}。'
    WHEN 'ja' THEN '新しいフライト {{FlightNumber}} ({{Origin}} 発 {{Destination}} 行) は {{DepartureTime}} に出発します。基本料金は {{BasePrice}} です。'
    WHEN 'ko' THEN '새 항공편 {{FlightNumber}}({{Origin}} 출발 {{Destination}} 도착)이 {{DepartureTime}}에 출발합니다. 기본 요금은 {{BasePrice}}입니다.'
    WHEN 'fr' THEN 'Un nouveau vol {{FlightNumber}} de {{Origin}} à {{Destination}} partira le {{DepartureTime}} avec un prix de base de {{BasePrice}}.'
END
WHERE code = 'FLIGHT_CREATED';
