-- =====================================================
-- AIRLINE TICKET SYSTEM - SEED DATA (PostgreSQL)
-- =====================================================

-- =====================================================
-- 1. SEED IDENTITY DATA (Roles, Permissions, Users)
-- =====================================================

INSERT INTO users."Roles" (Id, Name, Description) VALUES
(0, 'role.system_admin.name', 'role.system_admin.desc'),
(1, 'role.airline_admin.name', 'role.airline_admin.desc'),
(2, 'role.airline_staff.name', 'role.airline_staff.desc'),
(3, 'role.user_client.name', 'role.user_client.desc');

INSERT INTO users."Permissions" (Code, Name, Description) VALUES
('CREATE_FLIGHT', 'permission.create_flight.name', 'permission.create_flight.desc'),
('SELL_TICKET', 'permission.sell_ticket.name', 'permission.sell_ticket.desc'),
('MANAGE_AIRLINE_STAFF', 'permission.manage_airline_staff.name', 'permission.manage_airline_staff.desc'),
('MANAGE_USERS', 'permission.manage_users.name', 'permission.manage_users.desc'),
('MANAGE_FLIGHTS', 'permission.manage_flights.name', 'permission.manage_flights.desc'),
('MANAGE_AIRLINES', 'permission.manage_airlines.name', 'permission.manage_airlines.desc'),
('MANAGE_AIRPORTS', 'permission.manage_airports.name', 'permission.manage_airports.desc'),
('MANAGE_BOOKINGS', 'permission.manage_bookings.name', 'permission.manage_bookings.desc'),
('MANAGE_ARTICLES', 'permission.manage_articles.name', 'permission.manage_articles.desc');

INSERT INTO users."Users" (Id, Email, EmailConfirmed, PasswordHash, FullName, Phone, IsActive, IsDeleted, CreatedAt, UpdatedAt, Role) VALUES
('D4B0F2A8-9B2F-4A9B-89E3-4E80D77BC901', 'admin@airlineticket.com', 1, '$2b$12$d7mwUGX/x5jcRrxdNAFqJekces0IQ2f9F4mC24iVGZUpwM2pF1Qly', 'Hệ Thống Admin', '0123456789', 1, 0, NOW(), NOW(), 0),
('E2E3F4C5-1A2B-3C4D-5E6F-7A8B9C0D1E2F', 'client@gmail.com', 1, '$2b$12$d7mwUGX/x5jcRrxdNAFqJekces0IQ2f9F4mC24iVGZUpwM2pF1Qly', 'Nguyễn Văn A', '0987654321', 1, 0, NOW(), NOW(), 2),
('A1F3E2D4-BCDE-4F01-2345-6789ABCDEF91', 'admin.vna@airlineticket.com', 1, '$2b$12$d7mwUGX/x5jcRrxdNAFqJekces0IQ2f9F4mC24iVGZUpwM2pF1Qly', 'Admin VNA', '0111111111', 1, 0, NOW(), NOW(), 1),
('B2F3E2D4-BCDE-4F01-2345-6789ABCDEF92', 'admin.vj@airlineticket.com', 1, '$2b$12$d7mwUGX/x5jcRrxdNAFqJekces0IQ2f9F4mC24iVGZUpwM2pF1Qly', 'Admin VJ', '0222222222', 1, 0, NOW(), NOW(), 1),
('C3F3E2D4-BCDE-4F01-2345-6789ABCDEF93', 'admin.qh@airlineticket.com', 1, '$2b$12$d7mwUGX/x5jcRrxdNAFqJekces0IQ2f9F4mC24iVGZUpwM2pF1Qly', 'Admin QH', '0333333333', 1, 0, NOW(), NOW(), 1);

INSERT INTO users."UserPermissionScopes" (Id, UserId, PermissionId, AirlineId, ScopeDescription, CreatedAt) VALUES
(gen_random_uuid(), 'C3F3E2D4-BCDE-4F01-2345-6789ABCDEF93', 3, 'C3F3E2D4-BCDE-4F01-2345-6789ABCDEF03', 'Bamboo Airways Administration', NOW()),
(gen_random_uuid(), 'C3F3E2D4-BCDE-4F01-2345-6789ABCDEF93', 5, 'C3F3E2D4-BCDE-4F01-2345-6789ABCDEF03', 'QH Flight Administration', NOW());

-- =====================================================
-- 2. SEED FLIGHTS DATA
-- =====================================================

INSERT INTO flights."Airlines" (Id, IataCode, Name, LogoUrl, BaseCountry, IsActive, IsDeleted, CreatedAt) VALUES
('A1F3E2D4-BCDE-4F01-2345-6789ABCDEF01', 'VN', 'Vietnam Airlines', 'https://images.vietnamairlines.com/logos/vna-logo.png', 'Vietnam', 1, 0, NOW()),
('B2F3E2D4-BCDE-4F01-2345-6789ABCDEF02', 'VJ', 'VietJet Air', 'https://www.vietjetair.com/static/media/logo.8efdcd6f.svg', 'Vietnam', 1, 0, NOW()),
('C3F3E2D4-BCDE-4F01-2345-6789ABCDEF03', 'QH', 'Bamboo Airways', 'https://www.bambooairways.com/logo.png', 'Vietnam', 1, 0, NOW()),
('D4F3E2D4-BCDE-4F01-2345-6789ABCDEF04', 'VU', 'Vietravel Airlines', 'https://www.vietravelairlines.com/logo.png', 'Vietnam', 1, 0, NOW()),
('E1F3E2D4-BCDE-4F01-2345-6789ABCDEF27', 'SQ', 'Singapore Airlines', 'https://www.singaporeair.com/logo.png', 'Singapore', 1, 0, NOW()),
('E2F3E2D4-BCDE-4F01-2345-6789ABCDEF28', 'JL', 'Japan Airlines', 'https://www.jal.co.jp/logo.png', 'Japan', 1, 0, NOW()),
('E3F3E2D4-BCDE-4F01-2345-6789ABCDEF29', 'KE', 'Korean Air', 'https://www.koreanair.com/logo.png', 'South Korea', 1, 0, NOW()),
('E4F3E2D4-BCDE-4F01-2345-6789ABCDEF30', 'CX', 'Cathay Pacific', 'https://www.cathaypacific.com/logo.png', 'Hong Kong', 1, 0, NOW());

INSERT INTO flights."Airports" (Id, IataCode, NameEn, NameVi, CityEn, CityVi, CountryCode, Timezone, Latitude, Longitude, IsActive, IsDeleted) VALUES
('E5F3E2D4-BCDE-4F01-2345-6789ABCDEF05', 'HAN', 'Noi Bai International Airport', 'Sân bay quốc tế Nội Bài', 'Hanoi', 'Hà Nội', 'VN', 'Asia/Ho_Chi_Minh', 21.2212, 105.8072, 1, 0),
('F6F3E2D4-BCDE-4F01-2345-6789ABCDEF06', 'SGN', 'Tan Son Nhat International Airport', 'Sân bay quốc tế Tân Sơn Nhất', 'Ho Chi Minh City', 'TP. Hồ Chí Minh', 'VN', 'Asia/Ho_Chi_Minh', 10.8188, 106.6519, 1, 0),
('07F3E2D4-BCDE-4F01-2345-6789ABCDEF07', 'DAD', 'Da Nang International Airport', 'Sân bay quốc tế Đà Nẵng', 'Da Nang', 'Đà Nẵng', 'VN', 'Asia/Ho_Chi_Minh', 16.0439, 108.1994, 1, 0),
('08F3E2D4-BCDE-4F01-2345-6789ABCDEF08', 'NRT', 'Narita International Airport', 'Sân bay quốc tế Narita', 'Tokyo', 'Tokyo', 'JP', 'Asia/Tokyo', 35.7647, 140.3863, 1, 0),
('09F3E2D4-BCDE-4F01-2345-6789ABCDEF09', 'HND', 'Haneda Airport', 'Sân bay Haneda', 'Tokyo', 'Tokyo', 'JP', 'Asia/Tokyo', 35.5494, 139.7798, 1, 0),
('10F3E2D4-BCDE-4F01-2345-6789ABCDEF10', 'KIX', 'Kansai International Airport', 'Sân bay quốc tế Kansai', 'Osaka', 'Osaka', 'JP', 'Asia/Tokyo', 34.4320, 135.2304, 1, 0),
('11F3E2D4-BCDE-4F01-2345-6789ABCDEF11', 'ICN', 'Incheon International Airport', 'Sân bay quốc tế Incheon', 'Seoul', 'Seoul', 'KR', 'Asia/Seoul', 37.4602, 126.4407, 1, 0),
('12F3E2D4-BCDE-4F01-2345-6789ABCDEF12', 'PUS', 'Gimhae International Airport', 'Sân bay quốc tế Gimhae', 'Busan', 'Busan', 'KR', 'Asia/Seoul', 35.1795, 128.9382, 1, 0),
('13F3E2D4-BCDE-4F01-2345-6789ABCDEF13', 'PEK', 'Beijing Capital International Airport', 'Sân bay quốc tế Thủ đô Bắc Kinh', 'Beijing', 'Bắc Kinh', 'CN', 'Asia/Shanghai', 40.0799, 116.6031, 1, 0),
('14F3E2D4-BCDE-4F01-2345-6789ABCDEF14', 'PVG', 'Shanghai Pudong International Airport', 'Sân bay quốc tế Phố Đông Thượng Hải', 'Shanghai', 'Thượng Hải', 'CN', 'Asia/Shanghai', 31.1443, 121.8083, 1, 0),
('15F3E2D4-BCDE-4F01-2345-6789ABCDEF15', 'CAN', 'Guangzhou Baiyun International Airport', 'Sân bay quốc tế Bạch Vân Quảng Châu', 'Guangzhou', 'Quảng Châu', 'CN', 'Asia/Shanghai', 23.3924, 113.2988, 1, 0),
('16F3E2D4-BCDE-4F01-2345-6789ABCDEF16', 'HKG', 'Hong Kong International Airport', 'Sân bay quốc tế Hồng Kông', 'Hong Kong', 'Hồng Kông', 'HK', 'Asia/Hong_Kong', 22.3080, 113.9185, 1, 0),
('17F3E2D4-BCDE-4F01-2345-6789ABCDEF17', 'TPE', 'Taoyuan International Airport', 'Sân bay quốc tế Đào Viên', 'Taipei', 'Đài Bắc', 'TW', 'Asia/Taipei', 25.0797, 121.2342, 1, 0),
('18F3E2D4-BCDE-4F01-2345-6789ABCDEF18', 'BKK', 'Suvarnabhumi Airport', 'Sân bay Suvarnabhumi', 'Bangkok', 'Bangkok', 'TH', 'Asia/Bangkok', 13.6900, 100.7501, 1, 0),
('19F3E2D4-BCDE-4F01-2345-6789ABCDEF19', 'DMK', 'Don Mueang International Airport', 'Sân bay quốc tế Don Mueang', 'Bangkok', 'Bangkok', 'TH', 'Asia/Bangkok', 13.9126, 100.6068, 1, 0),
('20F3E2D4-BCDE-4F01-2345-6789ABCDEF20', 'HKT', 'Phuket International Airport', 'Sân bay quốc tế Phuket', 'Phuket', 'Phuket', 'TH', 'Asia/Bangkok', 8.1111, 98.3065, 1, 0),
('21F3E2D4-BCDE-4F01-2345-6789ABCDEF21', 'SIN', 'Changi Airport', 'Sân bay Changi', 'Singapore', 'Singapore', 'SG', 'Asia/Singapore', 1.3644, 103.9915, 1, 0),
('22F3E2D4-BCDE-4F01-2345-6789ABCDEF22', 'KUL', 'Kuala Lumpur International Airport', 'Sân bay quốc tế Kuala Lumpur', 'Kuala Lumpur', 'Kuala Lumpur', 'MY', 'Asia/Kuala_Lumpur', 2.7456, 101.7099, 1, 0),
('23F3E2D4-BCDE-4F01-2345-6789ABCDEF23', 'CGK', 'Soekarno-Hatta International Airport', 'Sân bay quốc tế Soekarno-Hatta', 'Jakarta', 'Jakarta', 'ID', 'Asia/Jakarta', -6.1256, 106.6558, 1, 0),
('24F3E2D4-BCDE-4F01-2345-6789ABCDEF24', 'DPS', 'Ngurah Rai International Airport', 'Sân bay quốc tế Ngurah Rai', 'Bali', 'Bali', 'ID', 'Asia/Makassar', -8.7482, 115.1675, 1, 0),
('25F3E2D4-BCDE-4F01-2345-6789ABCDEF25', 'MNL', 'Ninoy Aquino International Airport', 'Sân bay quốc tế Ninoy Aquino', 'Manila', 'Manila', 'PH', 'Asia/Manila', 14.5086, 121.0194, 1, 0),
('26F3E2D4-BCDE-4F01-2345-6789ABCDEF26', 'SYD', 'Sydney Airport', 'Sân bay Sydney', 'Sydney', 'Sydney', 'AU', 'Australia/Sydney', -33.9399, 151.1753, 1, 0),
('27F3E2D4-BCDE-4F01-2345-6789ABCDEF27', 'MEL', 'Melbourne Airport', 'Sân bay Melbourne', 'Melbourne', 'Melbourne', 'AU', 'Australia/Melbourne', -37.6690, 144.8410, 1, 0),
('28F3E2D4-BCDE-4F01-2345-6789ABCDEF28', 'LHR', 'Heathrow Airport', 'Sân bay Heathrow', 'London', 'Luân Đôn', 'GB', 'Europe/London', 51.4700, -0.4543, 1, 0),
('29F3E2D4-BCDE-4F01-2345-6789ABCDEF29', 'CDG', 'Charles de Gaulle Airport', 'Sân bay Charles de Gaulle', 'Paris', 'Paris', 'FR', 'Europe/Paris', 49.0097, 2.5479, 1, 0),
('30F3E2D4-BCDE-4F01-2345-6789ABCDEF30', 'FRA', 'Frankfurt Airport', 'Sân bay Frankfurt', 'Frankfurt', 'Frankfurt', 'DE', 'Europe/Berlin', 50.0333, 8.5706, 1, 0),
('31F3E2D4-BCDE-4F01-2345-6789ABCDEF31', 'AMS', 'Amsterdam Airport Schiphol', 'Sân bay Schiphol', 'Amsterdam', 'Amsterdam', 'NL', 'Europe/Amsterdam', 52.3105, 4.7683, 1, 0),
('32F3E2D4-BCDE-4F01-2345-6789ABCDEF32', 'DXB', 'Dubai International Airport', 'Sân bay quốc tế Dubai', 'Dubai', 'Dubai', 'AE', 'Asia/Dubai', 25.2532, 55.3657, 1, 0),
('33F3E2D4-BCDE-4F01-2345-6789ABCDEF33', 'DOH', 'Hamad International Airport', 'Sân bay quốc tế Hamad', 'Doha', 'Doha', 'QA', 'Asia/Qatar', 25.2731, 51.6080, 1, 0),
('34F3E2D4-BCDE-4F01-2345-6789ABCDEF34', 'JFK', 'John F. Kennedy International Airport', 'Sân bay quốc tế John F. Kennedy', 'New York', 'New York', 'US', 'America/New_York', 40.6413, -73.7781, 1, 0),
('35F3E2D4-BCDE-4F01-2345-6789ABCDEF35', 'LAX', 'Los Angeles International Airport', 'Sân bay quốc tế Los Angeles', 'Los Angeles', 'Los Angeles', 'US', 'America/Los_Angeles', 33.9416, -118.4085, 1, 0),
('36F3E2D4-BCDE-4F01-2345-6789ABCDEF36', 'SFO', 'San Francisco International Airport', 'Sân bay quốc tế San Francisco', 'San Francisco', 'San Francisco', 'US', 'America/Los_Angeles', 37.6213, -122.3790, 1, 0),
('37F3E2D4-BCDE-4F01-2345-6789ABCDEF37', 'YVR', 'Vancouver International Airport', 'Sân bay quốc tế Vancouver', 'Vancouver', 'Vancouver', 'CA', 'America/Vancouver', 49.1967, -123.1815, 1, 0);

INSERT INTO flights."Airplanes" (Id, AirlineId, Model, RegistrationNumber, TotalCapacity, IsDeleted) VALUES
('81F3E2D4-BCDE-4F01-2345-6789ABCDEF01', 'A1F3E2D4-BCDE-4F01-2345-6789ABCDEF01', 'Boeing 787-9 Dreamliner', 'VN-A861', 274, 0),
('82F3E2D4-BCDE-4F01-2345-6789ABCDEF02', 'A1F3E2D4-BCDE-4F01-2345-6789ABCDEF01', 'Airbus A350-900', 'VN-A886', 305, 0),
('83F3E2D4-BCDE-4F01-2345-6789ABCDEF03', 'B2F3E2D4-BCDE-4F01-2345-6789ABCDEF02', 'Airbus A321neo', 'VN-A600', 230, 0),
('84F3E2D4-BCDE-4F01-2345-6789ABCDEF04', 'C3F3E2D4-BCDE-4F01-2345-6789ABCDEF03', 'Boeing 787-9 Dreamliner', 'VN-A819', 294, 0);

INSERT INTO flights."Routes" (Id, AirlineId, OriginAirportId, DestinationAirportId, DistanceKm, EstimatedDurationMinutes, IsDeleted) VALUES
('91F3E2D4-BCDE-4F01-2345-6789ABCDEF01', 'A1F3E2D4-BCDE-4F01-2345-6789ABCDEF01', 'E5F3E2D4-BCDE-4F01-2345-6789ABCDEF05', 'F6F3E2D4-BCDE-4F01-2345-6789ABCDEF06', 1160, 130, 0),
('92F3E2D4-BCDE-4F01-2345-6789ABCDEF02', 'B2F3E2D4-BCDE-4F01-2345-6789ABCDEF02', 'F6F3E2D4-BCDE-4F01-2345-6789ABCDEF06', 'E5F3E2D4-BCDE-4F01-2345-6789ABCDEF05', 1160, 130, 0),
('93F3E2D4-BCDE-4F01-2345-6789ABCDEF03', 'A1F3E2D4-BCDE-4F01-2345-6789ABCDEF01', 'E5F3E2D4-BCDE-4F01-2345-6789ABCDEF05', '07F3E2D4-BCDE-4F01-2345-6789ABCDEF07', 627, 85, 0);

INSERT INTO flights."Flights" (Id, RouteId, AirplaneId, FlightNumber, DepartureTime, ArrivalTime, BasePrice, Currency, Status, IsDeleted, CreatedAt, UpdatedAt) VALUES
('F1F3E2D4-BCDE-4F01-2345-6789ABCDEF01', '91F3E2D4-BCDE-4F01-2345-6789ABCDEF01', '81F3E2D4-BCDE-4F01-2345-6789ABCDEF01', 'VN213', NOW() + INTERVAL '1 day', NOW() + INTERVAL '1 day 130 minutes', 80.00, 'USD', 0, 0, NOW(), NOW()),
('F2F3E2D4-BCDE-4F01-2345-6789ABCDEF02', '92F3E2D4-BCDE-4F01-2345-6789ABCDEF02', '83F3E2D4-BCDE-4F01-2345-6789ABCDEF03', 'VJ120', NOW() + INTERVAL '2 days', NOW() + INTERVAL '2 days 130 minutes', 50.00, 'USD', 0, 0, NOW(), NOW());

INSERT INTO flights."FlightSeats" (Id, FlightId, SeatNumber, SeatClass, PriceOverride, IsAvailable, IsExtraLegroom) VALUES
(gen_random_uuid(), 'F1F3E2D4-BCDE-4F01-2345-6789ABCDEF01', '01A', 2, 150.00, 1, 1),
(gen_random_uuid(), 'F1F3E2D4-BCDE-4F01-2345-6789ABCDEF01', '01B', 2, 150.00, 1, 1),
(gen_random_uuid(), 'F1F3E2D4-BCDE-4F01-2345-6789ABCDEF01', '10A', 0, NULL, 1, 0),
(gen_random_uuid(), 'F1F3E2D4-BCDE-4F01-2345-6789ABCDEF01', '10B', 0, NULL, 1, 0),
(gen_random_uuid(), 'F1F3E2D4-BCDE-4F01-2345-6789ABCDEF01', '11A', 0, NULL, 1, 0),
(gen_random_uuid(), 'F1F3E2D4-BCDE-4F01-2345-6789ABCDEF01', '11B', 0, NULL, 1, 0);

INSERT INTO flights."FlightSeats" (Id, FlightId, SeatNumber, SeatClass, PriceOverride, IsAvailable, IsExtraLegroom) VALUES
(gen_random_uuid(), 'F2F3E2D4-BCDE-4F01-2345-6789ABCDEF02', '01A', 1, 80.00, 1, 1),
(gen_random_uuid(), 'F2F3E2D4-BCDE-4F01-2345-6789ABCDEF02', '01B', 1, 80.00, 1, 1),
(gen_random_uuid(), 'F2F3E2D4-BCDE-4F01-2345-6789ABCDEF02', '05A', 0, NULL, 1, 0),
(gen_random_uuid(), 'F2F3E2D4-BCDE-4F01-2345-6789ABCDEF02', '05B', 0, NULL, 1, 0);

-- =====================================================
-- 3. SEED BOOKINGS DATA
-- =====================================================

INSERT INTO bookings."Bookings" (Id, UserId, PnrCode, TotalPrice, Currency, Status, ContactEmail, ContactPhone, SpecialRequests, IsDeleted, CreatedAt, UpdatedAt) VALUES
('B1B0F2A8-9B2F-4A9B-89E3-4E80D77BC901', 'E2E3F4C5-1A2B-3C4D-5E6F-7A8B9C0D1E2F', 'PNR123', 160.00, 'USD', 2, 'client@gmail.com', '0987654321', 'No Special Requests', 0, NOW(), NOW());

INSERT INTO bookings."Passengers" (Id, BookingId, FirstName, LastName, Gender, DateOfBirth, Nationality, PassportNumber, PassportExpiryDate) VALUES
('FA10F2A8-9B2F-4A9B-89E3-4E80D77BC901', 'B1B0F2A8-9B2F-4A9B-89E3-4E80D77BC901', 'Văn A', 'Nguyễn', 0, '1990-05-15', 'Vietnam', 'B1234567', '2030-05-15');

INSERT INTO bookings."Tickets" (Id, BookingId, PassengerId, FlightId, SeatId, TicketNumber, Gate, BoardingTime, Status) 
SELECT 
    'B110F2A8-9B2F-4A9B-89E3-4E80D77BC901', 
    'B1B0F2A8-9B2F-4A9B-89E3-4E80D77BC901', 
    'FA10F2A8-9B2F-4A9B-89E3-4E80D77BC901', 
    'F1F3E2D4-BCDE-4F01-2345-6789ABCDEF01', 
    fs.Id, 
    'TK-VN-20250615-001', 
    'Gate 5', 
    NOW() + INTERVAL '1 day' - INTERVAL '40 minutes', 
    0
FROM flights."FlightSeats" fs 
WHERE fs.FlightId = 'F1F3E2D4-BCDE-4F01-2345-6789ABCDEF01' AND fs.SeatNumber = '10A';

INSERT INTO bookings."Payments" (Id, BookingId, TransactionId, Amount, PaymentMethod, ProviderStatus, IsSuccessful, RawResponse, CreatedAt) VALUES
('FF10F2A8-9B2F-4A9B-89E3-4E80D77BC901', 'B1B0F2A8-9B2F-4A9B-89E3-4E80D77BC901', 'TXN-987654321', 160.00, 'Stripe', 'succeeded', 1, '{"status": "succeeded", "charge_id": "ch_123"}', NOW());

-- =====================================================
-- 4. SEED PROMOTIONS DATA
-- =====================================================

INSERT INTO promotions."Coupons" (Id, Code, Description, DiscountType, DiscountValue, MinOrderValue, MaxDiscountAmount, StartDate, EndDate, UsageLimit, UsageCount, IsActive, IsDeleted) VALUES
('C1F3E2D4-BCDE-4F01-2345-6789ABCDEF01', 'SUMMER2025', 'Giảm giá 10% dịp Hè', 0, 10.00, 100.00, 50.00, NOW(), NOW() + INTERVAL '3 months', 1000, 0, 1, 0),
('C2F3E2D4-BCDE-4F01-2345-6789ABCDEF02', 'WELCOME10', 'Giảm 10$ cho đơn hàng đầu tiên', 1, 10.00, 0.00, 10.00, NOW(), NOW() + INTERVAL '1 year', 5000, 0, 1, 0);

INSERT INTO promotions."Campaigns" (Id, Title, BannerUrl, Content, StartDate, EndDate, IsFeatured, IsDeleted) VALUES
('CC13E2D4-BCDE-4F01-2345-6789ABCDEF01', 'Siêu Hè Rực Rỡ 2025', 'https://images.vietnamairlines.com/banners/summer-2025.jpg', 'Giảm giá lên đến 20% các chặng bay nội địa và quốc tế dịp hè từ 01/06 đến 31/08/2025.', NOW(), NOW() + INTERVAL '3 months', 1, 0),
('CC23E2D4-BCDE-4F01-2345-6789ABCDEF02', 'Mùa Thu Vàng', 'https://images.vietnamairlines.com/banners/autumn-2025.jpg', 'Đón thu vàng cùng ngập tràn khuyến mãi vé bay khứ hồi giá cực tốt.', NOW() + INTERVAL '3 months', NOW() + INTERVAL '5 months', 0, 0);

-- =====================================================
-- 5. SEED CMS DATA
-- =====================================================

INSERT INTO cms."Categories" (Id, Name, Slug, IsDeleted) VALUES
('CA11E2D4-BCDE-4F01-2345-6789ABCDEF01', 'Tin Tức Khuyến Mãi', 'tin-tuc-khuyen-mai', 0),
('CA22E2D4-BCDE-4F01-2345-6789ABCDEF02', 'Cẩm Nang Du Lịch', 'cam-nang-du-lich', 0);

INSERT INTO cms."Articles" (Id, CategoryId, AuthorId, Title, Slug, Summary, Content, ThumbnailUrl, PublishedAt, Status, ViewCount, IsDeleted, CreatedAt) VALUES
('AE10E2D4-BCDE-4F01-2345-6789ABCDEF01', 'CA22E2D4-BCDE-4F01-2345-6789ABCDEF02', 'D4B0F2A8-9B2F-4A9B-89E3-4E80D77BC901', 'Cẩm nang du lịch Phú Quốc từ A đến Z năm 2025', 'cam-nang-du-lich-phu-quoc-2025', 'Những lưu ý quan trọng khi đi du lịch Phú Quốc tự túc.', 'Bài viết này cung cấp toàn bộ kinh nghiệm bay, đặt khách sạn, ẩm thực và địa điểm vui chơi tại Phú Quốc cho du khách.', 'https://images.phuquoc.vn/thumbnail.jpg', NOW(), 1, 245, 0, NOW());

-- =====================================================
-- 6. SEED INTERACTIONS DATA
-- =====================================================

INSERT INTO interactions."Reviews" (Id, UserId, AirlineId, FlightId, Rating, Comment, IsVerifiedPurchase, IsHidden, IsDeleted, CreatedAt) VALUES
('EE10E2D4-BCDE-4F01-2345-6789ABCDEF01', 'E2E3F4C5-1A2B-3C4D-5E6F-7A8B9C0D1E2F', 'A1F3E2D4-BCDE-4F01-2345-6789ABCDEF01', 'F1F3E2D4-BCDE-4F01-2345-6789ABCDEF01', 5, 'Dịch vụ của Vietnam Airlines rất tốt, bay đúng giờ, tiếp viên thân thiện.', 1, 0, 0, NOW());

-- =====================================================
-- 7. SEED NOTIFICATIONS DATA
-- =====================================================

INSERT INTO notifications."NotificationTemplates" (Id, Code, Subject, BodyTemplate, Language, CreatedAt) VALUES
('F1F3E2D4-BCDE-4F01-2345-6789ABCDEF01', 'BOOKING_CONFIRMED', 'Xác nhận đặt vé thành công', 'Chào {{PassengerName}}, Đặt chỗ của bạn (Mã: {{PnrCode}}) đã được xác nhận thành công. Chuyến bay: {{FlightNumber}}.', 'vi', NOW()),
('F2F3E2D4-BCDE-4F01-2345-6789ABCDEF02', 'BOOKING_CONFIRMED_EN', 'Booking Confirmation', 'Hello {{PassengerName}}, Your booking (PNR: {{PnrCode}}) has been confirmed. Flight: {{FlightNumber}}.', 'en', NOW()),
(gen_random_uuid(), 'FLIGHT_CREATED', 'New Flight Created: {{FlightNumber}}', 'A new flight {{FlightNumber}} from {{Origin}} to {{Destination}} has been created by a partner.', 'en', NOW()),
(gen_random_uuid(), 'FLIGHT_CREATED', 'Chuyến bay mới được tạo: {{FlightNumber}}', 'Chuyến bay mới {{FlightNumber}} từ {{Origin}} đến {{Destination}} vừa được tạo bởi đối tác.', 'vi', NOW()),
(gen_random_uuid(), 'FLIGHT_CREATED', '新航班已创建: {{FlightNumber}}', '合作伙伴已创建从 {{Origin}} 到 {{Destination}} 的新航班 {{FlightNumber}}。', 'zh', NOW()),
(gen_random_uuid(), 'FLIGHT_CREATED', '新しいフライトが作成されました: {{FlightNumber}}', 'パートナーによって {{Origin}} から {{Destination}} への新しいフライト {{FlightNumber}} が作成されました。', 'ja', NOW()),
(gen_random_uuid(), 'FLIGHT_CREATED', '새 항공편 생성됨: {{FlightNumber}}', '파트너가 {{Origin}}에서 {{Destination}}으로 가는 새 항공편 {{FlightNumber}}을(를) 생성했습니다.', 'ko', NOW()),
(gen_random_uuid(), 'FLIGHT_CREATED', 'Nouveau vol créé : {{FlightNumber}}', 'Un nouveau vol {{FlightNumber}} de {{Origin}} à {{Destination}} a été créé par un partenaire.', 'fr', NOW());

-- =====================================================
-- 8. SEED ROLE PERMISSIONS
-- =====================================================

INSERT INTO users."RolePermissions" (RoleId, PermissionId)
SELECT 0, Id FROM users."Permissions";

INSERT INTO users."RolePermissions" (RoleId, PermissionId)
SELECT 1, Id FROM users."Permissions"
WHERE Code IN ('CREATE_FLIGHT', 'SELL_TICKET', 'MANAGE_AIRLINE_STAFF', 'MANAGE_FLIGHTS', 'MANAGE_AIRLINES', 'MANAGE_BOOKINGS');

INSERT INTO users."RolePermissions" (RoleId, PermissionId)
SELECT 2, Id FROM users."Permissions"
WHERE Code IN ('SELL_TICKET', 'MANAGE_FLIGHTS', 'MANAGE_BOOKINGS');

INSERT INTO users."RolePermissions" (RoleId, PermissionId)
SELECT 3, Id FROM users."Permissions"
WHERE Code IN ('SELL_TICKET');

-- =====================================================
-- 9. SEED AIRCRAFT MODELS
-- =====================================================

INSERT INTO flights."AircraftModels" (Id, Name, Manufacturer, TotalSeats, IsDeleted) VALUES
('B7879000-BCDE-4F01-2345-6789ABCDEF01', 'Boeing 787-9 Dreamliner', 'Boeing', 294, 0),
('A3509000-BCDE-4F01-2345-6789ABCDEF02', 'Airbus A350-900', 'Airbus', 305, 0),
('A3212000-BCDE-4F01-2345-6789ABCDEF03', 'Airbus A321neo', 'Airbus', 230, 0);

UPDATE flights."Airplanes" SET AircraftModelId = 'B7879000-BCDE-4F01-2345-6789ABCDEF01' WHERE Id IN ('81F3E2D4-BCDE-4F01-2345-6789ABCDEF01', '84F3E2D4-BCDE-4F01-2345-6789ABCDEF04');
UPDATE flights."Airplanes" SET AircraftModelId = 'A3509000-BCDE-4F01-2345-6789ABCDEF02' WHERE Id = '82F3E2D4-BCDE-4F01-2345-6789ABCDEF02';
UPDATE flights."Airplanes" SET AircraftModelId = 'A3212000-BCDE-4F01-2345-6789ABCDEF03' WHERE Id = '83F3E2D4-BCDE-4F01-2345-6789ABCDEF03';

