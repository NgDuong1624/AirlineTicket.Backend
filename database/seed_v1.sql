-- =====================================================
-- AIRLINE TICKET SYSTEM - SEED DATA (v2)
-- Designed by GitHub Copilot
-- =====================================================

USE [AirlineTicketDb]
GO

-- =====================================================
-- 1. SEED IDENTITY DATA (Roles, Permissions, Users)
-- =====================================================

-- Khởi tạo danh sách Roles cố định với mã dịch i18n
SET IDENTITY_INSERT [dbo].[Roles] ON;
INSERT INTO [dbo].[Roles] ([Id], [Name], [Description]) VALUES
(0, N'role.system_admin.name', N'role.system_admin.desc'),
(1, N'role.airline_admin.name', N'role.airline_admin.desc'),
(2, N'role.airline_staff.name', N'role.airline_staff.desc'),
(3, N'role.user_client.name', N'role.user_client.desc');
SET IDENTITY_INSERT [dbo].[Roles] OFF;
GO

-- Khởi tạo một số Permissions mẫu với mã dịch i18n
INSERT INTO [dbo].[Permissions] ([Code], [Name], [Description]) VALUES
('CREATE_FLIGHT', N'permission.create_flight.name', N'permission.create_flight.desc'),
('SELL_TICKET', N'permission.sell_ticket.name', N'permission.sell_ticket.desc'),
('MANAGE_AIRLINE_STAFF', N'permission.manage_airline_staff.name', N'permission.manage_airline_staff.desc'),
('MANAGE_USERS', N'permission.manage_users.name', N'permission.manage_users.desc'),
('MANAGE_FLIGHTS', N'permission.manage_flights.name', N'permission.manage_flights.desc'),
('MANAGE_AIRLINES', N'permission.manage_airlines.name', N'permission.manage_airlines.desc'),
('MANAGE_AIRPORTS', N'permission.manage_airports.name', N'permission.manage_airports.desc'),
('MANAGE_BOOKINGS', N'permission.manage_bookings.name', N'permission.manage_bookings.desc'),
('MANAGE_ARTICLES', N'permission.manage_articles.name', N'permission.manage_articles.desc');
GO

-- Khởi tạo danh sách Users mẫu (Admin và Client)
-- Mật khẩu mặc định có mã băm tương ứng
INSERT INTO [dbo].[Users] ([Id], [Email], [EmailConfirmed], [PasswordHash], [FullName], [Phone], [IsActive], [IsDeleted], [CreatedAt], [UpdatedAt], [Role]) VALUES
('D4B0F2A8-9B2F-4A9B-89E3-4E80D77BC901', 'admin@airlineticket.com', 1, '$2b$12$d7mwUGX/x5jcRrxdNAFqJekces0IQ2f9F4mC24iVGZUpwM2pF1Qly', N'Hệ Thống Admin', '0123456789', 1, 0, GETUTCDATE(), GETUTCDATE(), 0),
('E2E3F4C5-1A2B-3C4D-5E6F-7A8B9C0D1E2F', 'client@gmail.com', 1, '$2b$12$d7mwUGX/x5jcRrxdNAFqJekces0IQ2f9F4mC24iVGZUpwM2pF1Qly', N'Nguyễn Văn A', '0987654321', 1, 0, GETUTCDATE(), GETUTCDATE(), 2),
('A1F3E2D4-BCDE-4F01-2345-6789ABCDEF91', 'admin.vna@airlineticket.com', 1, '$2b$12$d7mwUGX/x5jcRrxdNAFqJekces0IQ2f9F4mC24iVGZUpwM2pF1Qly', N'Admin VNA', '0111111111', 1, 0, GETUTCDATE(), GETUTCDATE(), 1),
('B2F3E2D4-BCDE-4F01-2345-6789ABCDEF92', 'admin.vj@airlineticket.com', 1, '$2b$12$d7mwUGX/x5jcRrxdNAFqJekces0IQ2f9F4mC24iVGZUpwM2pF1Qly', N'Admin VJ', '0222222222', 1, 0, GETUTCDATE(), GETUTCDATE(), 1),
('C3F3E2D4-BCDE-4F01-2345-6789ABCDEF93', 'admin.qh@airlineticket.com', 1, '$2b$12$d7mwUGX/x5jcRrxdNAFqJekces0IQ2f9F4mC24iVGZUpwM2pF1Qly', N'Admin QH', '0333333333', 1, 0, GETUTCDATE(), GETUTCDATE(), 1);
GO

-- Phân quyền quản trị cho các Airline Admin
INSERT INTO [dbo].[UserPermissionScopes] ([Id], [UserId], [PermissionId], [AirlineId], [ScopeDescription], [CreatedAt]) VALUES
(NEWID(), 'C3F3E2D4-BCDE-4F01-2345-6789ABCDEF93', 3, 'C3F3E2D4-BCDE-4F01-2345-6789ABCDEF03', N'Bamboo Airways Administration', GETUTCDATE()),
(NEWID(), 'C3F3E2D4-BCDE-4F01-2345-6789ABCDEF93', 5, 'C3F3E2D4-BCDE-4F01-2345-6789ABCDEF03', N'QH Flight Administration', GETUTCDATE());
GO

-- =====================================================
-- 2. SEED FLIGHTS DATA
-- =====================================================

-- Khởi tạo danh sách các Hãng hàng không (Airlines) để hiển thị cho Frontend
INSERT INTO [dbo].[Airlines] ([Id], [IataCode], [Name], [LogoUrl], [BaseCountry], [IsActive], [IsDeleted], [CreatedAt]) VALUES
('A1F3E2D4-BCDE-4F01-2345-6789ABCDEF01', 'VN', N'Vietnam Airlines', 'https://images.vietnamairlines.com/logos/vna-logo.png', N'Vietnam', 1, 0, GETUTCDATE()),
('B2F3E2D4-BCDE-4F01-2345-6789ABCDEF02', 'VJ', N'VietJet Air', 'https://www.vietjetair.com/static/media/logo.8efdcd6f.svg', N'Vietnam', 1, 0, GETUTCDATE()),
('C3F3E2D4-BCDE-4F01-2345-6789ABCDEF03', 'QH', N'Bamboo Airways', 'https://www.bambooairways.com/logo.png', N'Vietnam', 1, 0, GETUTCDATE()),
('D4F3E2D4-BCDE-4F01-2345-6789ABCDEF04', 'VU', N'Vietravel Airlines', 'https://www.vietravelairlines.com/logo.png', N'Vietnam', 1, 0, GETUTCDATE()),
('E1F3E2D4-BCDE-4F01-2345-6789ABCDEF27', 'SQ', N'Singapore Airlines', 'https://www.singaporeair.com/logo.png', N'Singapore', 1, 0, GETUTCDATE()),
('E2F3E2D4-BCDE-4F01-2345-6789ABCDEF28', 'JL', N'Japan Airlines', 'https://www.jal.co.jp/logo.png', N'Japan', 1, 0, GETUTCDATE()),
('E3F3E2D4-BCDE-4F01-2345-6789ABCDEF29', 'KE', N'Korean Air', 'https://www.koreanair.com/logo.png', N'South Korea', 1, 0, GETUTCDATE()),
('E4F3E2D4-BCDE-4F01-2345-6789ABCDEF30', 'CX', N'Cathay Pacific', 'https://www.cathaypacific.com/logo.png', N'Hong Kong', 1, 0, GETUTCDATE());
GO

-- Khởi tạo danh sách các Sân bay (Airports) phổ biến ở Việt Nam và Quốc tế
INSERT INTO [dbo].[Airports] ([Id], [IataCode], [NameEn], [NameVi], [CityEn], [CityVi], [CountryCode], [Timezone], [Latitude], [Longitude], [IsActive], [IsDeleted]) VALUES
('E5F3E2D4-BCDE-4F01-2345-6789ABCDEF05', 'HAN', 'Noi Bai International Airport', N'Sân bay quốc tế Nội Bài', 'Hanoi', N'Hà Nội', 'VN', 'Asia/Ho_Chi_Minh', 21.2212, 105.8072, 1, 0),
('F6F3E2D4-BCDE-4F01-2345-6789ABCDEF06', 'SGN', 'Tan Son Nhat International Airport', N'Sân bay quốc tế Tân Sơn Nhất', 'Ho Chi Minh City', N'TP. Hồ Chí Minh', 'VN', 'Asia/Ho_Chi_Minh', 10.8188, 106.6519, 1, 0),
('07F3E2D4-BCDE-4F01-2345-6789ABCDEF07', 'DAD', 'Da Nang International Airport', N'Sân bay quốc tế Đà Nẵng', 'Da Nang', N'Đà Nẵng', 'VN', 'Asia/Ho_Chi_Minh', 16.0439, 108.1994, 1, 0),
('08F3E2D4-BCDE-4F01-2345-6789ABCDEF08', 'NRT', 'Narita International Airport', N'Sân bay quốc tế Narita', 'Tokyo', N'Tokyo', 'JP', 'Asia/Tokyo', 35.7647, 140.3863, 1, 0),
('09F3E2D4-BCDE-4F01-2345-6789ABCDEF09', 'HND', 'Haneda Airport', N'Sân bay Haneda', 'Tokyo', N'Tokyo', 'JP', 'Asia/Tokyo', 35.5494, 139.7798, 1, 0),
('10F3E2D4-BCDE-4F01-2345-6789ABCDEF10', 'KIX', 'Kansai International Airport', N'Sân bay quốc tế Kansai', 'Osaka', N'Osaka', 'JP', 'Asia/Tokyo', 34.4320, 135.2304, 1, 0),
('11F3E2D4-BCDE-4F01-2345-6789ABCDEF11', 'ICN', 'Incheon International Airport', N'Sân bay quốc tế Incheon', 'Seoul', N'Seoul', 'KR', 'Asia/Seoul', 37.4602, 126.4407, 1, 0),
('12F3E2D4-BCDE-4F01-2345-6789ABCDEF12', 'PUS', 'Gimhae International Airport', N'Sân bay quốc tế Gimhae', 'Busan', N'Busan', 'KR', 'Asia/Seoul', 35.1795, 128.9382, 1, 0),
('13F3E2D4-BCDE-4F01-2345-6789ABCDEF13', 'PEK', 'Beijing Capital International Airport', N'Sân bay quốc tế Thủ đô Bắc Kinh', 'Beijing', N'Bắc Kinh', 'CN', 'Asia/Shanghai', 40.0799, 116.6031, 1, 0),
('14F3E2D4-BCDE-4F01-2345-6789ABCDEF14', 'PVG', 'Shanghai Pudong International Airport', N'Sân bay quốc tế Phố Đông Thượng Hải', 'Shanghai', N'Thượng Hải', 'CN', 'Asia/Shanghai', 31.1443, 121.8083, 1, 0),
('15F3E2D4-BCDE-4F01-2345-6789ABCDEF15', 'CAN', 'Guangzhou Baiyun International Airport', N'Sân bay quốc tế Bạch Vân Quảng Châu', 'Guangzhou', N'Quảng Châu', 'CN', 'Asia/Shanghai', 23.3924, 113.2988, 1, 0),
('16F3E2D4-BCDE-4F01-2345-6789ABCDEF16', 'HKG', 'Hong Kong International Airport', N'Sân bay quốc tế Hồng Kông', 'Hong Kong', N'Hồng Kông', 'HK', 'Asia/Hong_Kong', 22.3080, 113.9185, 1, 0),
('17F3E2D4-BCDE-4F01-2345-6789ABCDEF17', 'TPE', 'Taoyuan International Airport', N'Sân bay quốc tế Đào Viên', 'Taipei', N'Đài Bắc', 'TW', 'Asia/Taipei', 25.0797, 121.2342, 1, 0),
('18F3E2D4-BCDE-4F01-2345-6789ABCDEF18', 'BKK', 'Suvarnabhumi Airport', N'Sân bay Suvarnabhumi', 'Bangkok', N'Bangkok', 'TH', 'Asia/Bangkok', 13.6900, 100.7501, 1, 0),
('19F3E2D4-BCDE-4F01-2345-6789ABCDEF19', 'DMK', 'Don Mueang International Airport', N'Sân bay quốc tế Don Mueang', 'Bangkok', N'Bangkok', 'TH', 'Asia/Bangkok', 13.9126, 100.6068, 1, 0),
('20F3E2D4-BCDE-4F01-2345-6789ABCDEF20', 'HKT', 'Phuket International Airport', N'Sân bay quốc tế Phuket', 'Phuket', N'Phuket', 'TH', 'Asia/Bangkok', 8.1111, 98.3065, 1, 0),
('21F3E2D4-BCDE-4F01-2345-6789ABCDEF21', 'SIN', 'Changi Airport', N'Sân bay Changi', 'Singapore', N'Singapore', 'SG', 'Asia/Singapore', 1.3644, 103.9915, 1, 0),
('22F3E2D4-BCDE-4F01-2345-6789ABCDEF22', 'KUL', 'Kuala Lumpur International Airport', N'Sân bay quốc tế Kuala Lumpur', 'Kuala Lumpur', N'Kuala Lumpur', 'MY', 'Asia/Kuala_Lumpur', 2.7456, 101.7099, 1, 0),
('23F3E2D4-BCDE-4F01-2345-6789ABCDEF23', 'CGK', 'Soekarno-Hatta International Airport', N'Sân bay quốc tế Soekarno-Hatta', 'Jakarta', N'Jakarta', 'ID', 'Asia/Jakarta', -6.1256, 106.6558, 1, 0),
('24F3E2D4-BCDE-4F01-2345-6789ABCDEF24', 'DPS', 'Ngurah Rai International Airport', N'Sân bay quốc tế Ngurah Rai', 'Bali', N'Bali', 'ID', 'Asia/Makassar', -8.7482, 115.1675, 1, 0),
('25F3E2D4-BCDE-4F01-2345-6789ABCDEF25', 'MNL', 'Ninoy Aquino International Airport', N'Sân bay quốc tế Ninoy Aquino', 'Manila', N'Manila', 'PH', 'Asia/Manila', 14.5086, 121.0194, 1, 0),
('26F3E2D4-BCDE-4F01-2345-6789ABCDEF26', 'SYD', 'Sydney Airport', N'Sân bay Sydney', 'Sydney', N'Sydney', 'AU', 'Australia/Sydney', -33.9399, 151.1753, 1, 0),
('27F3E2D4-BCDE-4F01-2345-6789ABCDEF27', 'MEL', 'Melbourne Airport', N'Sân bay Melbourne', 'Melbourne', N'Melbourne', 'AU', 'Australia/Melbourne', -37.6690, 144.8410, 1, 0),
('28F3E2D4-BCDE-4F01-2345-6789ABCDEF28', 'LHR', 'Heathrow Airport', N'Sân bay Heathrow', 'London', N'Luân Đôn', 'GB', 'Europe/London', 51.4700, -0.4543, 1, 0),
('29F3E2D4-BCDE-4F01-2345-6789ABCDEF29', 'CDG', 'Charles de Gaulle Airport', N'Sân bay Charles de Gaulle', 'Paris', N'Paris', 'FR', 'Europe/Paris', 49.0097, 2.5479, 1, 0),
('30F3E2D4-BCDE-4F01-2345-6789ABCDEF30', 'FRA', 'Frankfurt Airport', N'Sân bay Frankfurt', 'Frankfurt', N'Frankfurt', 'DE', 'Europe/Berlin', 50.0333, 8.5706, 1, 0),
('31F3E2D4-BCDE-4F01-2345-6789ABCDEF31', 'AMS', 'Amsterdam Airport Schiphol', N'Sân bay Schiphol', 'Amsterdam', N'Amsterdam', 'NL', 'Europe/Amsterdam', 52.3105, 4.7683, 1, 0),
('32F3E2D4-BCDE-4F01-2345-6789ABCDEF32', 'DXB', 'Dubai International Airport', N'Sân bay quốc tế Dubai', 'Dubai', N'Dubai', 'AE', 'Asia/Dubai', 25.2532, 55.3657, 1, 0),
('33F3E2D4-BCDE-4F01-2345-6789ABCDEF33', 'DOH', 'Hamad International Airport', N'Sân bay quốc tế Hamad', 'Doha', N'Doha', 'QA', 'Asia/Qatar', 25.2731, 51.6080, 1, 0),
('34F3E2D4-BCDE-4F01-2345-6789ABCDEF34', 'JFK', 'John F. Kennedy International Airport', N'Sân bay quốc tế John F. Kennedy', 'New York', N'New York', 'US', 'America/New_York', 40.6413, -73.7781, 1, 0),
('35F3E2D4-BCDE-4F01-2345-6789ABCDEF35', 'LAX', 'Los Angeles International Airport', N'Sân bay quốc tế Los Angeles', 'Los Angeles', N'Los Angeles', 'US', 'America/Los_Angeles', 33.9416, -118.4085, 1, 0),
('36F3E2D4-BCDE-4F01-2345-6789ABCDEF36', 'SFO', 'San Francisco International Airport', N'Sân bay quốc tế San Francisco', 'San Francisco', N'San Francisco', 'US', 'America/Los_Angeles', 37.6213, -122.3790, 1, 0),
('37F3E2D4-BCDE-4F01-2345-6789ABCDEF37', 'YVR', 'Vancouver International Airport', N'Sân bay quốc tế Vancouver', 'Vancouver', N'Vancouver', 'CA', 'America/Vancouver', 49.1967, -123.1815, 1, 0);
GO

-- Seed Airplanes (Máy bay mẫu)
INSERT INTO [dbo].[Airplanes] ([Id], [AirlineId], [Model], [RegistrationNumber], [TotalCapacity], [IsDeleted]) VALUES
('81F3E2D4-BCDE-4F01-2345-6789ABCDEF01', 'A1F3E2D4-BCDE-4F01-2345-6789ABCDEF01', 'Boeing 787-9 Dreamliner', 'VN-A861', 274, 0),
('82F3E2D4-BCDE-4F01-2345-6789ABCDEF02', 'A1F3E2D4-BCDE-4F01-2345-6789ABCDEF01', 'Airbus A350-900', 'VN-A886', 305, 0),
('83F3E2D4-BCDE-4F01-2345-6789ABCDEF03', 'B2F3E2D4-BCDE-4F01-2345-6789ABCDEF02', 'Airbus A321neo', 'VN-A600', 230, 0),
('84F3E2D4-BCDE-4F01-2345-6789ABCDEF04', 'C3F3E2D4-BCDE-4F01-2345-6789ABCDEF03', 'Boeing 787-9 Dreamliner', 'VN-A819', 294, 0);
GO

-- Seed Routes (Tuyến bay mẫu)
INSERT INTO [dbo].[Routes] ([Id], [AirlineId], [OriginAirportId], [DestinationAirportId], [DistanceKm], [EstimatedDurationMinutes], [IsDeleted]) VALUES
('91F3E2D4-BCDE-4F01-2345-6789ABCDEF01', 'A1F3E2D4-BCDE-4F01-2345-6789ABCDEF01', 'E5F3E2D4-BCDE-4F01-2345-6789ABCDEF05', 'F6F3E2D4-BCDE-4F01-2345-6789ABCDEF06', 1160, 130, 0), -- HAN to SGN (VN)
('92F3E2D4-BCDE-4F01-2345-6789ABCDEF02', 'B2F3E2D4-BCDE-4F01-2345-6789ABCDEF02', 'F6F3E2D4-BCDE-4F01-2345-6789ABCDEF06', 'E5F3E2D4-BCDE-4F01-2345-6789ABCDEF05', 1160, 130, 0), -- SGN to HAN (VJ)
('93F3E2D4-BCDE-4F01-2345-6789ABCDEF03', 'A1F3E2D4-BCDE-4F01-2345-6789ABCDEF01', 'E5F3E2D4-BCDE-4F01-2345-6789ABCDEF05', '07F3E2D4-BCDE-4F01-2345-6789ABCDEF07', 627, 85, 0);   -- HAN to DAD (VN)
GO

-- Seed Flights (Chuyến bay mẫu)
INSERT INTO [dbo].[Flights] ([Id], [RouteId], [AirplaneId], [FlightNumber], [DepartureTime], [ArrivalTime], [BasePrice], [Currency], [Status], [IsDeleted], [CreatedAt], [UpdatedAt]) VALUES
('F1F3E2D4-BCDE-4F01-2345-6789ABCDEF01', '91F3E2D4-BCDE-4F01-2345-6789ABCDEF01', '81F3E2D4-BCDE-4F01-2345-6789ABCDEF01', 'VN213', DATEADD(day, 1, GETUTCDATE()), DATEADD(minute, 130, DATEADD(day, 1, GETUTCDATE())), 80.00, 'USD', 0, 0, GETUTCDATE(), GETUTCDATE()),
('F2F3E2D4-BCDE-4F01-2345-6789ABCDEF02', '92F3E2D4-BCDE-4F01-2345-6789ABCDEF02', '83F3E2D4-BCDE-4F01-2345-6789ABCDEF03', 'VJ120', DATEADD(day, 2, GETUTCDATE()), DATEADD(minute, 130, DATEADD(day, 2, GETUTCDATE())), 50.00, 'USD', 0, 0, GETUTCDATE(), GETUTCDATE());
GO

-- Seed FlightSeats (Ghế của chuyến bay)
-- Cấu hình ghế cho VN213
INSERT INTO [dbo].[FlightSeats] ([Id], [FlightId], [SeatNumber], [SeatClass], [PriceOverride], [IsAvailable], [IsExtraLegroom]) VALUES
(NEWID(), 'F1F3E2D4-BCDE-4F01-2345-6789ABCDEF01', '01A', 2, 150.00, 1, 1), -- Business
(NEWID(), 'F1F3E2D4-BCDE-4F01-2345-6789ABCDEF01', '01B', 2, 150.00, 1, 1),
(NEWID(), 'F1F3E2D4-BCDE-4F01-2345-6789ABCDEF01', '10A', 0, NULL, 1, 0),   -- Economy
(NEWID(), 'F1F3E2D4-BCDE-4F01-2345-6789ABCDEF01', '10B', 0, NULL, 1, 0),
(NEWID(), 'F1F3E2D4-BCDE-4F01-2345-6789ABCDEF01', '11A', 0, NULL, 1, 0),
(NEWID(), 'F1F3E2D4-BCDE-4F01-2345-6789ABCDEF01', '11B', 0, NULL, 1, 0);

-- Cấu hình ghế cho VJ120
INSERT INTO [dbo].[FlightSeats] ([Id], [FlightId], [SeatNumber], [SeatClass], [PriceOverride], [IsAvailable], [IsExtraLegroom]) VALUES
(NEWID(), 'F2F3E2D4-BCDE-4F01-2345-6789ABCDEF02', '01A', 1, 80.00, 1, 1),  -- Premium Economy
(NEWID(), 'F2F3E2D4-BCDE-4F01-2345-6789ABCDEF02', '01B', 1, 80.00, 1, 1),
(NEWID(), 'F2F3E2D4-BCDE-4F01-2345-6789ABCDEF02', '05A', 0, NULL, 1, 0),   -- Economy
(NEWID(), 'F2F3E2D4-BCDE-4F01-2345-6789ABCDEF02', '05B', 0, NULL, 1, 0);
GO


-- =====================================================
-- 3. SEED BOOKINGS DATA (Bookings, Passengers, Tickets, Payments)
-- =====================================================

-- Seed Bookings
INSERT INTO [dbo].[Bookings] ([Id], [UserId], [PnrCode], [TotalPrice], [Currency], [Status], [ContactEmail], [ContactPhone], [SpecialRequests], [IsDeleted], [CreatedAt], [UpdatedAt]) VALUES
('B1B0F2A8-9B2F-4A9B-89E3-4E80D77BC901', 'E2E3F4C5-1A2B-3C4D-5E6F-7A8B9C0D1E2F', 'PNR123', 160.00, 'USD', 2, 'client@gmail.com', '0987654321', N'No Special Requests', 0, GETUTCDATE(), GETUTCDATE());
GO

-- Seed Passengers
INSERT INTO [dbo].[Passengers] ([Id], [BookingId], [FirstName], [LastName], [Gender], [DateOfBirth], [Nationality], [PassportNumber], [PassportExpiryDate]) VALUES
('FA10F2A8-9B2F-4A9B-89E3-4E80D77BC901', 'B1B0F2A8-9B2F-4A9B-89E3-4E80D77BC901', N'Văn A', N'Nguyễn', 0, '1990-05-15', N'Vietnam', 'B1234567', '2030-05-15');
GO

-- Seed Tickets
INSERT INTO [dbo].[Tickets] ([Id], [BookingId], [PassengerId], [FlightId], [SeatId], [TicketNumber], [Gate], [BoardingTime], [Status]) 
SELECT 
    'B110F2A8-9B2F-4A9B-89E3-4E80D77BC901', 
    'B1B0F2A8-9B2F-4A9B-89E3-4E80D77BC901', 
    'FA10F2A8-9B2F-4A9B-89E3-4E80D77BC901', 
    'F1F3E2D4-BCDE-4F01-2345-6789ABCDEF01', 
    fs.Id, 
    'TK-VN-20250615-001', 
    'Gate 5', 
    DATEADD(minute, -40, DATEADD(day, 1, GETUTCDATE())), 
    0
FROM [dbo].[FlightSeats] fs 
WHERE fs.FlightId = 'F1F3E2D4-BCDE-4F01-2345-6789ABCDEF01' AND fs.SeatNumber = '10A';
GO

-- Seed Payments
INSERT INTO [dbo].[Payments] ([Id], [BookingId], [TransactionId], [Amount], [PaymentMethod], [ProviderStatus], [IsSuccessful], [RawResponse], [CreatedAt]) VALUES
('FF10F2A8-9B2F-4A9B-89E3-4E80D77BC901', 'B1B0F2A8-9B2F-4A9B-89E3-4E80D77BC901', 'TXN-987654321', 160.00, 'Stripe', 'succeeded', 1, '{"status": "succeeded", "charge_id": "ch_123"}', GETUTCDATE());
GO


-- =====================================================
-- 4. SEED PROMOTIONS DATA (Coupons, Campaigns)
-- =====================================================

INSERT INTO [dbo].[Coupons] ([Id], [Code], [Description], [DiscountType], [DiscountValue], [MinOrderValue], [MaxDiscountAmount], [StartDate], [EndDate], [UsageLimit], [UsageCount], [IsActive], [IsDeleted]) VALUES
('C1F3E2D4-BCDE-4F01-2345-6789ABCDEF01', 'SUMMER2025', N'Giảm giá 10% dịp Hè', 0, 10.00, 100.00, 50.00, GETUTCDATE(), DATEADD(month, 3, GETUTCDATE()), 1000, 0, 1, 0),
('C2F3E2D4-BCDE-4F01-2345-6789ABCDEF02', 'WELCOME10', N'Giảm 10$ cho đơn hàng đầu tiên', 1, 10.00, 0.00, 10.00, GETUTCDATE(), DATEADD(year, 1, GETUTCDATE()), 5000, 0, 1, 0);
GO

-- Seed Campaigns (Chiến dịch marketing)
INSERT INTO [dbo].[Campaigns] ([Id], [Title], [BannerUrl], [Content], [StartDate], [EndDate], [IsFeatured], [IsDeleted]) VALUES
('CC13E2D4-BCDE-4F01-2345-6789ABCDEF01', N'Siêu Hè Rực Rỡ 2025', 'https://images.vietnamairlines.com/banners/summer-2025.jpg', N'Giảm giá lên đến 20% các chặng bay nội địa và quốc tế dịp hè từ 01/06 đến 31/08/2025.', GETUTCDATE(), DATEADD(month, 3, GETUTCDATE()), 1, 0),
('CC23E2D4-BCDE-4F01-2345-6789ABCDEF02', N'Mùa Thu Vàng', 'https://images.vietnamairlines.com/banners/autumn-2025.jpg', N'Đón thu vàng cùng ngập tràn khuyến mãi vé bay khứ hồi giá cực tốt.', DATEADD(month, 3, GETUTCDATE()), DATEADD(month, 5, GETUTCDATE()), 0, 0);
GO


-- =====================================================
-- 5. SEED CMS DATA (Categories, Articles)
-- =====================================================

-- Seed Categories
INSERT INTO [dbo].[Categories] ([Id], [Name], [Slug], [IsDeleted]) VALUES
('CA11E2D4-BCDE-4F01-2345-6789ABCDEF01', N'Tin Tức Khuyến Mãi', 'tin-tuc-khuyen-mai', 0),
('CA22E2D4-BCDE-4F01-2345-6789ABCDEF02', N'Cẩm Nang Du Lịch', 'cam-nang-du-lich', 0);
GO

-- Seed Articles
INSERT INTO [dbo].[Articles] ([Id], [CategoryId], [AuthorId], [Title], [Slug], [Summary], [Content], [ThumbnailUrl], [PublishedAt], [Status], [ViewCount], [IsDeleted], [CreatedAt]) VALUES
('AE10E2D4-BCDE-4F01-2345-6789ABCDEF01', 'CA22E2D4-BCDE-4F01-2345-6789ABCDEF02', 'D4B0F2A8-9B2F-4A9B-89E3-4E80D77BC901', N'Cẩm nang du lịch Phú Quốc từ A đến Z năm 2025', 'cam-nang-du-lich-phu-quoc-2025', N'Những lưu ý quan trọng khi đi du lịch Phú Quốc tự túc.', N'Bài viết này cung cấp toàn bộ kinh nghiệm bay, đặt khách sạn, ẩm thực và địa điểm vui chơi tại Phú Quốc cho du khách.', 'https://images.phuquoc.vn/thumbnail.jpg', GETUTCDATE(), 1, 245, 0, GETUTCDATE());
GO


-- =====================================================
-- 6. SEED INTERACTIONS DATA (Reviews)
-- =====================================================

INSERT INTO [dbo].[Reviews] ([Id], [UserId], [AirlineId], [FlightId], [Rating], [Comment], [IsVerifiedPurchase], [IsHidden], [IsDeleted], [CreatedAt]) VALUES
('EE10E2D4-BCDE-4F01-2345-6789ABCDEF01', 'E2E3F4C5-1A2B-3C4D-5E6F-7A8B9C0D1E2F', 'A1F3E2D4-BCDE-4F01-2345-6789ABCDEF01', 'F1F3E2D4-BCDE-4F01-2345-6789ABCDEF01', 5, N'Dịch vụ của Vietnam Airlines rất tốt, bay đúng giờ, tiếp viên thân thiện.', 1, 0, 0, GETUTCDATE());
GO


-- =====================================================
-- 7. SEED NOTIFICATIONS DATA (Templates)
-- =====================================================

INSERT INTO [dbo].[NotificationTemplates] ([Id], [Code], [Subject], [BodyTemplate], [Language], [CreatedAt]) VALUES
('F1F3E2D4-BCDE-4F01-2345-6789ABCDEF01', 'BOOKING_CONFIRMED', N'Xác nhận đặt vé thành công', N'Chào {{PassengerName}}, Đặt chỗ của bạn (Mã: {{PnrCode}}) đã được xác nhận thành công. Chuyến bay: {{FlightNumber}}.', 'vi', GETUTCDATE()),
('F2F3E2D4-BCDE-4F01-2345-6789ABCDEF02', 'BOOKING_CONFIRMED_EN', 'Booking Confirmation', 'Hello {{PassengerName}}, Your booking (PNR: {{PnrCode}}) has been confirmed. Flight: {{FlightNumber}}.', 'en', GETUTCDATE());
GO
