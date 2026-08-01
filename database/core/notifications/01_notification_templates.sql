-- =====================================================
-- CORE: Notification Templates (i18n)
-- Schema: notifications
-- Locales: vi, en, zh, ja, ko, fr
-- =====================================================

-- Booking Confirmed templates
INSERT INTO notifications.notification_templates (id, code, subject, body_template, language, created_at) VALUES
('F1F3E2D4-BCDE-4F01-2345-6789ABCDEF01', 'BOOKING_CONFIRMED', 'Xác nhận đặt vé thành công', 'Chào {{PassengerName}}, Đặt chỗ của bạn (Mã: {{PnrCode}}) đã được xác nhận thành công. Chuyến bay: {{FlightNumber}}.', 'vi', NOW()),
('F2F3E2D4-BCDE-4F01-2345-6789ABCDEF02', 'BOOKING_CONFIRMED_EN', 'Booking Confirmation', 'Hello {{PassengerName}}, Your booking (PNR: {{PnrCode}}) has been confirmed. Flight: {{FlightNumber}}.', 'en', NOW())
ON CONFLICT (id) DO NOTHING;

-- Flight Created templates (multi-language)
INSERT INTO notifications.notification_templates (id, code, subject, body_template, language, created_at) VALUES
(gen_random_uuid(), 'FLIGHT_CREATED', 'New Flight Created: {{FlightNumber}}', 'A new flight {{FlightNumber}} from {{Origin}} to {{Destination}} has been created by a partner.', 'en', NOW()),
(gen_random_uuid(), 'FLIGHT_CREATED', 'Chuyến bay mới được tạo: {{FlightNumber}}', 'Chuyến bay mới {{FlightNumber}} từ {{Origin}} đến {{Destination}} vừa được tạo bởi đối tác.', 'vi', NOW()),
(gen_random_uuid(), 'FLIGHT_CREATED', '新航班已创建: {{FlightNumber}}', '合作伙伴已创建从 {{Origin}} 到 {{Destination}} 的新航班 {{FlightNumber}}。', 'zh', NOW()),
(gen_random_uuid(), 'FLIGHT_CREATED', '新しいフライトが作成されました: {{FlightNumber}}', 'パートナーによって {{Origin}} から {{Destination}} への新しいフライト {{FlightNumber}} が作成されました。', 'ja', NOW()),
(gen_random_uuid(), 'FLIGHT_CREATED', '새 항공편 생성됨: {{FlightNumber}}', '파트너가 {{Origin}}에서 {{Destination}}으로 가는 새 항공편 {{FlightNumber}}을((를) 생성했습니다.', 'ko', NOW()),
(gen_random_uuid(), 'FLIGHT_CREATED', 'Nouveau vol créé : {{FlightNumber}}', 'Un nouveau vol {{FlightNumber}} de {{Origin}} à {{Destination}} a été créé par un partenaire.', 'fr', NOW())
ON CONFLICT (code, language) DO NOTHING;
