-- =====================================================
-- AIRLINE TICKET SYSTEM - COMPLETE DATABASE DESIGN (v2)
-- Designed by GitHub Copilot
-- =====================================================

USE [master]
GO

-- =====================================================
-- 1. CREATE SCHEMAS (Modular Monolith Approach)
-- =====================================================
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'identity') EXEC sp_executesql N'CREATE SCHEMA identity'
GO
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'flights') EXEC sp_executesql N'CREATE SCHEMA flights'
GO
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'bookings') EXEC sp_executesql N'CREATE SCHEMA bookings'
GO
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'promotions') EXEC sp_executesql N'CREATE SCHEMA promotions'
GO
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'interactions') EXEC sp_executesql N'CREATE SCHEMA interactions'
GO
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'cms') EXEC sp_executesql N'CREATE SCHEMA cms'
GO

-- =====================================================
-- 2. IDENTITY SCHEMA - Authentication & Authorization
-- =====================================================

CREATE TABLE [identity].[Roles] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [Name] NVARCHAR(50) NOT NULL UNIQUE,
    [Description] NVARCHAR(255) NULL
)
GO

CREATE TABLE [identity].[Users] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [Email] NVARCHAR(255) NOT NULL UNIQUE,
    [EmailConfirmed] BIT NOT NULL DEFAULT 0,
    [PasswordHash] NVARCHAR(MAX) NOT NULL,
    [FullName] NVARCHAR(255) NOT NULL,
    [PhoneNumber] NVARCHAR(20) NULL,
    [AvatarUrl] NVARCHAR(500) NULL,
    [LanguagePreference] NVARCHAR(10) DEFAULT 'vi',
    [LastLoginAt] DATETIME2 NULL,
    [IsActive] BIT NOT NULL DEFAULT 1,
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2 DEFAULT GETUTCDATE()
)
GO

CREATE TABLE [identity].[UserRoles] (
    [UserId] UNIQUEIDENTIFIER NOT NULL,
    [RoleId] UNIQUEIDENTIFIER NOT NULL,
    PRIMARY KEY ([UserId], [RoleId]),
    CONSTRAINT [FK_UserRoles_Users] FOREIGN KEY ([UserId]) REFERENCES [identity].[Users]([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_UserRoles_Roles] FOREIGN KEY ([RoleId]) REFERENCES [identity].[Roles]([Id]) ON DELETE CASCADE
)
GO

-- =====================================================
-- 3. FLIGHTS SCHEMA - Core Domain
-- =====================================================

CREATE TABLE [flights].[Airlines] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [IataCode] NVARCHAR(10) NOT NULL UNIQUE,
    [Name] NVARCHAR(255) NOT NULL,
    [LogoUrl] NVARCHAR(500) NULL,
    [BaseCountry] NVARCHAR(100) NULL,
    [ApiEndpoint] NVARCHAR(500) NULL, -- B2B Partner Integration Endpoint
    [ApiKey] NVARCHAR(255) NULL,
    [IsActive] BIT NOT NULL DEFAULT 1,
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE()
)
GO

CREATE TABLE [flights].[Airports] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [IataCode] NVARCHAR(10) NOT NULL UNIQUE,
    [NameEn] NVARCHAR(255) NOT NULL,
    [NameVi] NVARCHAR(255) NOT NULL,
    [CityEn] NVARCHAR(100) NOT NULL,
    [CityVi] NVARCHAR(100) NOT NULL,
    [CountryCode] NVARCHAR(10) NOT NULL,
    [Timezone] NVARCHAR(50) NOT NULL,
    [Latitude] DECIMAL(9, 6) NULL,
    [Longitude] DECIMAL(9, 6) NULL,
    [IsActive] BIT NOT NULL DEFAULT 1
)
GO

CREATE TABLE [flights].[Airplanes] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [AirlineId] UNIQUEIDENTIFIER NOT NULL,
    [Model] NVARCHAR(100) NOT NULL,
    [RegistrationNumber] NVARCHAR(50) NOT NULL UNIQUE,
    [TotalCapacity] INT NOT NULL,
    CONSTRAINT [FK_Airplanes_Airlines] FOREIGN KEY ([AirlineId]) REFERENCES [flights].[Airlines]([Id])
)
GO

CREATE TABLE [flights].[Routes] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [AirlineId] UNIQUEIDENTIFIER NOT NULL,
    [OriginAirportId] UNIQUEIDENTIFIER NOT NULL,
    [DestinationAirportId] UNIQUEIDENTIFIER NOT NULL,
    [DistanceKm] DECIMAL(10, 2) NULL,
    [EstimatedDurationMinutes] INT NULL,
    CONSTRAINT [FK_Routes_Airlines] FOREIGN KEY ([AirlineId]) REFERENCES [flights].[Airlines]([Id]),
    CONSTRAINT [FK_Routes_Origin] FOREIGN KEY ([OriginAirportId]) REFERENCES [flights].[Airports]([Id]),
    CONSTRAINT [FK_Routes_Destination] FOREIGN KEY ([DestinationAirportId]) REFERENCES [flights].[Airports]([Id])
)
GO

CREATE TABLE [flights].[Flights] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [RouteId] UNIQUEIDENTIFIER NOT NULL,
    [AirplaneId] UNIQUEIDENTIFIER NOT NULL,
    [FlightNumber] NVARCHAR(20) NOT NULL,
    [DepartureTime] DATETIME2 NOT NULL,
    [ArrivalTime] DATETIME2 NOT NULL,
    [BasePrice] DECIMAL(18, 2) NOT NULL,
    [Currency] NVARCHAR(3) DEFAULT 'USD',
    [Status] INT NOT NULL DEFAULT 0, -- 0: Scheduled, 1: Delayed, 2: Boarding, 3: InAir, 4: Landed, 5: Cancelled
    [ExternalId] NVARCHAR(100) NULL,
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    CONSTRAINT [FK_Flights_Routes] FOREIGN KEY ([RouteId]) REFERENCES [flights].[Routes]([Id]),
    CONSTRAINT [FK_Flights_Airplanes] FOREIGN KEY ([AirplaneId]) REFERENCES [flights].[Airplanes]([Id])
)
GO

CREATE TABLE [flights].[FlightSeats] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [FlightId] UNIQUEIDENTIFIER NOT NULL,
    [SeatNumber] NVARCHAR(10) NOT NULL,
    [SeatClass] INT NOT NULL, -- 0: Economy, 1: PremiumEconomy, 2: Business, 3: FirstClass
    [PriceOverride] DECIMAL(18, 2) NULL,
    [IsAvailable] BIT NOT NULL DEFAULT 1,
    [IsExtraLegroom] BIT NOT NULL DEFAULT 0,
    CONSTRAINT [FK_FlightSeats_Flights] FOREIGN KEY ([FlightId]) REFERENCES [flights].[Flights]([Id]),
    CONSTRAINT [UC_FlightSeat] UNIQUE ([FlightId], [SeatNumber])
)
GO

-- =====================================================
-- 4. BOOKINGS SCHEMA - Transactional Data
-- =====================================================

CREATE TABLE [bookings].[Bookings] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [UserId] UNIQUEIDENTIFIER NOT NULL,
    [PnrCode] NVARCHAR(10) NOT NULL UNIQUE,
    [TotalPrice] DECIMAL(18, 2) NOT NULL,
    [Currency] NVARCHAR(3) DEFAULT 'USD',
    [Status] INT NOT NULL DEFAULT 0, -- 0: Pending, 1: Paid, 2: Confirmed, 3: Cancelled, 4: Refunded
    [ContactEmail] NVARCHAR(255) NOT NULL,
    [ContactPhone] NVARCHAR(20) NOT NULL,
    [SpecialRequests] NVARCHAR(MAX) NULL,
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    CONSTRAINT [FK_Bookings_Users] FOREIGN KEY ([UserId]) REFERENCES [identity].[Users]([Id])
)
GO

CREATE TABLE [bookings].[Passengers] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [BookingId] UNIQUEIDENTIFIER NOT NULL,
    [FirstName] NVARCHAR(100) NOT NULL,
    [LastName] NVARCHAR(100) NOT NULL,
    [Gender] INT NULL, -- 0: Male, 1: Female, 2: Other
    [DateOfBirth] DATE NOT NULL,
    [Nationality] NVARCHAR(100) NULL,
    [PassportNumber] NVARCHAR(50) NOT NULL,
    [PassportExpiryDate] DATE NOT NULL,
    CONSTRAINT [FK_Passengers_Bookings] FOREIGN KEY ([BookingId]) REFERENCES [bookings].[Bookings]([Id]) ON DELETE CASCADE
)
GO

CREATE TABLE [bookings].[Tickets] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [BookingId] UNIQUEIDENTIFIER NOT NULL,
    [PassengerId] UNIQUEIDENTIFIER NOT NULL,
    [FlightId] UNIQUEIDENTIFIER NOT NULL,
    [SeatId] UNIQUEIDENTIFIER NOT NULL,
    [TicketNumber] NVARCHAR(50) NOT NULL UNIQUE,
    [Gate] NVARCHAR(20) NULL,
    [BoardingTime] DATETIME2 NULL,
    [Status] INT NOT NULL DEFAULT 0, -- 0: Valid, 1: CheckedIn, 2: Used, 3: Cancelled
    CONSTRAINT [FK_Tickets_Bookings] FOREIGN KEY ([BookingId]) REFERENCES [bookings].[Bookings]([Id]),
    CONSTRAINT [FK_Tickets_Passengers] FOREIGN KEY ([PassengerId]) REFERENCES [bookings].[Passengers]([Id]),
    CONSTRAINT [FK_Tickets_Flights] FOREIGN KEY ([FlightId]) REFERENCES [flights].[Flights]([Id]),
    CONSTRAINT [FK_Tickets_Seats] FOREIGN KEY ([SeatId]) REFERENCES [flights].[FlightSeats]([Id])
)
GO

CREATE TABLE [bookings].[Payments] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [BookingId] UNIQUEIDENTIFIER NOT NULL,
    [TransactionId] NVARCHAR(100) NOT NULL UNIQUE,
    [Amount] DECIMAL(18, 2) NOT NULL,
    [PaymentMethod] NVARCHAR(50) NOT NULL,
    [ProviderStatus] NVARCHAR(50) NULL,
    [IsSuccessful] BIT NOT NULL DEFAULT 0,
    [RawResponse] NVARCHAR(MAX) NULL,
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    CONSTRAINT [FK_Payments_Bookings] FOREIGN KEY ([BookingId]) REFERENCES [bookings].[Bookings]([Id])
)
GO

-- =====================================================
-- 5. PROMOTIONS SCHEMA
-- =====================================================

CREATE TABLE [promotions].[Coupons] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [Code] NVARCHAR(50) NOT NULL UNIQUE,
    [Description] NVARCHAR(500) NULL,
    [DiscountType] INT NOT NULL, -- 0: Percentage, 1: FixedAmount
    [DiscountValue] DECIMAL(18, 2) NOT NULL,
    [MinOrderValue] DECIMAL(18, 2) NULL,
    [MaxDiscountAmount] DECIMAL(18, 2) NULL,
    [StartDate] DATETIME2 NOT NULL,
    [EndDate] DATETIME2 NOT NULL,
    [UsageLimit] INT NULL,
    [UsageCount] INT DEFAULT 0,
    [IsActive] BIT DEFAULT 1
)
GO

CREATE TABLE [promotions].[Campaigns] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [Title] NVARCHAR(255) NOT NULL,
    [BannerUrl] NVARCHAR(500) NULL,
    [Content] NVARCHAR(MAX) NULL,
    [StartDate] DATETIME2 NOT NULL,
    [EndDate] DATETIME2 NOT NULL,
    [IsFeatured] BIT DEFAULT 0
)
GO

-- =====================================================
-- 6. INTERACTIONS SCHEMA (Reviews & Feedback)
-- =====================================================

CREATE TABLE [interactions].[Reviews] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [UserId] UNIQUEIDENTIFIER NOT NULL,
    [AirlineId] UNIQUEIDENTIFIER NULL,
    [FlightId] UNIQUEIDENTIFIER NULL,
    [Rating] INT NOT NULL CHECK ([Rating] >= 1 AND [Rating] <= 5),
    [Comment] NVARCHAR(MAX) NULL,
    [IsVerifiedPurchase] BIT DEFAULT 0,
    [IsHidden] BIT DEFAULT 0,
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    CONSTRAINT [FK_Reviews_Users] FOREIGN KEY ([UserId]) REFERENCES [identity].[Users]([Id]),
    CONSTRAINT [FK_Reviews_Airlines] FOREIGN KEY ([AirlineId]) REFERENCES [flights].[Airlines]([Id]),
    CONSTRAINT [FK_Reviews_Flights] FOREIGN KEY ([FlightId]) REFERENCES [flights].[Flights]([Id])
)
GO

-- =====================================================
-- 7. CMS SCHEMA (Articles & Content)
-- =====================================================

CREATE TABLE [cms].[Categories] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [Name] NVARCHAR(100) NOT NULL,
    [Slug] NVARCHAR(100) NOT NULL UNIQUE
)
GO

CREATE TABLE [cms].[Articles] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [CategoryId] UNIQUEIDENTIFIER NOT NULL,
    [AuthorId] UNIQUEIDENTIFIER NOT NULL,
    [Title] NVARCHAR(255) NOT NULL,
    [Slug] NVARCHAR(255) NOT NULL UNIQUE,
    [Summary] NVARCHAR(500) NULL,
    [Content] NVARCHAR(MAX) NOT NULL,
    [ThumbnailUrl] NVARCHAR(500) NULL,
    [PublishedAt] DATETIME2 NULL,
    [Status] INT NOT NULL DEFAULT 0, -- 0: Draft, 1: Published, 2: Archived
    [ViewCount] INT DEFAULT 0,
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    CONSTRAINT [FK_Articles_Categories] FOREIGN KEY ([CategoryId]) REFERENCES [cms].[Categories]([Id]),
    CONSTRAINT [FK_Articles_Users] FOREIGN KEY ([AuthorId]) REFERENCES [identity].[Users]([Id])
)
GO

-- =====================================================
-- 8. INDEXES FOR PERFORMANCE
-- =====================================================
CREATE INDEX [IX_Flights_Departure] ON [flights].[Flights]([DepartureTime])
CREATE INDEX [IX_Flights_Route] ON [flights].[Flights]([RouteId])
CREATE INDEX [IX_Bookings_Pnr] ON [bookings].[Bookings]([PnrCode])
CREATE INDEX [IX_Bookings_User] ON [bookings].[Bookings]([UserId])
CREATE INDEX [IX_Tickets_Number] ON [bookings].[Tickets]([TicketNumber])
GO

-- =====================================================
-- 9. SEED INITIAL DATA
-- =====================================================
INSERT INTO [identity].[Roles] ([Name], [Description]) VALUES 
('Admin', 'System Administrator'),
('Partner', 'Airline Partner Staff'),
('Customer', 'Regular Passenger')
GO

-- Sample Admin
INSERT INTO [identity].[Users] ([Email], [PasswordHash], [FullName], [IsActive]) 
VALUES ('admin@airticket.com', 'AQAAAAEAACcQAAAAEP...', 'System Admin', 1)
GO

-- Assign Admin Role
DECLARE @AdminId UNIQUEIDENTIFIER = (SELECT Id FROM [identity].[Users] WHERE Email = 'admin@airticket.com')
DECLARE @RoleId UNIQUEIDENTIFIER = (SELECT Id FROM [identity].[Roles] WHERE Name = 'Admin')
INSERT INTO [identity].[UserRoles] (UserId, RoleId) VALUES (@AdminId, @RoleId)
GO
