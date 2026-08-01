-- =====================================================
-- CORE: Airports (Reference data)
-- Schema: flights
-- =====================================================

INSERT INTO flights.airports (id, iata_code, name_en, name_vi, city_en, city_vi, country_code, timezone, latitude, longitude, is_active, is_deleted) VALUES
-- Vietnam
('E5F3E2D4-BCDE-4F01-2345-6789ABCDEF05', 'HAN', 'Noi Bai International Airport', 'Sân bay quốc tế Nội Bài', 'Hanoi', 'Hà Nội', 'VN', 'Asia/Ho_Chi_Minh', 21.2212, 105.8072, TRUE, FALSE),
('F6F3E2D4-BCDE-4F01-2345-6789ABCDEF06', 'SGN', 'Tan Son Nhat International Airport', 'Sân bay quốc tế Tân Sơn Nhất', 'Ho Chi Minh City', 'TP. Hồ Chí Minh', 'VN', 'Asia/Ho_Chi_Minh', 10.8188, 106.6519, TRUE, FALSE),
('07F3E2D4-BCDE-4F01-2345-6789ABCDEF07', 'DAD', 'Da Nang International Airport', 'Sân bay quốc tế Đà Nẵng', 'Da Nang', 'Đà Nẵng', 'VN', 'Asia/Ho_Chi_Minh', 16.0439, 108.1994, TRUE, FALSE),
-- Japan
('08F3E2D4-BCDE-4F01-2345-6789ABCDEF08', 'NRT', 'Narita International Airport', 'Sân bay quốc tế Narita', 'Tokyo', 'Tokyo', 'JP', 'Asia/Tokyo', 35.7647, 140.3863, TRUE, FALSE),
('09F3E2D4-BCDE-4F01-2345-6789ABCDEF09', 'HND', 'Haneda Airport', 'Sân bay Haneda', 'Tokyo', 'Tokyo', 'JP', 'Asia/Tokyo', 35.5494, 139.7798, TRUE, FALSE),
('10F3E2D4-BCDE-4F01-2345-6789ABCDEF10', 'KIX', 'Kansai International Airport', 'Sân bay quốc tế Kansai', 'Osaka', 'Osaka', 'JP', 'Asia/Tokyo', 34.4320, 135.2304, TRUE, FALSE),
-- South Korea
('11F3E2D4-BCDE-4F01-2345-6789ABCDEF11', 'ICN', 'Incheon International Airport', 'Sân bay quốc tế Incheon', 'Seoul', 'Seoul', 'KR', 'Asia/Seoul', 37.4602, 126.4407, TRUE, FALSE),
('12F3E2D4-BCDE-4F01-2345-6789ABCDEF12', 'PUS', 'Gimhae International Airport', 'Sân bay quốc tế Gimhae', 'Busan', 'Busan', 'KR', 'Asia/Seoul', 35.1795, 128.9382, TRUE, FALSE),
-- China
('13F3E2D4-BCDE-4F01-2345-6789ABCDEF13', 'PEK', 'Beijing Capital International Airport', 'Sân bay quốc tế Thủ đô Bắc Kinh', 'Beijing', 'Bắc Kinh', 'CN', 'Asia/Shanghai', 40.0799, 116.6031, TRUE, FALSE),
('14F3E2D4-BCDE-4F01-2345-6789ABCDEF14', 'PVG', 'Shanghai Pudong International Airport', 'Sân bay quốc tế Phố Đông Thượng Hải', 'Shanghai', 'Thượng Hải', 'CN', 'Asia/Shanghai', 31.1443, 121.8083, TRUE, FALSE),
('15F3E2D4-BCDE-4F01-2345-6789ABCDEF15', 'CAN', 'Guangzhou Baiyun International Airport', 'Sân bay quốc tế Bạch Vân Quảng Châu', 'Guangzhou', 'Quảng Châu', 'CN', 'Asia/Shanghai', 23.3924, 113.2988, TRUE, FALSE),
-- Hong Kong & Taiwan
('16F3E2D4-BCDE-4F01-2345-6789ABCDEF16', 'HKG', 'Hong Kong International Airport', 'Sân bay quốc tế Hồng Kông', 'Hong Kong', 'Hồng Kông', 'HK', 'Asia/Hong_Kong', 22.3080, 113.9185, TRUE, FALSE),
('17F3E2D4-BCDE-4F01-2345-6789ABCDEF17', 'TPE', 'Taoyuan International Airport', 'Sân bay quốc tế Đào Viên', 'Taipei', 'Đài Bắc', 'TW', 'Asia/Taipei', 25.0797, 121.2342, TRUE, FALSE),
-- Thailand
('18F3E2D4-BCDE-4F01-2345-6789ABCDEF18', 'BKK', 'Suvarnabhumi Airport', 'Sân bay Suvarnabhumi', 'Bangkok', 'Bangkok', 'TH', 'Asia/Bangkok', 13.6900, 100.7501, TRUE, FALSE),
('19F3E2D4-BCDE-4F01-2345-6789ABCDEF19', 'DMK', 'Don Mueang International Airport', 'Sân bay quốc tế Don Mueang', 'Bangkok', 'Bangkok', 'TH', 'Asia/Bangkok', 13.9126, 100.6068, TRUE, FALSE),
('20F3E2D4-BCDE-4F01-2345-6789ABCDEF20', 'HKT', 'Phuket International Airport', 'Sân bay quốc tế Phuket', 'Phuket', 'Phuket', 'TH', 'Asia/Bangkok', 8.1111, 98.3065, TRUE, FALSE),
-- Southeast Asia
('21F3E2D4-BCDE-4F01-2345-6789ABCDEF21', 'SIN', 'Changi Airport', 'Sân bay Changi', 'Singapore', 'Singapore', 'SG', 'Asia/Singapore', 1.3644, 103.9915, TRUE, FALSE),
('22F3E2D4-BCDE-4F01-2345-6789ABCDEF22', 'KUL', 'Kuala Lumpur International Airport', 'Sân bay quốc tế Kuala Lumpur', 'Kuala Lumpur', 'Kuala Lumpur', 'MY', 'Asia/Kuala_Lumpur', 2.7456, 101.7099, TRUE, FALSE),
('23F3E2D4-BCDE-4F01-2345-6789ABCDEF23', 'CGK', 'Soekarno-Hatta International Airport', 'Sân bay quốc tế Soekarno-Hatta', 'Jakarta', 'Jakarta', 'ID', 'Asia/Jakarta', -6.1256, 106.6558, TRUE, FALSE),
('24F3E2D4-BCDE-4F01-2345-6789ABCDEF24', 'DPS', 'Ngurah Rai International Airport', 'Sân bay quốc tế Ngurah Rai', 'Bali', 'Bali', 'ID', 'Asia/Makassar', -8.7482, 115.1675, TRUE, FALSE),
('25F3E2D4-BCDE-4F01-2345-6789ABCDEF25', 'MNL', 'Ninoy Aquino International Airport', 'Sân bay quốc tế Ninoy Aquino', 'Manila', 'Manila', 'PH', 'Asia/Manila', 14.5086, 121.0194, TRUE, FALSE),
-- Australia
('26F3E2D4-BCDE-4F01-2345-6789ABCDEF26', 'SYD', 'Sydney Airport', 'Sân bay Sydney', 'Sydney', 'Sydney', 'AU', 'Australia/Sydney', -33.9399, 151.1753, TRUE, FALSE),
('27F3E2D4-BCDE-4F01-2345-6789ABCDEF27', 'MEL', 'Melbourne Airport', 'Sân bay Melbourne', 'Melbourne', 'Melbourne', 'AU', 'Australia/Melbourne', -37.6690, 144.8410, TRUE, FALSE),
-- Europe
('28F3E2D4-BCDE-4F01-2345-6789ABCDEF28', 'LHR', 'Heathrow Airport', 'Sân bay Heathrow', 'London', 'Luân Đôn', 'GB', 'Europe/London', 51.4700, -0.4543, TRUE, FALSE),
('29F3E2D4-BCDE-4F01-2345-6789ABCDEF29', 'CDG', 'Charles de Gaulle Airport', 'Sân bay Charles de Gaulle', 'Paris', 'Paris', 'FR', 'Europe/Paris', 49.0097, 2.5479, TRUE, FALSE),
('30F3E2D4-BCDE-4F01-2345-6789ABCDEF30', 'FRA', 'Frankfurt Airport', 'Sân bay Frankfurt', 'Frankfurt', 'Frankfurt', 'DE', 'Europe/Berlin', 50.0333, 8.5706, TRUE, FALSE),
('31F3E2D4-BCDE-4F01-2345-6789ABCDEF31', 'AMS', 'Amsterdam Airport Schiphol', 'Sân bay Schiphol', 'Amsterdam', 'Amsterdam', 'NL', 'Europe/Amsterdam', 52.3105, 4.7683, TRUE, FALSE),
-- Middle East
('32F3E2D4-BCDE-4F01-2345-6789ABCDEF32', 'DXB', 'Dubai International Airport', 'Sân bay quốc tế Dubai', 'Dubai', 'Dubai', 'AE', 'Asia/Dubai', 25.2532, 55.3657, TRUE, FALSE),
('33F3E2D4-BCDE-4F01-2345-6789ABCDEF33', 'DOH', 'Hamad International Airport', 'Sân bay quốc tế Hamad', 'Doha', 'Doha', 'QA', 'Asia/Qatar', 25.2731, 51.6080, TRUE, FALSE),
-- North America
('34F3E2D4-BCDE-4F01-2345-6789ABCDEF34', 'JFK', 'John F. Kennedy International Airport', 'Sân bay quốc tế John F. Kennedy', 'New York', 'New York', 'US', 'America/New_York', 40.6413, -73.7781, TRUE, FALSE),
('35F3E2D4-BCDE-4F01-2345-6789ABCDEF35', 'LAX', 'Los Angeles International Airport', 'Sân bay quốc tế Los Angeles', 'Los Angeles', 'Los Angeles', 'US', 'America/Los_Angeles', 33.9416, -118.4085, TRUE, FALSE),
('36F3E2D4-BCDE-4F01-2345-6789ABCDEF36', 'SFO', 'San Francisco International Airport', 'Sân bay quốc tế San Francisco', 'San Francisco', 'San Francisco', 'US', 'America/Los_Angeles', 37.6213, -122.3790, TRUE, FALSE),
('37F3E2D4-BCDE-4F01-2345-6789ABCDEF37', 'YVR', 'Vancouver International Airport', 'Sân bay quốc tế Vancouver', 'Vancouver', 'Vancouver', 'CA', 'America/Vancouver', 49.1967, -123.1815, TRUE, FALSE)
ON CONFLICT (id) DO NOTHING;
