-- =====================================================
-- AIRLINE TICKET SYSTEM - SEED DATA
-- =====================================================


-- =====================================================
-- 1. SEED IDENTITY DATA (Roles, Permissions, Users)
-- =====================================================

-- Initialize fixed Roles list with i18n translation codes

INSERT INTO users."Roles" (Id, Name, Description) VALUES
(0, 'role.system_admin.name', 'role.system_admin.desc'),
(1, 'role.airline_admin.name', 'role.airline_admin.desc'),
(2, 'role.airline_staff.name', 'role.airline_staff.desc'),
(3, 'role.user_client.name', 'role.user_client.desc');



-- Initialize sample Permissions with i18n translation codes
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


-- Initialize sample Users (Admin and Client)
-- Default passwords have corresponding hashes
INSERT INTO users."Users" (Id, Email, EmailConfirmed, PasswordHash, FullName, Phone, IsActive, IsDeleted, CreatedAt, UpdatedAt, Role) VALUES
('D4B0F2A8-9B2F-4A9B-89E3-4E80D77BC901', 'admin@airlineticket.com', 1, '$2b$12$d7mwUGX/x5jcRrxdNAFqJekces0IQ2f9F4mC24iVGZUpwM2pF1Qly', 'Hệ Thống Admin', '0123456789', 1, 0, NOW(), NOW(), 0),
('E2E3F4C5-1A2B-3C4D-5E6F-7A8B9C0D1E2F', 'client@gmail.com', 1, '$2b$12$d7mwUGX/x5jcRrxdNAFqJekces0IQ2f9F4mC24iVGZUpwM2pF1Qly', 'Nguyễn Văn A', '0987654321', 1, 0, NOW(), NOW(), 2),
('A1F3E2D4-BCDE-4F01-2345-6789ABCDEF91', 'admin.vna@airlineticket.com', 1, '$2b$12$d7mwUGX/x5jcRrxdNAFqJekces0IQ2f9F4mC24iVGZUpwM2pF1Qly', 'Admin VNA', '0111111111', 1, 0, NOW(), NOW(), 1),
('B2F3E2D4-BCDE-4F01-2345-6789ABCDEF92', 'admin.vj@airlineticket.com', 1, '$2b$12$d7mwUGX/x5jcRrxdNAFqJekces0IQ2f9F4mC24iVGZUpwM2pF1Qly', 'Admin VJ', '0222222222', 1, 0, NOW(), NOW(), 1),
('C3F3E2D4-BCDE-4F01-2345-6789ABCDEF93', 'admin.qh@airlineticket.com', 1, '$2b$12$d7mwUGX/x5jcRrxdNAFqJekces0IQ2f9F4mC24iVGZUpwM2pF1Qly', 'Admin QH', '0333333333', 1, 0, NOW(), NOW(), 1);


-- Assign admin permissions for Airline Admins
INSERT INTO users."UserPermissionScopes" (Id, UserId, PermissionId, AirlineId, ScopeDescription, CreatedAt) VALUES
(gen_random_uuid(), 'C3F3E2D4-BCDE-4F01-2345-6789ABCDEF93', 3, 'C3F3E2D4-BCDE-4F01-2345-6789ABCDEF03', 'Bamboo Airways Administration', NOW()),
(gen_random_uuid(), 'C3F3E2D4-BCDE-4F01-2345-6789ABCDEF93', 5, 'C3F3E2D4-BCDE-4F01-2345-6789ABCDEF03', 'QH Flight Administration', NOW());


-- =====================================================
-- 2. SEED FLIGHTS DATA
-- =====================================================

-- Initialize Airlines list for Frontend display
INSERT INTO flights."Airlines" (Id, IataCode, Name, LogoUrl, BaseCountry, IsActive, IsDeleted, CreatedAt) VALUES
('A1F3E2D4-BCDE-4F01-2345-6789ABCDEF01', 'VN', 'Vietnam Airlines', 'https://images.vietnamairlines.com/logos/vna-logo.png', 'Vietnam', 1, 0, NOW()),
('B2F3E2D4-BCDE-4F01-2345-6789ABCDEF02', 'VJ', 'VietJet Air', 'https://www.vietjetair.com/static/media/logo.8efdcd6f.svg', 'Vietnam', 1, 0, NOW()),
('C3F3E2D4-BCDE-4F01-2345-6789ABCDEF03', 'QH', 'Bamboo Airways', 'https://www.bambooairways.com/logo.png', 'Vietnam', 1, 0, NOW()),
('D4F3E2D4-BCDE-4F01-2345-6789ABCDEF04', 'VU', 'Vietravel Airlines', 'https://www.vietravelairlines.com/logo.png', 'Vietnam', 1, 0, NOW()),
('E1F3E2D4-BCDE-4F01-2345-6789ABCDEF27', 'SQ', 'Singapore Airlines', 'https://www.singaporeair.com/logo.png', 'Singapore', 1, 0, NOW()),
('E2F3E2D4-BCDE-4F01-2345-6789ABCDEF28', 'JL', 'Japan Airlines', 'https://www.jal.co.jp/logo.png', 'Japan', 1, 0, NOW()),
('E3F3E2D4-BCDE-4F01-2345-6789ABCDEF29', 'KE', 'Korean Air', 'https://www.koreanair.com/logo.png', 'South Korea', 1, 0, NOW()),
('E4F3E2D4-BCDE-4F01-2345-6789ABCDEF30', 'CX', 'Cathay Pacific', 'https://www.cathaypacific.com/logo.png', 'Hong Kong', 1, 0, NOW());


-- Initialize popular Airports in Vietnam and International
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


-- Seed Airplanes (Sample airplanes)
INSERT INTO flights."Airplanes" (Id, AirlineId, Model, RegistrationNumber, TotalCapacity, IsDeleted) VALUES
('81F3E2D4-BCDE-4F01-2345-6789ABCDEF01', 'A1F3E2D4-BCDE-4F01-2345-6789ABCDEF01', 'Boeing 787-9 Dreamliner', 'VN-A861', 274, 0),
('82F3E2D4-BCDE-4F01-2345-6789ABCDEF02', 'A1F3E2D4-BCDE-4F01-2345-6789ABCDEF01', 'Airbus A350-900', 'VN-A886', 305, 0),
('83F3E2D4-BCDE-4F01-2345-6789ABCDEF03', 'B2F3E2D4-BCDE-4F01-2345-6789ABCDEF02', 'Airbus A321neo', 'VN-A600', 230, 0),
('84F3E2D4-BCDE-4F01-2345-6789ABCDEF04', 'C3F3E2D4-BCDE-4F01-2345-6789ABCDEF03', 'Boeing 787-9 Dreamliner', 'VN-A819', 294, 0);


-- Seed Routes (Sample routes)
INSERT INTO flights."Routes" (Id, AirlineId, OriginAirportId, DestinationAirportId, DistanceKm, EstimatedDurationMinutes, IsDeleted) VALUES
('91F3E2D4-BCDE-4F01-2345-6789ABCDEF01', 'A1F3E2D4-BCDE-4F01-2345-6789ABCDEF01', 'E5F3E2D4-BCDE-4F01-2345-6789ABCDEF05', 'F6F3E2D4-BCDE-4F01-2345-6789ABCDEF06', 1160, 130, 0), -- HAN to SGN (VN)
('92F3E2D4-BCDE-4F01-2345-6789ABCDEF02', 'B2F3E2D4-BCDE-4F01-2345-6789ABCDEF02', 'F6F3E2D4-BCDE-4F01-2345-6789ABCDEF06', 'E5F3E2D4-BCDE-4F01-2345-6789ABCDEF05', 1160, 130, 0), -- SGN to HAN (VJ)
('93F3E2D4-BCDE-4F01-2345-6789ABCDEF03', 'A1F3E2D4-BCDE-4F01-2345-6789ABCDEF01', 'E5F3E2D4-BCDE-4F01-2345-6789ABCDEF05', '07F3E2D4-BCDE-4F01-2345-6789ABCDEF07', 627, 85, 0);   -- HAN to DAD (VN)


-- Seed Flights (Sample flights)
INSERT INTO flights."Flights" (Id, RouteId, AirplaneId, FlightNumber, DepartureTime, ArrivalTime, BasePrice, Currency, Status, IsDeleted, CreatedAt, UpdatedAt) VALUES
('F1F3E2D4-BCDE-4F01-2345-6789ABCDEF01', '91F3E2D4-BCDE-4F01-2345-6789ABCDEF01', '81F3E2D4-BCDE-4F01-2345-6789ABCDEF01', 'VN213', DATEADD(day, 1, NOW()), DATEADD(minute, 130, DATEADD(day, 1, NOW())), 80.00, 'USD', 0, 0, NOW(), NOW()),
('F2F3E2D4-BCDE-4F01-2345-6789ABCDEF02', '92F3E2D4-BCDE-4F01-2345-6789ABCDEF02', '83F3E2D4-BCDE-4F01-2345-6789ABCDEF03', 'VJ120', DATEADD(day, 2, NOW()), DATEADD(minute, 130, DATEADD(day, 2, NOW())), 50.00, 'USD', 0, 0, NOW(), NOW());


-- Seed FlightSeats (Flight seats)
-- Seat configuration for VN213
INSERT INTO flights."FlightSeats" (Id, FlightId, SeatNumber, SeatClass, PriceOverride, IsAvailable, IsExtraLegroom) VALUES
(gen_random_uuid(), 'F1F3E2D4-BCDE-4F01-2345-6789ABCDEF01', '01A', 2, 150.00, 1, 1), -- Business
(gen_random_uuid(), 'F1F3E2D4-BCDE-4F01-2345-6789ABCDEF01', '01B', 2, 150.00, 1, 1),
(gen_random_uuid(), 'F1F3E2D4-BCDE-4F01-2345-6789ABCDEF01', '10A', 0, NULL, 1, 0),   -- Economy
(gen_random_uuid(), 'F1F3E2D4-BCDE-4F01-2345-6789ABCDEF01', '10B', 0, NULL, 1, 0),
(gen_random_uuid(), 'F1F3E2D4-BCDE-4F01-2345-6789ABCDEF01', '11A', 0, NULL, 1, 0),
(gen_random_uuid(), 'F1F3E2D4-BCDE-4F01-2345-6789ABCDEF01', '11B', 0, NULL, 1, 0);

-- Seat configuration for VJ120
INSERT INTO flights."FlightSeats" (Id, FlightId, SeatNumber, SeatClass, PriceOverride, IsAvailable, IsExtraLegroom) VALUES
(gen_random_uuid(), 'F2F3E2D4-BCDE-4F01-2345-6789ABCDEF02', '01A', 1, 80.00, 1, 1),  -- Premium Economy
(gen_random_uuid(), 'F2F3E2D4-BCDE-4F01-2345-6789ABCDEF02', '01B', 1, 80.00, 1, 1),
(gen_random_uuid(), 'F2F3E2D4-BCDE-4F01-2345-6789ABCDEF02', '05A', 0, NULL, 1, 0),   -- Economy
(gen_random_uuid(), 'F2F3E2D4-BCDE-4F01-2345-6789ABCDEF02', '05B', 0, NULL, 1, 0);



-- =====================================================
-- 3. SEED BOOKINGS DATA (Bookings, Passengers, Tickets, Payments)
-- =====================================================

-- Seed Bookings
INSERT INTO bookings."Bookings" (Id, UserId, PnrCode, TotalPrice, Currency, Status, ContactEmail, ContactPhone, SpecialRequests, IsDeleted, CreatedAt, UpdatedAt) VALUES
('B1B0F2A8-9B2F-4A9B-89E3-4E80D77BC901', 'E2E3F4C5-1A2B-3C4D-5E6F-7A8B9C0D1E2F', 'PNR123', 160.00, 'USD', 2, 'client@gmail.com', '0987654321', 'No Special Requests', 0, NOW(), NOW());


-- Seed Passengers
INSERT INTO bookings."Passengers" (Id, BookingId, FirstName, LastName, Gender, DateOfBirth, Nationality, PassportNumber, PassportExpiryDate) VALUES
('FA10F2A8-9B2F-4A9B-89E3-4E80D77BC901', 'B1B0F2A8-9B2F-4A9B-89E3-4E80D77BC901', 'Văn A', 'Nguyễn', 0, '1990-05-15', 'Vietnam', 'B1234567', '2030-05-15');


-- Seed Tickets
INSERT INTO bookings."Tickets" (Id, BookingId, PassengerId, FlightId, SeatId, TicketNumber, Gate, BoardingTime, Status) 
SELECT 
    'B110F2A8-9B2F-4A9B-89E3-4E80D77BC901', 
    'B1B0F2A8-9B2F-4A9B-89E3-4E80D77BC901', 
    'FA10F2A8-9B2F-4A9B-89E3-4E80D77BC901', 
    'F1F3E2D4-BCDE-4F01-2345-6789ABCDEF01', 
    fs.Id, 
    'TK-VN-20250615-001', 
    'Gate 5', 
    DATEADD(minute, -40, DATEADD(day, 1, NOW())), 
    0
FROM flights."FlightSeats" fs 
WHERE fs.FlightId = 'F1F3E2D4-BCDE-4F01-2345-6789ABCDEF01' AND fs.SeatNumber = '10A';


-- Seed Payments
INSERT INTO bookings."Payments" (Id, BookingId, TransactionId, Amount, PaymentMethod, ProviderStatus, IsSuccessful, RawResponse, CreatedAt) VALUES
('FF10F2A8-9B2F-4A9B-89E3-4E80D77BC901', 'B1B0F2A8-9B2F-4A9B-89E3-4E80D77BC901', 'TXN-987654321', 160.00, 'Stripe', 'succeeded', 1, '{"status": "succeeded", "charge_id": "ch_123"}', NOW());



-- =====================================================
-- 4. SEED PROMOTIONS DATA (Coupons, Campaigns)
-- =====================================================

INSERT INTO promotions."Coupons" (Id, Code, Description, DiscountType, DiscountValue, MinOrderValue, MaxDiscountAmount, StartDate, EndDate, UsageLimit, UsageCount, IsActive, IsDeleted) VALUES
('C1F3E2D4-BCDE-4F01-2345-6789ABCDEF01', 'SUMMER2025', 'Giảm giá 10% dịp Hè', 0, 10.00, 100.00, 50.00, NOW(), DATEADD(month, 3, NOW()), 1000, 0, 1, 0),
('C2F3E2D4-BCDE-4F01-2345-6789ABCDEF02', 'WELCOME10', 'Giảm 10$ cho đơn hàng đầu tiên', 1, 10.00, 0.00, 10.00, NOW(), DATEADD(year, 1, NOW()), 5000, 0, 1, 0);


-- Seed Campaigns (Marketing campaigns)
INSERT INTO promotions."Campaigns" (Id, Title, BannerUrl, Content, StartDate, EndDate, IsFeatured, IsDeleted) VALUES
('CC13E2D4-BCDE-4F01-2345-6789ABCDEF01', 'Siêu Hè Rực Rỡ 2025', 'https://images.vietnamairlines.com/banners/summer-2025.jpg', 'Giảm giá lên đến 20% các chặng bay nội địa và quốc tế dịp hè từ 01/06 đến 31/08/2025.', NOW(), DATEADD(month, 3, NOW()), 1, 0),
('CC23E2D4-BCDE-4F01-2345-6789ABCDEF02', 'Mùa Thu Vàng', 'https://images.vietnamairlines.com/banners/autumn-2025.jpg', 'Đón thu vàng cùng ngập tràn khuyến mãi vé bay khứ hồi giá cực tốt.', DATEADD(month, 3, NOW()), DATEADD(month, 5, NOW()), 0, 0);



-- =====================================================
-- 5. SEED CMS DATA (Categories, Articles)
-- =====================================================

-- Seed Categories
INSERT INTO cms."Categories" (Id, Name, Slug, IsDeleted) VALUES
('CA11E2D4-BCDE-4F01-2345-6789ABCDEF01', 'Tin Tức Khuyến Mãi', 'tin-tuc-khuyen-mai', 0),
('CA22E2D4-BCDE-4F01-2345-6789ABCDEF02', 'Cẩm Nang Du Lịch', 'cam-nang-du-lich', 0);


-- Seed Articles
INSERT INTO cms."Articles" (Id, CategoryId, AuthorId, Title, Slug, Summary, Content, ThumbnailUrl, PublishedAt, Status, ViewCount, IsDeleted, CreatedAt) VALUES
('AE10E2D4-BCDE-4F01-2345-6789ABCDEF01', 'CA22E2D4-BCDE-4F01-2345-6789ABCDEF02', 'D4B0F2A8-9B2F-4A9B-89E3-4E80D77BC901', 'Cẩm nang du lịch Phú Quốc từ A đến Z năm 2025', 'cam-nang-du-lich-phu-quoc-2025', 'Những lưu ý quan trọng khi đi du lịch Phú Quốc tự túc.', 'Bài viết này cung cấp toàn bộ kinh nghiệm bay, đặt khách sạn, ẩm thực và địa điểm vui chơi tại Phú Quốc cho du khách.', 'https://images.phuquoc.vn/thumbnail.jpg', NOW(), 1, 245, 0, NOW());



-- =====================================================
-- 6. SEED INTERACTIONS DATA (Reviews)
-- =====================================================

INSERT INTO interactions."Reviews" (Id, UserId, AirlineId, FlightId, Rating, Comment, IsVerifiedPurchase, IsHidden, IsDeleted, CreatedAt) VALUES
('EE10E2D4-BCDE-4F01-2345-6789ABCDEF01', 'E2E3F4C5-1A2B-3C4D-5E6F-7A8B9C0D1E2F', 'A1F3E2D4-BCDE-4F01-2345-6789ABCDEF01', 'F1F3E2D4-BCDE-4F01-2345-6789ABCDEF01', 5, 'Dịch vụ của Vietnam Airlines rất tốt, bay đúng giờ, tiếp viên thân thiện.', 1, 0, 0, NOW());



-- =====================================================
-- 7. SEED NOTIFICATIONS DATA (Templates)
-- =====================================================

INSERT INTO dbo.NotificationTemplates (Id, Code, Subject, BodyTemplate, Language, CreatedAt) VALUES
('F1F3E2D4-BCDE-4F01-2345-6789ABCDEF01', 'BOOKING_CONFIRMED', 'Xác nhận đặt vé thành công', 'Chào {{PassengerName}}, Đặt chỗ của bạn (Mã: {{PnrCode}}) đã được xác nhận thành công. Chuyến bay: {{FlightNumber}}.', 'vi', NOW()),
('F2F3E2D4-BCDE-4F01-2345-6789ABCDEF02', 'BOOKING_CONFIRMED_EN', 'Booking Confirmation', 'Hello {{PassengerName}}, Your booking (PNR: {{PnrCode}}) has been confirmed. Flight: {{FlightNumber}}.', 'en', NOW());



-- === Merged from seed_role_permissions.sql ===
-- =====================================================
-- SEED ROLE PERMISSIONS
-- Maps roles to their default permissions
-- =====================================================



-- System Admin (RoleId=0): Full access to all permissions
INSERT INTO users."RolePermissions" (RoleId, PermissionId)
SELECT 0, Id FROM users."Permissions";


-- Airline Admin (RoleId=1): Manage flights, airlines, bookings, staff (no system-level permissions)
INSERT INTO users."RolePermissions" (RoleId, PermissionId)
SELECT 1, Id FROM users."Permissions"
WHERE Code IN ('CREATE_FLIGHT', 'SELL_TICKET', 'MANAGE_AIRLINE_STAFF', 'MANAGE_FLIGHTS', 'MANAGE_AIRLINES', 'MANAGE_BOOKINGS');


-- Airline Staff (RoleId=2): Sell tickets, view bookings
INSERT INTO users."RolePermissions" (RoleId, PermissionId)
SELECT 2, Id FROM users."Permissions"
WHERE Code IN ('SELL_TICKET', 'MANAGE_FLIGHTS', 'MANAGE_BOOKINGS');


-- User Client (RoleId=3): View-only permissions (no admin permissions)
INSERT INTO users."RolePermissions" (RoleId, PermissionId)
SELECT 3, Id FROM users."Permissions"
WHERE Code IN ('SELL_TICKET');


-- === Merged from seed_aircraft_models.sql ===


PRINT '==========================================================='
PRINT ' SEED SCRIPT: Aircraft Models and Seat Templates'
PRINT '==========================================================='

SET NOCOUNT ON;

-- 1. Define Aircraft Model IDs
DECLARE @ModelB787 UNIQUEIDENTIFIER = 'B7879000-BCDE-4F01-2345-6789ABCDEF01';
DECLARE @ModelA350 UNIQUEIDENTIFIER = 'A3509000-BCDE-4F01-2345-6789ABCDEF02';
DECLARE @ModelA321 UNIQUEIDENTIFIER = 'A3212000-BCDE-4F01-2345-6789ABCDEF03';

-- 2. Insert Aircraft Models
IF NOT EXISTS (SELECT 1 FROM flights."AircraftModels" WHERE Id = @ModelB787)
BEGIN
    INSERT INTO flights."AircraftModels" (Id, Name, Manufacturer, TotalSeats, IsDeleted)
    VALUES (@ModelB787, 'Boeing 787-9 Dreamliner', 'Boeing', 294, 0);
END

IF NOT EXISTS (SELECT 1 FROM flights."AircraftModels" WHERE Id = @ModelA350)
BEGIN
    INSERT INTO flights."AircraftModels" (Id, Name, Manufacturer, TotalSeats, IsDeleted)
    VALUES (@ModelA350, 'Airbus A350-900', 'Airbus', 305, 0);
END

IF NOT EXISTS (SELECT 1 FROM flights."AircraftModels" WHERE Id = @ModelA321)
BEGIN
    INSERT INTO flights."AircraftModels" (Id, Name, Manufacturer, TotalSeats, IsDeleted)
    VALUES (@ModelA321, 'Airbus A321neo', 'Airbus', 230, 0);
END

-- 3. Generate Seat Templates using loops to avoid massive SQL file size
PRINT 'Generating Seat Templates...'

-- Helper table for columns
IF OBJECT_ID('tempdb..#Cols') IS NOT NULL DROP TABLE #Cols;
CREATE TABLE #Cols (Col CHAR(1), ColIndex INT);
INSERT INTO #Cols VALUES ('A', 1), ('B', 2), ('C', 3), ('D', 4), ('E', 5), ('F', 6), ('G', 7), ('H', 8), ('K', 9);

-- --- Boeing 787-9 Seat Template ---
-- Business Class: Rows 1-5, 1-2-1 layout (A, D, G, K)
DECLARE @Row INT = 1;
WHILE @Row <= 5
BEGIN
    INSERT INTO flights."AircraftModelSeatTemplates" (Id, AircraftModelId, SeatNumber, SeatRow, SeatColumn, SeatClass, IsExtraLegroom, PriceMultiplier)
    SELECT 
        gen_random_uuid(), 
        @ModelB787, 
        CAST(@Row AS VARCHAR(2)) + Col, 
        CAST(@Row AS VARCHAR(2)), 
        Col, 
        2, -- Business
        1, -- Extra legroom for Business
        2.5 -- 2.5x price
    FROM #Cols WHERE Col IN ('A', 'D', 'G', 'K')
    AND NOT EXISTS (SELECT 1 FROM flights."AircraftModelSeatTemplates" WHERE AircraftModelId = @ModelB787 AND SeatNumber = CAST(@Row AS VARCHAR(2)) + Col);
    
    SET @Row = @Row + 1;
END

-- Premium Economy: Rows 10-15, 2-3-2 layout (A, C, D, F, G, H, K)
SET @Row = 10;
WHILE @Row <= 15
BEGIN
    INSERT INTO flights."AircraftModelSeatTemplates" (Id, AircraftModelId, SeatNumber, SeatRow, SeatColumn, SeatClass, IsExtraLegroom, PriceMultiplier)
    SELECT 
        gen_random_uuid(), 
        @ModelB787, 
        CAST(@Row AS VARCHAR(2)) + Col, 
        CAST(@Row AS VARCHAR(2)), 
        Col, 
        1, -- Premium Economy
        0, 
        1.5 -- 1.5x price
    FROM #Cols WHERE Col IN ('A', 'C', 'D', 'F', 'G', 'H', 'K')
    AND NOT EXISTS (SELECT 1 FROM flights."AircraftModelSeatTemplates" WHERE AircraftModelId = @ModelB787 AND SeatNumber = CAST(@Row AS VARCHAR(2)) + Col);
    
    SET @Row = @Row + 1;
END

-- Economy: Rows 20-45, 3-3-3 layout (A, B, C, D, E, F, G, H, K)
SET @Row = 20;
WHILE @Row <= 45
BEGIN
    INSERT INTO flights."AircraftModelSeatTemplates" (Id, AircraftModelId, SeatNumber, SeatRow, SeatColumn, SeatClass, IsExtraLegroom, PriceMultiplier)
    SELECT 
        gen_random_uuid(), 
        @ModelB787, 
        CAST(@Row AS VARCHAR(2)) + Col, 
        CAST(@Row AS VARCHAR(2)), 
        Col, 
        0, -- Economy
        CASE WHEN @Row = 20 THEN 1 ELSE 0 END, -- Row 20 has extra legroom
        CASE WHEN @Row = 20 THEN 1.2 ELSE 1.0 END
    FROM #Cols WHERE Col IN ('A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'K')
    AND NOT EXISTS (SELECT 1 FROM flights."AircraftModelSeatTemplates" WHERE AircraftModelId = @ModelB787 AND SeatNumber = CAST(@Row AS VARCHAR(2)) + Col);
    
    SET @Row = @Row + 1;
END


-- --- Airbus A350-900 Seat Template ---
-- Business Class: Rows 1-6, 1-2-1 layout (A, D, G, K)
SET @Row = 1;
WHILE @Row <= 6
BEGIN
    INSERT INTO flights."AircraftModelSeatTemplates" (Id, AircraftModelId, SeatNumber, SeatRow, SeatColumn, SeatClass, IsExtraLegroom, PriceMultiplier)
    SELECT 
        gen_random_uuid(), 
        @ModelA350, 
        CAST(@Row AS VARCHAR(2)) + Col, 
        CAST(@Row AS VARCHAR(2)), 
        Col, 
        2, -- Business
        1, 
        2.5
    FROM #Cols WHERE Col IN ('A', 'D', 'G', 'K')
    AND NOT EXISTS (SELECT 1 FROM flights."AircraftModelSeatTemplates" WHERE AircraftModelId = @ModelA350 AND SeatNumber = CAST(@Row AS VARCHAR(2)) + Col);
    
    SET @Row = @Row + 1;
END

-- Premium Economy: Rows 10-16, 2-4-2 layout (A, C, D, E, F, G, H, K)
SET @Row = 10;
WHILE @Row <= 16
BEGIN
    INSERT INTO flights."AircraftModelSeatTemplates" (Id, AircraftModelId, SeatNumber, SeatRow, SeatColumn, SeatClass, IsExtraLegroom, PriceMultiplier)
    SELECT 
        gen_random_uuid(), 
        @ModelA350, 
        CAST(@Row AS VARCHAR(2)) + Col, 
        CAST(@Row AS VARCHAR(2)), 
        Col, 
        1, -- Premium Economy
        0, 
        1.5
    FROM #Cols WHERE Col IN ('A', 'C', 'D', 'E', 'F', 'G', 'H', 'K')
    AND NOT EXISTS (SELECT 1 FROM flights."AircraftModelSeatTemplates" WHERE AircraftModelId = @ModelA350 AND SeatNumber = CAST(@Row AS VARCHAR(2)) + Col);
    
    SET @Row = @Row + 1;
END

-- Economy: Rows 20-46, 3-3-3 layout (A, B, C, D, E, F, G, H, K)
SET @Row = 20;
WHILE @Row <= 46
BEGIN
    INSERT INTO flights."AircraftModelSeatTemplates" (Id, AircraftModelId, SeatNumber, SeatRow, SeatColumn, SeatClass, IsExtraLegroom, PriceMultiplier)
    SELECT 
        gen_random_uuid(), 
        @ModelA350, 
        CAST(@Row AS VARCHAR(2)) + Col, 
        CAST(@Row AS VARCHAR(2)), 
        Col, 
        0, -- Economy
        CASE WHEN @Row = 20 THEN 1 ELSE 0 END, 
        CASE WHEN @Row = 20 THEN 1.2 ELSE 1.0 END
    FROM #Cols WHERE Col IN ('A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'K')
    AND NOT EXISTS (SELECT 1 FROM flights."AircraftModelSeatTemplates" WHERE AircraftModelId = @ModelA350 AND SeatNumber = CAST(@Row AS VARCHAR(2)) + Col);
    
    SET @Row = @Row + 1;
END


-- --- Airbus A321neo Seat Template ---
-- Business Class: Rows 1-2, 2-2 layout (A, C, H, K)
SET @Row = 1;
WHILE @Row <= 2
BEGIN
    INSERT INTO flights."AircraftModelSeatTemplates" (Id, AircraftModelId, SeatNumber, SeatRow, SeatColumn, SeatClass, IsExtraLegroom, PriceMultiplier)
    SELECT 
        gen_random_uuid(), 
        @ModelA321, 
        CAST(@Row AS VARCHAR(2)) + Col, 
        CAST(@Row AS VARCHAR(2)), 
        Col, 
        2, -- Business
        1, 
        2.0
    FROM #Cols WHERE Col IN ('A', 'C', 'H', 'K')
    AND NOT EXISTS (SELECT 1 FROM flights."AircraftModelSeatTemplates" WHERE AircraftModelId = @ModelA321 AND SeatNumber = CAST(@Row AS VARCHAR(2)) + Col);
    
    SET @Row = @Row + 1;
END

-- Economy: Rows 3-38, 3-3 layout (A, B, C, H, J, K)
SET @Row = 3;
WHILE @Row <= 38
BEGIN
    INSERT INTO flights."AircraftModelSeatTemplates" (Id, AircraftModelId, SeatNumber, SeatRow, SeatColumn, SeatClass, IsExtraLegroom, PriceMultiplier)
    SELECT 
        gen_random_uuid(), 
        @ModelA321, 
        CAST(@Row AS VARCHAR(2)) + Col, 
        CAST(@Row AS VARCHAR(2)), 
        Col, 
        0, -- Economy
        CASE WHEN @Row IN (11, 12) THEN 1 ELSE 0 END, -- Exit rows
        CASE WHEN @Row IN (11, 12) THEN 1.2 ELSE 1.0 END
    FROM #Cols WHERE Col IN ('A', 'B', 'C', 'G', 'H', 'K') -- Map G to J for simplicity
    AND NOT EXISTS (SELECT 1 FROM flights."AircraftModelSeatTemplates" WHERE AircraftModelId = @ModelA321 AND SeatNumber = CAST(@Row AS VARCHAR(2)) + Col);
    
    SET @Row = @Row + 1;
END

-- 4. Update existing Airplanes to link to Aircraft Models
PRINT 'Linking existing Airplanes to Aircraft Models...'

UPDATE flights."Airplanes"
SET AircraftModelId = @ModelB787
WHERE Id IN ('81F3E2D4-BCDE-4F01-2345-6789ABCDEF01', '84F3E2D4-BCDE-4F01-2345-6789ABCDEF04');

UPDATE flights."Airplanes"
SET AircraftModelId = @ModelA350
WHERE Id = '82F3E2D4-BCDE-4F01-2345-6789ABCDEF02';

UPDATE flights."Airplanes"
SET AircraftModelId = @ModelA321
WHERE Id = '83F3E2D4-BCDE-4F01-2345-6789ABCDEF03';

-- 5. Generate AirplaneSeats for existing Airplanes from templates
PRINT 'Generating AirplaneSeats for existing Airplanes...'

-- Clear existing seats to avoid duplicates
DELETE FROM flights."AirplaneSeats";

-- Insert seats for each airplane based on its model's template
INSERT INTO flights."AirplaneSeats" (Id, AirplaneId, SeatNumber, SeatRow, SeatColumn, SeatClass, IsExtraLegroom, PriceMultiplier)
SELECT 
    gen_random_uuid(),
    a.Id,
    t.SeatNumber,
    t.SeatRow,
    t.SeatColumn,
    t.SeatClass,
    t.IsExtraLegroom,
    t.PriceMultiplier
FROM flights."Airplanes" a
JOIN flights."AircraftModelSeatTemplates" t ON a.AircraftModelId = t.AircraftModelId;

PRINT 'Aircraft Models and Seat Templates seeding completed successfully!'
PRINT '==========================================================='


-- === Merged from seed_routes_flights.sql ===


IF DB_ID('AirlineTicketDb') IS NOT NULL
BEGIN
    END


PRINT '==========================================================='
PRINT ' SEED SCRIPT: Routes and Flights'
PRINT '==========================================================='

SET NOCOUNT ON;

-- Variables
DECLARE @HubSGN UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM flights."Airports" WHERE IataCode = 'SGN');
DECLARE @HubHAN UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM flights."Airports" WHERE IataCode = 'HAN');

IF @HubSGN IS NULL OR @HubHAN IS NULL
BEGIN
    PRINT 'Error: SGN or HAN airports not found. Please run base seed first.';
    RETURN;
END

-- Create a temporary table to store the routes we want to insert
IF OBJECT_ID('tempdb..#RoutesToInsert') IS NOT NULL DROP TABLE #RoutesToInsert;
CREATE TABLE #RoutesToInsert (
    OriginAirportId UNIQUEIDENTIFIER,
    DestinationAirportId UNIQUEIDENTIFIER
);

-- Generate Routes to/from SGN for all airports (except SGN itself)
INSERT INTO #RoutesToInsert (OriginAirportId, DestinationAirportId)
SELECT @HubSGN, Id FROM flights."Airports" WHERE Id <> @HubSGN;

INSERT INTO #RoutesToInsert (OriginAirportId, DestinationAirportId)
SELECT Id, @HubSGN FROM flights."Airports" WHERE Id <> @HubSGN;

-- Generate Routes to/from HAN for all airports (except HAN itself)
INSERT INTO #RoutesToInsert (OriginAirportId, DestinationAirportId)
SELECT @HubHAN, Id FROM flights."Airports" WHERE Id <> @HubHAN;

INSERT INTO #RoutesToInsert (OriginAirportId, DestinationAirportId)
SELECT Id, @HubHAN FROM flights."Airports" WHERE Id <> @HubHAN;

PRINT 'Generating Routes...'

-- Insert routes that don't exist yet
INSERT INTO flights."Routes" (
    Id, AirlineId, OriginAirportId, DestinationAirportId, DistanceKm, EstimatedDurationMinutes, IsDeleted
)
SELECT 
    gen_random_uuid(),
    (SELECT TOP 1 Id FROM flights."Airlines" ORDER BY gen_random_uuid()), -- Random Airline
    r.OriginAirportId,
    r.DestinationAirportId,
    ABS(CHECKSUM(gen_random_uuid())) % 2000 + 500 AS DistanceKm, -- Random Distance between 500 and 2500
    ABS(CHECKSUM(gen_random_uuid())) % 180 + 60 AS EstimatedDurationMinutes, -- Random Duration between 60 and 240 mins
    0
FROM #RoutesToInsert r
WHERE NOT EXISTS (
    SELECT 1 FROM flights."Routes" existing 
    WHERE existing.OriginAirportId = r.OriginAirportId 
    AND existing.DestinationAirportId = r.DestinationAirportId
);

PRINT 'Routes generation completed.'
PRINT 'Generating Flights...'

-- Create Flights
DECLARE @RouteId UNIQUEIDENTIFIER;
DECLARE @AirlineId UNIQUEIDENTIFIER;
DECLARE @OriginIata NVARCHAR(10);
DECLARE @EstimatedDurationMinutes INT;
DECLARE @FlightCount INT;
DECLARE @I INT;

DECLARE route_cursor CURSOR FOR 
SELECT 
    r.Id, 
    r.AirlineId, 
    a.IataCode,
    ISNULL(r.EstimatedDurationMinutes, 180)
FROM flights."Routes" r
JOIN flights."Airports" a ON r.OriginAirportId = a.Id;

OPEN route_cursor;
FETCH NEXT FROM route_cursor INTO @RouteId, @AirlineId, @OriginIata, @EstimatedDurationMinutes;

WHILE @@FETCH_STATUS = 0
BEGIN
    -- Check how many flights already exist for this route
    SELECT @FlightCount = COUNT(*) FROM flights."Flights" WHERE RouteId = @RouteId AND IsDeleted = 0;
    
    SET @I = @FlightCount;
    WHILE @I < 5
    BEGIN
        DECLARE @NewFlightId UNIQUEIDENTIFIER = gen_random_uuid();
        DECLARE @AirplaneId UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM flights."Airplanes" WHERE AirlineId = @AirlineId ORDER BY gen_random_uuid());
        
        -- Fallback if airline has no airplane, just pick any
        IF @AirplaneId IS NULL
        BEGIN
            SET @AirplaneId = (SELECT TOP 1 Id FROM flights."Airplanes" ORDER BY gen_random_uuid());
        END

        IF @AirplaneId IS NOT NULL
        BEGIN
            -- Generate flight departure time randomly within next 30 days
            DECLARE @DaysToAdd INT = ABS(CHECKSUM(gen_random_uuid())) % 30;
            DECLARE @HoursToAdd INT = ABS(CHECKSUM(gen_random_uuid())) % 24;
            DECLARE @MinutesToAdd INT = (ABS(CHECKSUM(gen_random_uuid())) % 12) * 5; -- Multiple of 5
            
            DECLARE @DepartureTime DATETIME2 = DATEADD(MINUTE, @MinutesToAdd, DATEADD(HOUR, @HoursToAdd, DATEADD(DAY, @DaysToAdd, NOW())));
            DECLARE @ArrivalTime DATETIME2 = DATEADD(MINUTE, @EstimatedDurationMinutes, @DepartureTime);
            DECLARE @BasePrice DECIMAL(18,2) = CAST((ABS(CHECKSUM(gen_random_uuid())) % 751 + 50) AS DECIMAL(18,2)); -- Random between 50 and 800
            
            -- Insert Flight
            INSERT INTO flights."Flights" (
                Id, RouteId, AirplaneId, FlightNumber, DepartureTime, ArrivalTime, BasePrice, Currency, Status, ExternalId, IsDeleted, CreatedAt, UpdatedAt
            )
            VALUES (
                @NewFlightId,
                @RouteId,
                @AirplaneId,
                'FL' + @OriginIata + CAST((@I + 1) AS NVARCHAR(10)),
                @DepartureTime,
                @ArrivalTime,
                @BasePrice,
                'USD',
                0, -- Scheduled
                gen_random_uuid(),
                0,
                NOW(),
                NOW()
            );

            -- Insert 10 FlightSeats for this flight
            DECLARE @SeatIndex INT = 1;
            WHILE @SeatIndex <= 10
            BEGIN
                INSERT INTO flights."FlightSeats" (
                    Id, FlightId, SeatNumber, SeatClass, PriceOverride, IsAvailable, IsExtraLegroom
                )
                VALUES (
                    gen_random_uuid(),
                    @NewFlightId,
                    CAST(@SeatIndex AS NVARCHAR(5)) + CASE WHEN @SeatIndex % 2 = 0 THEN 'A' ELSE 'B' END,
                    CASE WHEN @SeatIndex <= 2 THEN 2 ELSE 0 END, -- First 2 are Business(2), rest Economy(0)
                    CASE WHEN @SeatIndex <= 2 THEN @BasePrice * 2 ELSE NULL END,
                    1,
                    CASE WHEN @SeatIndex <= 4 THEN 1 ELSE 0 END
                );
                SET @SeatIndex = @SeatIndex + 1;
            END
        END
        
        SET @I = @I + 1;
    END

    FETCH NEXT FROM route_cursor INTO @RouteId, @AirlineId, @OriginIata, @EstimatedDurationMinutes;
END

CLOSE route_cursor;
DEALLOCATE route_cursor;

PRINT 'Flights and FlightSeats generation completed.'
PRINT '==========================================================='


-- === Merged from seed_notification_templates.sql ===
-- =====================================================
-- Seed Notification Templates (i18n)
-- Locales: vi, en, zh, ja, ko, fr
-- =====================================================



-- 1. English (en)
IF NOT EXISTS (SELECT 1 FROM notifications."NotificationTemplates" WHERE Code = 'FLIGHT_CREATED' AND Language = 'en')
BEGIN
    INSERT INTO notifications."NotificationTemplates" (Id, Code, Subject, BodyTemplate, Language, CreatedAt)
    VALUES (gen_random_uuid(), 'FLIGHT_CREATED', 'New Flight Created: {{FlightNumber}}', 'A new flight {{FlightNumber}} from {{Origin}} to {{Destination}} has been created by a partner.', 'en', NOW())
END


-- 2. Vietnamese (vi)
IF NOT EXISTS (SELECT 1 FROM notifications."NotificationTemplates" WHERE Code = 'FLIGHT_CREATED' AND Language = 'vi')
BEGIN
    INSERT INTO notifications."NotificationTemplates" (Id, Code, Subject, BodyTemplate, Language, CreatedAt)
    VALUES (gen_random_uuid(), 'FLIGHT_CREATED', 'Chuyến bay mới được tạo: {{FlightNumber}}', 'Chuyến bay mới {{FlightNumber}} từ {{Origin}} đến {{Destination}} vừa được tạo bởi đối tác.', 'vi', NOW())
END


-- 3. Chinese (zh)
IF NOT EXISTS (SELECT 1 FROM notifications."NotificationTemplates" WHERE Code = 'FLIGHT_CREATED' AND Language = 'zh')
BEGIN
    INSERT INTO notifications."NotificationTemplates" (Id, Code, Subject, BodyTemplate, Language, CreatedAt)
    VALUES (gen_random_uuid(), 'FLIGHT_CREATED', '新航班已创建: {{FlightNumber}}', '合作伙伴已创建从 {{Origin}} 到 {{Destination}} 的新航班 {{FlightNumber}}。', 'zh', NOW())
END


-- 4. Japanese (ja)
IF NOT EXISTS (SELECT 1 FROM notifications."NotificationTemplates" WHERE Code = 'FLIGHT_CREATED' AND Language = 'ja')
BEGIN
    INSERT INTO notifications."NotificationTemplates" (Id, Code, Subject, BodyTemplate, Language, CreatedAt)
    VALUES (gen_random_uuid(), 'FLIGHT_CREATED', '新しいフライトが作成されました: {{FlightNumber}}', 'パートナーによって {{Origin}} から {{Destination}} への新しいフライト {{FlightNumber}} が作成されました。', 'ja', NOW())
END


-- 5. Korean (ko)
IF NOT EXISTS (SELECT 1 FROM notifications."NotificationTemplates" WHERE Code = 'FLIGHT_CREATED' AND Language = 'ko')
BEGIN
    INSERT INTO notifications."NotificationTemplates" (Id, Code, Subject, BodyTemplate, Language, CreatedAt)
    VALUES (gen_random_uuid(), 'FLIGHT_CREATED', '새 항공편 생성됨: {{FlightNumber}}', '파트너가 {{Origin}}에서 {{Destination}}으로 가는 새 항공편 {{FlightNumber}}을(를) 생성했습니다.', 'ko', NOW())
END


-- 6. French (fr)
IF NOT EXISTS (SELECT 1 FROM notifications."NotificationTemplates" WHERE Code = 'FLIGHT_CREATED' AND Language = 'fr')
BEGIN
    INSERT INTO notifications."NotificationTemplates" (Id, Code, Subject, BodyTemplate, Language, CreatedAt)
    VALUES (gen_random_uuid(), 'FLIGHT_CREATED', 'Nouveau vol créé : {{FlightNumber}}', 'Un nouveau vol {{FlightNumber}} de {{Origin}} à {{Destination}} a été créé par un partenaire.', 'fr', NOW())
END


-- === Merged from seed_extra_data.sql ===

-- =====================================================
-- AIRLINE TICKET SYSTEM - EXTRA SEED DATA
-- =====================================================



-- 1. Generate 100 new Flights
-- Requires Routes first. Since routes might not exist for all combinations, 
-- I will create some routes or use existing ones if possible, but for realism, 
-- creating routes for the combinations is safer.
-- Wait, the task asks to generate flights distributed across airlines and airports.
-- To simplify, I will create a set of routes first, then flights.

-- For simplicity in the script, I'll use a loop or just explicit statements.
-- Since I need to generate 100, I'll use a script to do it.

DECLARE @Count INT = 0;
DECLARE @AirlineId UNIQUEIDENTIFIER;
DECLARE @RouteId UNIQUEIDENTIFIER;
DECLARE @AirplaneId UNIQUEIDENTIFIER;
DECLARE @OriginAirportId UNIQUEIDENTIFIER;
DECLARE @DestinationAirportId UNIQUEIDENTIFIER;
DECLARE @FlightNumber NVARCHAR(20);
DECLARE @DepartureTime DATETIME2;
DECLARE @ArrivalTime DATETIME2;

-- Helper to pick random values would be great, but T-SQL random is tricky.
-- I'll hardcode some and use gen_random_uuid() for variations.

WHILE @Count < 100
BEGIN
    SET @Count = @Count + 1;
    
    -- Pick a random airline (simplified, I'll pick one of the 8)
    -- This is a bit complex for a simple script, I will just hardcode combinations
    
    -- Let's pick a random route and airplane
    SELECT TOP 1 @RouteId = Id, @AirlineId = AirlineId, @OriginAirportId = OriginAirportId, @DestinationAirportId = DestinationAirportId FROM flights."Routes" ORDER BY gen_random_uuid();
    SELECT TOP 1 @AirplaneId = Id FROM flights."Airplanes" WHERE AirlineId = @AirlineId ORDER BY gen_random_uuid();
    
    -- If no airplane for this airline, pick any
    IF @AirplaneId IS NULL
        SELECT TOP 1 @AirplaneId = Id FROM flights."Airplanes" ORDER BY gen_random_uuid();

    SET @FlightNumber = 'FL' + CAST(1000 + @Count AS NVARCHAR(10));
    SET @DepartureTime = DATEADD(hour, @Count, NOW());
    SET @ArrivalTime = DATEADD(hour, 3, @DepartureTime);

    DECLARE @FlightId UNIQUEIDENTIFIER = gen_random_uuid();

    INSERT INTO flights."Flights" (Id, RouteId, AirplaneId, FlightNumber, DepartureTime, ArrivalTime, BasePrice, Currency, Status, IsDeleted, CreatedAt, UpdatedAt)
    VALUES (@FlightId, @RouteId, @AirplaneId, @FlightNumber, @DepartureTime, @ArrivalTime, 100.00, 'USD', 0, 0, NOW(), NOW());

    -- 2. Generate 10-20 seats for this flight
    DECLARE @SeatCount INT = 0;
    DECLARE @TotalSeats INT = 10 + (ABS(CHECKSUM(gen_random_uuid())) % 11);
    
    WHILE @SeatCount < @TotalSeats
    BEGIN
        SET @SeatCount = @SeatCount + 1;
        INSERT INTO flights."FlightSeats" (Id, FlightId, SeatNumber, SeatClass, PriceOverride, IsAvailable, IsExtraLegroom)
        VALUES (gen_random_uuid(), @FlightId, CAST(@SeatCount AS NVARCHAR(5)) + 'A', 0, NULL, 1, 0);
    END
END


-- 3. 50 new Bookings with corresponding Passengers, Tickets, and Payments
DECLARE @BookingCount INT = 0;
DECLARE @UserId UNIQUEIDENTIFIER;
DECLARE @FlightId UNIQUEIDENTIFIER;
DECLARE @SeatId UNIQUEIDENTIFIER;
DECLARE @BookingId UNIQUEIDENTIFIER;
DECLARE @PassengerId UNIQUEIDENTIFIER;

SELECT TOP 1 @UserId = Id FROM users."Users" WHERE Email = 'client@gmail.com';

WHILE @BookingCount < 50
BEGIN
    SET @BookingCount = @BookingCount + 1;
    
    SELECT TOP 1 @FlightId = Id FROM flights."Flights" ORDER BY gen_random_uuid();
    SELECT TOP 1 @SeatId = Id FROM flights."FlightSeats" WHERE FlightId = @FlightId AND IsAvailable = 1 ORDER BY gen_random_uuid();
    
    IF @SeatId IS NOT NULL
    BEGIN
        SET @BookingId = gen_random_uuid();
        SET @PassengerId = gen_random_uuid();
        
        INSERT INTO bookings."Bookings" (Id, UserId, PnrCode, TotalPrice, Currency, Status, ContactEmail, ContactPhone, SpecialRequests, IsDeleted, CreatedAt, UpdatedAt)
        VALUES (@BookingId, @UserId, 'PNR' + CAST(1000 + @BookingCount AS NVARCHAR(10)), 100.00, 'USD', 1, 'client@gmail.com', '0987654321', NULL, 0, NOW(), NOW());
        
        INSERT INTO bookings."Passengers" (Id, BookingId, FirstName, LastName, Gender, DateOfBirth, Nationality, PassportNumber, PassportExpiryDate)
        VALUES (@PassengerId, @BookingId, 'Passenger', CAST(@BookingCount AS NVARCHAR(10)), 0, '1990-01-01', 'VN', 'PS' + CAST(@BookingCount AS NVARCHAR(10)), '2030-01-01');
        
        INSERT INTO bookings."Tickets" (Id, BookingId, PassengerId, FlightId, SeatId, TicketNumber, Gate, BoardingTime, Status)
        VALUES (gen_random_uuid(), @BookingId, @PassengerId, @FlightId, @SeatId, 'TK' + CAST(1000 + @BookingCount AS NVARCHAR(10)), 'Gate A', DATEADD(hour, -1, NOW()), 0);
        
        INSERT INTO bookings."Payments" (Id, BookingId, TransactionId, Amount, PaymentMethod, ProviderStatus, IsSuccessful, RawResponse, CreatedAt)
        VALUES (gen_random_uuid(), @BookingId, 'TXN' + CAST(1000 + @BookingCount AS NVARCHAR(10)), 100.00, 'CreditCard', 'Success', 1, NULL, NOW());
        
        UPDATE flights."FlightSeats" SET IsAvailable = 0 WHERE Id = @SeatId;
    END
END

