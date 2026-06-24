
-- =====================================================
-- AIRLINE TICKET SYSTEM - EXTRA SEED DATA
-- =====================================================

USE [AirlineTicketDb]
GO

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
-- I'll hardcode some and use NEWID() for variations.

WHILE @Count < 100
BEGIN
    SET @Count = @Count + 1;
    
    -- Pick a random airline (simplified, I'll pick one of the 8)
    -- This is a bit complex for a simple script, I will just hardcode combinations
    
    -- Let's pick a random route and airplane
    SELECT TOP 1 @RouteId = [Id], @AirlineId = [AirlineId], @OriginAirportId = [OriginAirportId], @DestinationAirportId = [DestinationAirportId] FROM [dbo].[Routes] ORDER BY NEWID();
    SELECT TOP 1 @AirplaneId = [Id] FROM [dbo].[Airplanes] WHERE [AirlineId] = @AirlineId ORDER BY NEWID();
    
    -- If no airplane for this airline, pick any
    IF @AirplaneId IS NULL
        SELECT TOP 1 @AirplaneId = [Id] FROM [dbo].[Airplanes] ORDER BY NEWID();

    SET @FlightNumber = 'FL' + CAST(1000 + @Count AS NVARCHAR(10));
    SET @DepartureTime = DATEADD(hour, @Count, GETUTCDATE());
    SET @ArrivalTime = DATEADD(hour, 3, @DepartureTime);

    DECLARE @FlightId UNIQUEIDENTIFIER = NEWID();

    INSERT INTO [dbo].[Flights] ([Id], [RouteId], [AirplaneId], [FlightNumber], [DepartureTime], [ArrivalTime], [BasePrice], [Currency], [Status], [IsDeleted], [CreatedAt], [UpdatedAt])
    VALUES (@FlightId, @RouteId, @AirplaneId, @FlightNumber, @DepartureTime, @ArrivalTime, 100.00, 'USD', 0, 0, GETUTCDATE(), GETUTCDATE());

    -- 2. Generate 10-20 seats for this flight
    DECLARE @SeatCount INT = 0;
    DECLARE @TotalSeats INT = 10 + (ABS(CHECKSUM(NEWID())) % 11);
    
    WHILE @SeatCount < @TotalSeats
    BEGIN
        SET @SeatCount = @SeatCount + 1;
        INSERT INTO [dbo].[FlightSeats] ([Id], [FlightId], [SeatNumber], [SeatClass], [PriceOverride], [IsAvailable], [IsExtraLegroom])
        VALUES (NEWID(), @FlightId, CAST(@SeatCount AS NVARCHAR(5)) + 'A', 0, NULL, 1, 0);
    END
END
GO

-- 3. 50 new Bookings with corresponding Passengers, Tickets, and Payments
DECLARE @BookingCount INT = 0;
DECLARE @UserId UNIQUEIDENTIFIER;
DECLARE @FlightId UNIQUEIDENTIFIER;
DECLARE @SeatId UNIQUEIDENTIFIER;
DECLARE @BookingId UNIQUEIDENTIFIER;
DECLARE @PassengerId UNIQUEIDENTIFIER;

SELECT TOP 1 @UserId = [Id] FROM [dbo].[Users] WHERE [Email] = 'client@gmail.com';

WHILE @BookingCount < 50
BEGIN
    SET @BookingCount = @BookingCount + 1;
    
    SELECT TOP 1 @FlightId = [Id] FROM [dbo].[Flights] ORDER BY NEWID();
    SELECT TOP 1 @SeatId = [Id] FROM [dbo].[FlightSeats] WHERE [FlightId] = @FlightId AND [IsAvailable] = 1 ORDER BY NEWID();
    
    IF @SeatId IS NOT NULL
    BEGIN
        SET @BookingId = NEWID();
        SET @PassengerId = NEWID();
        
        INSERT INTO [dbo].[Bookings] ([Id], [UserId], [PnrCode], [TotalPrice], [Currency], [Status], [ContactEmail], [ContactPhone], [SpecialRequests], [IsDeleted], [CreatedAt], [UpdatedAt])
        VALUES (@BookingId, @UserId, 'PNR' + CAST(1000 + @BookingCount AS NVARCHAR(10)), 100.00, 'USD', 1, 'client@gmail.com', '0987654321', NULL, 0, GETUTCDATE(), GETUTCDATE());
        
        INSERT INTO [dbo].[Passengers] ([Id], [BookingId], [FirstName], [LastName], [Gender], [DateOfBirth], [Nationality], [PassportNumber], [PassportExpiryDate])
        VALUES (@PassengerId, @BookingId, 'Passenger', CAST(@BookingCount AS NVARCHAR(10)), 0, '1990-01-01', 'VN', 'PS' + CAST(@BookingCount AS NVARCHAR(10)), '2030-01-01');
        
        INSERT INTO [dbo].[Tickets] ([Id], [BookingId], [PassengerId], [FlightId], [SeatId], [TicketNumber], [Gate], [BoardingTime], [Status])
        VALUES (NEWID(), @BookingId, @PassengerId, @FlightId, @SeatId, 'TK' + CAST(1000 + @BookingCount AS NVARCHAR(10)), 'Gate A', DATEADD(hour, -1, GETUTCDATE()), 0);
        
        INSERT INTO [dbo].[Payments] ([Id], [BookingId], [TransactionId], [Amount], [PaymentMethod], [ProviderStatus], [IsSuccessful], [RawResponse], [CreatedAt])
        VALUES (NEWID(), @BookingId, 'TXN' + CAST(1000 + @BookingCount AS NVARCHAR(10)), 100.00, 'CreditCard', 'Success', 1, NULL, GETUTCDATE());
        
        UPDATE [dbo].[FlightSeats] SET [IsAvailable] = 0 WHERE [Id] = @SeatId;
    END
END
GO
