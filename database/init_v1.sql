-- =====================================================
-- AIRLINE TICKET SYSTEM - DATABASE INITIALIZATION V1
-- =====================================================

-- =====================================================
-- 1. CREATE SCHEMAS
-- =====================================================
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'flights')
    EXEC sp_executesql N'CREATE SCHEMA flights'
GO

IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'bookings')
    EXEC sp_executesql N'CREATE SCHEMA bookings'
GO

IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'users')
    EXEC sp_executesql N'CREATE SCHEMA users'
GO

IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'promotions')
    EXEC sp_executesql N'CREATE SCHEMA promotions'
GO

-- =====================================================
-- 2. FLIGHTS SCHEMA - TABLES
-- =====================================================

-- Airports (Sân bay)
IF OBJECT_ID('[flights].[Airports]', 'U') IS NOT NULL 
    DROP TABLE [flights].[Airports]
GO

CREATE TABLE [flights].[Airports] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [IataCode] NVARCHAR(10) NOT NULL UNIQUE,
    [Name] NVARCHAR(255) NOT NULL,
    [City] NVARCHAR(100) NOT NULL,
    [Country] NVARCHAR(100) NOT NULL,
    [Timezone] NVARCHAR(50) NOT NULL,
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2 DEFAULT GETUTCDATE()
)
GO

-- Airlines (Hãng bay)
IF OBJECT_ID('[flights].[Airlines]', 'U') IS NOT NULL 
    DROP TABLE [flights].[Airlines]
GO

CREATE TABLE [flights].[Airlines] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [IataCode] NVARCHAR(10) NOT NULL UNIQUE,
    [Name] NVARCHAR(255) NOT NULL,
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2 DEFAULT GETUTCDATE()
)
GO

-- Airplanes (Máy bay)
IF OBJECT_ID('[flights].[Airplanes]', 'U') IS NOT NULL 
    DROP TABLE [flights].[Airplanes]
GO

CREATE TABLE [flights].[Airplanes] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [AirlineId] UNIQUEIDENTIFIER NOT NULL,
    [Model] NVARCHAR(100) NOT NULL,
    [RegistrationNumber] NVARCHAR(50) NOT NULL UNIQUE,
    [TotalCapacity] INT NOT NULL,
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    CONSTRAINT [FK_Airplanes_Airlines] FOREIGN KEY ([AirlineId]) REFERENCES [flights].[Airlines]([Id])
)
GO

-- AirplaneSeats (Cấu hình ghế mẫu)
IF OBJECT_ID('[flights].[AirplaneSeats]', 'U') IS NOT NULL 
    DROP TABLE [flights].[AirplaneSeats]
GO

CREATE TABLE [flights].[AirplaneSeats] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [AirplaneId] UNIQUEIDENTIFIER NOT NULL,
    [SeatNumber] NVARCHAR(10) NOT NULL,
    [SeatClass] INT NOT NULL, -- 0: Economy, 1: Business, 2: First
    [PriceMultiplier] DECIMAL(10, 2) NOT NULL DEFAULT 1.0,
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    CONSTRAINT [FK_AirplaneSeats_Airplanes] FOREIGN KEY ([AirplaneId]) REFERENCES [flights].[Airplanes]([Id]),
    CONSTRAINT [UC_AirplaneSeats_SeatNumber] UNIQUE ([AirplaneId], [SeatNumber])
)
GO

-- Routes (Tuyến bay)
IF OBJECT_ID('[flights].[Routes]', 'U') IS NOT NULL 
    DROP TABLE [flights].[Routes]
GO

CREATE TABLE [flights].[Routes] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [AirlineId] UNIQUEIDENTIFIER NOT NULL,
    [OriginAirportId] UNIQUEIDENTIFIER NOT NULL,
    [DestinationAirportId] UNIQUEIDENTIFIER NOT NULL,
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    CONSTRAINT [FK_Routes_Airlines] FOREIGN KEY ([AirlineId]) REFERENCES [flights].[Airlines]([Id]),
    CONSTRAINT [FK_Routes_OriginAirport] FOREIGN KEY ([OriginAirportId]) REFERENCES [flights].[Airports]([Id]),
    CONSTRAINT [FK_Routes_DestinationAirport] FOREIGN KEY ([DestinationAirportId]) REFERENCES [flights].[Airports]([Id])
)
GO

-- Flights (Chuyến bay thực tế)
IF OBJECT_ID('[flights].[Flights]', 'U') IS NOT NULL 
    DROP TABLE [flights].[Flights]
GO

CREATE TABLE [flights].[Flights] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [RouteId] UNIQUEIDENTIFIER NOT NULL,
    [AirplaneId] UNIQUEIDENTIFIER NOT NULL,
    [FlightNumber] NVARCHAR(20) NOT NULL,
    [BasePrice] DECIMAL(15, 2) NOT NULL,
    [ScheduledDeparture] DATETIME2 NOT NULL,
    [ScheduledArrival] DATETIME2 NOT NULL,
    [ActualDeparture] DATETIME2 NULL,
    [ActualArrival] DATETIME2 NULL,
    [Status] INT NOT NULL DEFAULT 0, -- 0: Scheduled, 1: Delayed, 2: Boarding, 3: InAir, 4: Landed, 5: Cancelled
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    CONSTRAINT [FK_Flights_Routes] FOREIGN KEY ([RouteId]) REFERENCES [flights].[Routes]([Id]),
    CONSTRAINT [FK_Flights_Airplanes] FOREIGN KEY ([AirplaneId]) REFERENCES [flights].[Airplanes]([Id])
)
GO

-- FlightSeats (Ghế của chuyến bay cụ thể)
IF OBJECT_ID('[flights].[FlightSeats]', 'U') IS NOT NULL 
    DROP TABLE [flights].[FlightSeats]
GO

CREATE TABLE [flights].[FlightSeats] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [FlightId] UNIQUEIDENTIFIER NOT NULL,
    [SeatNumber] NVARCHAR(10) NOT NULL,
    [SeatClass] INT NOT NULL, -- 0: Economy, 1: Business, 2: First
    [Price] DECIMAL(15, 2) NOT NULL,
    [IsAvailable] BIT NOT NULL DEFAULT 1,
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    CONSTRAINT [FK_FlightSeats_Flights] FOREIGN KEY ([FlightId]) REFERENCES [flights].[Flights]([Id]),
    CONSTRAINT [UC_FlightSeats_SeatNumber] UNIQUE ([FlightId], [SeatNumber])
)
GO

-- =====================================================
-- 3. USERS SCHEMA - TABLES
-- =====================================================

-- Users (Người dùng)
IF OBJECT_ID('[users].[Users]', 'U') IS NOT NULL 
    DROP TABLE [users].[Users]
GO

CREATE TABLE [users].[Users] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [Email] NVARCHAR(255) NOT NULL UNIQUE,
    [PasswordHash] NVARCHAR(MAX) NOT NULL,
    [FullName] NVARCHAR(255) NOT NULL,
    [Phone] NVARCHAR(20) NULL,
    [Role] INT NOT NULL DEFAULT 0, -- 0: Customer, 1: Admin, 2: Staff
    [IsActive] BIT NOT NULL DEFAULT 1,
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2 DEFAULT GETUTCDATE()
)
GO

-- =====================================================
-- 4. BOOKINGS SCHEMA - TABLES
-- =====================================================

-- Bookings (Đơn đặt chỗ)
IF OBJECT_ID('[bookings].[Bookings]', 'U') IS NOT NULL 
    DROP TABLE [bookings].[Bookings]
GO

CREATE TABLE [bookings].[Bookings] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [UserId] UNIQUEIDENTIFIER NOT NULL,
    [PnrCode] NVARCHAR(20) NOT NULL UNIQUE,
    [TotalPrice] DECIMAL(15, 2) NOT NULL,
    [Status] INT NOT NULL DEFAULT 0, -- 0: Pending, 1: Confirmed, 2: Cancelled, 3: Refunded
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    CONSTRAINT [FK_Bookings_Users] FOREIGN KEY ([UserId]) REFERENCES [users].[Users]([Id])
)
GO

-- Passengers (Hành khách)
IF OBJECT_ID('[bookings].[Passengers]', 'U') IS NOT NULL 
    DROP TABLE [bookings].[Passengers]
GO

CREATE TABLE [bookings].[Passengers] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [BookingId] UNIQUEIDENTIFIER NOT NULL,
    [FirstName] NVARCHAR(100) NOT NULL,
    [LastName] NVARCHAR(100) NOT NULL,
    [DateOfBirth] DATE NOT NULL,
    [PassportNumber] NVARCHAR(50) NOT NULL,
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    CONSTRAINT [FK_Passengers_Bookings] FOREIGN KEY ([BookingId]) REFERENCES [bookings].[Bookings]([Id])
)
GO

-- Tickets (Vé điện tử)
IF OBJECT_ID('[bookings].[Tickets]', 'U') IS NOT NULL 
    DROP TABLE [bookings].[Tickets]
GO

CREATE TABLE [bookings].[Tickets] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [BookingId] UNIQUEIDENTIFIER NOT NULL,
    [FlightId] UNIQUEIDENTIFIER NOT NULL,
    [PassengerId] UNIQUEIDENTIFIER NOT NULL,
    [SeatId] UNIQUEIDENTIFIER NOT NULL,
    [TicketNumber] NVARCHAR(50) NOT NULL UNIQUE,
    [Status] INT NOT NULL DEFAULT 0, -- 0: Issued, 1: Used, 2: Cancelled
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    CONSTRAINT [FK_Tickets_Bookings] FOREIGN KEY ([BookingId]) REFERENCES [bookings].[Bookings]([Id]),
    CONSTRAINT [FK_Tickets_Flights] FOREIGN KEY ([FlightId]) REFERENCES [flights].[Flights]([Id]),
    CONSTRAINT [FK_Tickets_Passengers] FOREIGN KEY ([PassengerId]) REFERENCES [bookings].[Passengers]([Id]),
    CONSTRAINT [FK_Tickets_FlightSeats] FOREIGN KEY ([SeatId]) REFERENCES [flights].[FlightSeats]([Id])
)
GO

-- Payments (Thanh toán)
IF OBJECT_ID('[bookings].[Payments]', 'U') IS NOT NULL 
    DROP TABLE [bookings].[Payments]
GO

CREATE TABLE [bookings].[Payments] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [BookingId] UNIQUEIDENTIFIER NOT NULL,
    [Amount] DECIMAL(15, 2) NOT NULL,
    [PaymentMethod] NVARCHAR(50) NOT NULL,
    [Status] INT NOT NULL DEFAULT 0, -- 0: Pending, 1: Completed, 2: Failed
    [TransactionId] NVARCHAR(100) NULL,
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    CONSTRAINT [FK_Payments_Bookings] FOREIGN KEY ([BookingId]) REFERENCES [bookings].[Bookings]([Id])
)
GO

-- =====================================================
-- 5. PROMOTIONS SCHEMA - TABLES
-- =====================================================

-- Campaigns (Chiến dịch giảm giá)
IF OBJECT_ID('[promotions].[Campaigns]', 'U') IS NOT NULL 
    DROP TABLE [promotions].[Campaigns]
GO

CREATE TABLE [promotions].[Campaigns] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [Name] NVARCHAR(255) NOT NULL,
    [Description] NVARCHAR(MAX) NULL,
    [StartDate] DATETIME2 NOT NULL,
    [EndDate] DATETIME2 NOT NULL,
    [DiscountType] INT NOT NULL, -- 0: Percentage, 1: FixedAmount
    [DiscountValue] DECIMAL(15, 2) NOT NULL,
    [TargetAirlineId] UNIQUEIDENTIFIER NULL,
    [TargetFlightId] UNIQUEIDENTIFIER NULL,
    [IsActive] BIT NOT NULL DEFAULT 1,
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    CONSTRAINT [FK_Campaigns_Airlines] FOREIGN KEY ([TargetAirlineId]) REFERENCES [flights].[Airlines]([Id]),
    CONSTRAINT [FK_Campaigns_Flights] FOREIGN KEY ([TargetFlightId]) REFERENCES [flights].[Flights]([Id])
)
GO

-- Coupons (Mã giảm giá)
IF OBJECT_ID('[promotions].[Coupons]', 'U') IS NOT NULL 
    DROP TABLE [promotions].[Coupons]
GO

CREATE TABLE [promotions].[Coupons] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [Code] NVARCHAR(50) NOT NULL UNIQUE,
    [DiscountType] INT NOT NULL, -- 0: Percentage, 1: FixedAmount
    [DiscountValue] DECIMAL(15, 2) NOT NULL,
    [MaxDiscountAmount] DECIMAL(15, 2) NULL,
    [MaxUsages] INT NULL,
    [CurrentUsages] INT DEFAULT 0,
    [ValidFrom] DATETIME2 NOT NULL,
    [ValidTo] DATETIME2 NOT NULL,
    [IsActive] BIT NOT NULL DEFAULT 1,
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2 DEFAULT GETUTCDATE()
)
GO

-- =====================================================
-- 6. CREATE INDEXES
-- =====================================================

-- Airports
CREATE NONCLUSTERED INDEX [IX_Airports_IataCode] ON [flights].[Airports]([IataCode])
GO

-- Airlines
CREATE NONCLUSTERED INDEX [IX_Airlines_IataCode] ON [flights].[Airlines]([IataCode])
GO

-- Flights
CREATE NONCLUSTERED INDEX [IX_Flights_RouteId] ON [flights].[Flights]([RouteId])
GO

CREATE NONCLUSTERED INDEX [IX_Flights_Status] ON [flights].[Flights]([Status])
GO

CREATE NONCLUSTERED INDEX [IX_Flights_ScheduledDeparture] ON [flights].[Flights]([ScheduledDeparture])
GO

-- FlightSeats
CREATE NONCLUSTERED INDEX [IX_FlightSeats_FlightId] ON [flights].[FlightSeats]([FlightId])
GO

CREATE NONCLUSTERED INDEX [IX_FlightSeats_IsAvailable] ON [flights].[FlightSeats]([IsAvailable])
GO

-- Users
CREATE NONCLUSTERED INDEX [IX_Users_Email] ON [users].[Users]([Email])
GO

-- Bookings
CREATE NONCLUSTERED INDEX [IX_Bookings_UserId] ON [bookings].[Bookings]([UserId])
GO

CREATE NONCLUSTERED INDEX [IX_Bookings_Status] ON [bookings].[Bookings]([Status])
GO

-- Tickets
CREATE NONCLUSTERED INDEX [IX_Tickets_BookingId] ON [bookings].[Tickets]([BookingId])
GO

CREATE NONCLUSTERED INDEX [IX_Tickets_PassengerId] ON [bookings].[Tickets]([PassengerId])
GO

-- Passengers
CREATE NONCLUSTERED INDEX [IX_Passengers_BookingId] ON [bookings].[Passengers]([BookingId])
GO

-- Payments
CREATE NONCLUSTERED INDEX [IX_Payments_BookingId] ON [bookings].[Payments]([BookingId])
GO

CREATE NONCLUSTERED INDEX [IX_Payments_Status] ON [bookings].[Payments]([Status])
GO

-- Campaigns
CREATE NONCLUSTERED INDEX [IX_Campaigns_IsActive] ON [promotions].[Campaigns]([IsActive])
GO

-- Coupons
CREATE NONCLUSTERED INDEX [IX_Coupons_Code] ON [promotions].[Coupons]([Code])
GO

CREATE NONCLUSTERED INDEX [IX_Coupons_IsActive] ON [promotions].[Coupons]([IsActive])
GO

-- =====================================================
-- 7. SEED DATA
-- =====================================================

-- Insert sample airports
INSERT INTO [flights].[Airports] ([IataCode], [Name], [City], [Country], [Timezone])
VALUES
    ('SGN', 'Tan Son Nhat International Airport', 'Ho Chi Minh City', 'Vietnam', 'UTC+7'),
    ('HAN', 'Noi Bai International Airport', 'Hanoi', 'Vietnam', 'UTC+7'),
    ('DAD', 'Da Nang International Airport', 'Da Nang', 'Vietnam', 'UTC+7'),
    ('NRT', 'Narita International Airport', 'Tokyo', 'Japan', 'UTC+9'),
    ('ICN', 'Incheon International Airport', 'Seoul', 'South Korea', 'UTC+9')
GO

-- Insert sample airlines
INSERT INTO [flights].[Airlines] ([IataCode], [Name])
VALUES
    ('VN', 'Vietnam Airlines'),
    ('VJ', 'VietJet Air'),
    ('BL', 'Bamboo Airways')
GO

-- Insert sample airplanes
DECLARE @airlineId UNIQUEIDENTIFIER
SELECT TOP 1 @airlineId = [Id] FROM [flights].[Airlines] WHERE [IataCode] = 'VN'

INSERT INTO [flights].[Airplanes] ([AirlineId], [Model], [RegistrationNumber], [TotalCapacity])
VALUES
    (@airlineId, 'Boeing 787', 'VN-A123', 242),
    (@airlineId, 'Airbus A350', 'VN-A456', 325)
GO

-- Insert sample seats for first airplane
DECLARE @airplaneId UNIQUEIDENTIFIER
SELECT TOP 1 @airplaneId = [Id] FROM [flights].[Airplanes]

-- Economy seats (1.0x multiplier)
INSERT INTO [flights].[AirplaneSeats] ([AirplaneId], [SeatNumber], [SeatClass], [PriceMultiplier])
SELECT @airplaneId, CHAR(ASCII('A') + seq.seq - 1) + CAST(row_num.row_num AS VARCHAR), 0, 1.0
FROM (
    SELECT ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS seq
    FROM (SELECT 1 UNION SELECT 2 UNION SELECT 3 UNION SELECT 4 UNION SELECT 5 UNION SELECT 6) AS t1
) seq
CROSS JOIN (
    SELECT ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS row_num
    FROM (SELECT 1 UNION SELECT 2 UNION SELECT 3 UNION SELECT 4 UNION SELECT 5 UNION 
          SELECT 6 UNION SELECT 7 UNION SELECT 8 UNION SELECT 9 UNION SELECT 10 UNION
          SELECT 11 UNION SELECT 12 UNION SELECT 13 UNION SELECT 14 UNION SELECT 15 UNION
          SELECT 16 UNION SELECT 17 UNION SELECT 18 UNION SELECT 19 UNION SELECT 20 UNION
          SELECT 21 UNION SELECT 22 UNION SELECT 23 UNION SELECT 24 UNION SELECT 25 UNION
          SELECT 26 UNION SELECT 27 UNION SELECT 28 UNION SELECT 29 UNION SELECT 30) AS t2
) row_num
WHERE seq.seq <= 6 AND row_num.row_num <= 30

-- Business seats (1.5x multiplier)
INSERT INTO [flights].[AirplaneSeats] ([AirplaneId], [SeatNumber], [SeatClass], [PriceMultiplier])
SELECT @airplaneId, CHAR(ASCII('A') + seq.seq - 1) + CAST(row_num.row_num AS VARCHAR), 1, 1.5
FROM (
    SELECT ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS seq
    FROM (SELECT 1 UNION SELECT 2 UNION SELECT 3 UNION SELECT 4) AS t1
) seq
CROSS JOIN (
    SELECT ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS row_num
    FROM (SELECT 1 UNION SELECT 2 UNION SELECT 3 UNION SELECT 4 UNION SELECT 5 UNION 
          SELECT 6 UNION SELECT 7 UNION SELECT 8 UNION SELECT 9 UNION SELECT 10) AS t2
) row_num
WHERE seq.seq <= 4 AND row_num.row_num <= 10

-- First class seats (3.0x multiplier)
INSERT INTO [flights].[AirplaneSeats] ([AirplaneId], [SeatNumber], [SeatClass], [PriceMultiplier])
SELECT @airplaneId, CHAR(ASCII('A') + seq.seq - 1) + CAST(row_num.row_num AS VARCHAR), 2, 3.0
FROM (
    SELECT ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS seq
    FROM (SELECT 1 UNION SELECT 2) AS t1
) seq
CROSS JOIN (
    SELECT ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS row_num
    FROM (SELECT 1 UNION SELECT 2 UNION SELECT 3 UNION SELECT 4 UNION SELECT 5) AS t2
) row_num
WHERE seq.seq <= 2 AND row_num.row_num <= 5
GO

-- Insert sample routes
DECLARE @airlineId UNIQUEIDENTIFIER, @sgn UNIQUEIDENTIFIER, @han UNIQUEIDENTIFIER

SELECT TOP 1 @airlineId = [Id] FROM [flights].[Airlines] WHERE [IataCode] = 'VN'
SELECT TOP 1 @sgn = [Id] FROM [flights].[Airports] WHERE [IataCode] = 'SGN'
SELECT TOP 1 @han = [Id] FROM [flights].[Airports] WHERE [IataCode] = 'HAN'

INSERT INTO [flights].[Routes] ([AirlineId], [OriginAirportId], [DestinationAirportId])
VALUES
    (@airlineId, @sgn, @han),
    (@airlineId, @han, @sgn)
GO

-- Insert sample admin user
INSERT INTO [users].[Users] ([Email], [PasswordHash], [FullName], [Phone], [Role], [IsActive])
VALUES
    ('admin@airlineticket.com', 'hashed_password_here', 'Administrator', '0901234567', 1, 1)
GO

-- Insert sample customer users
INSERT INTO [users].[Users] ([Email], [PasswordHash], [FullName], [Phone], [Role], [IsActive])
VALUES
    ('customer1@example.com', 'hashed_password_here', 'John Doe', '0912345678', 0, 1),
    ('customer2@example.com', 'hashed_password_here', 'Jane Smith', '0923456789', 0, 1)
GO

-- Insert sample coupons
INSERT INTO [promotions].[Coupons] ([Code], [DiscountType], [DiscountValue], [MaxDiscountAmount], [MaxUsages], [ValidFrom], [ValidTo], [IsActive])
VALUES
    ('SUMMER20', 0, 20, 500000, 1000, GETUTCDATE(), DATEADD(DAY, 90, GETUTCDATE()), 1),
    ('FIXED100K', 1, 100000, NULL, 500, GETUTCDATE(), DATEADD(DAY, 30, GETUTCDATE()), 1)
GO

PRINT 'Database initialization completed successfully!'
