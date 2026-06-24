USE [AirlineTicketDb]
GO

IF DB_ID('AirlineTicketDb') IS NOT NULL
BEGIN
    USE [AirlineTicketDb]
END
GO

PRINT '==========================================================='
PRINT ' SEED SCRIPT: Routes and Flights'
PRINT '==========================================================='

SET NOCOUNT ON;

-- Variables
DECLARE @HubSGN UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM [dbo].[Airports] WHERE IataCode = 'SGN');
DECLARE @HubHAN UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM [dbo].[Airports] WHERE IataCode = 'HAN');

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
SELECT @HubSGN, Id FROM [dbo].[Airports] WHERE Id <> @HubSGN;

INSERT INTO #RoutesToInsert (OriginAirportId, DestinationAirportId)
SELECT Id, @HubSGN FROM [dbo].[Airports] WHERE Id <> @HubSGN;

-- Generate Routes to/from HAN for all airports (except HAN itself)
INSERT INTO #RoutesToInsert (OriginAirportId, DestinationAirportId)
SELECT @HubHAN, Id FROM [dbo].[Airports] WHERE Id <> @HubHAN;

INSERT INTO #RoutesToInsert (OriginAirportId, DestinationAirportId)
SELECT Id, @HubHAN FROM [dbo].[Airports] WHERE Id <> @HubHAN;

PRINT 'Generating Routes...'

-- Insert routes that don't exist yet
INSERT INTO [dbo].[Routes] (
    Id, AirlineId, OriginAirportId, DestinationAirportId, DistanceKm, EstimatedDurationMinutes, IsDeleted
)
SELECT 
    NEWID(),
    (SELECT TOP 1 Id FROM [dbo].[Airlines] ORDER BY NEWID()), -- Random Airline
    r.OriginAirportId,
    r.DestinationAirportId,
    ABS(CHECKSUM(NEWID())) % 2000 + 500 AS DistanceKm, -- Random Distance between 500 and 2500
    ABS(CHECKSUM(NEWID())) % 180 + 60 AS EstimatedDurationMinutes, -- Random Duration between 60 and 240 mins
    0
FROM #RoutesToInsert r
WHERE NOT EXISTS (
    SELECT 1 FROM [dbo].[Routes] existing 
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
FROM [dbo].[Routes] r
JOIN [dbo].[Airports] a ON r.OriginAirportId = a.Id;

OPEN route_cursor;
FETCH NEXT FROM route_cursor INTO @RouteId, @AirlineId, @OriginIata, @EstimatedDurationMinutes;

WHILE @@FETCH_STATUS = 0
BEGIN
    -- Check how many flights already exist for this route
    SELECT @FlightCount = COUNT(*) FROM [dbo].[Flights] WHERE RouteId = @RouteId AND IsDeleted = 0;
    
    SET @I = @FlightCount;
    WHILE @I < 5
    BEGIN
        DECLARE @NewFlightId UNIQUEIDENTIFIER = NEWID();
        DECLARE @AirplaneId UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM [dbo].[Airplanes] WHERE AirlineId = @AirlineId ORDER BY NEWID());
        
        -- Fallback if airline has no airplane, just pick any
        IF @AirplaneId IS NULL
        BEGIN
            SET @AirplaneId = (SELECT TOP 1 Id FROM [dbo].[Airplanes] ORDER BY NEWID());
        END

        IF @AirplaneId IS NOT NULL
        BEGIN
            -- Generate flight departure time randomly within next 30 days
            DECLARE @DaysToAdd INT = ABS(CHECKSUM(NEWID())) % 30;
            DECLARE @HoursToAdd INT = ABS(CHECKSUM(NEWID())) % 24;
            DECLARE @MinutesToAdd INT = (ABS(CHECKSUM(NEWID())) % 12) * 5; -- Multiple of 5
            
            DECLARE @DepartureTime DATETIME2 = DATEADD(MINUTE, @MinutesToAdd, DATEADD(HOUR, @HoursToAdd, DATEADD(DAY, @DaysToAdd, GETUTCDATE())));
            DECLARE @ArrivalTime DATETIME2 = DATEADD(MINUTE, @EstimatedDurationMinutes, @DepartureTime);
            DECLARE @BasePrice DECIMAL(18,2) = CAST((ABS(CHECKSUM(NEWID())) % 751 + 50) AS DECIMAL(18,2)); -- Random between 50 and 800
            
            -- Insert Flight
            INSERT INTO [dbo].[Flights] (
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
                NEWID(),
                0,
                GETUTCDATE(),
                GETUTCDATE()
            );

            -- Insert 10 FlightSeats for this flight
            DECLARE @SeatIndex INT = 1;
            WHILE @SeatIndex <= 10
            BEGIN
                INSERT INTO [dbo].[FlightSeats] (
                    Id, FlightId, SeatNumber, SeatClass, PriceOverride, IsAvailable, IsExtraLegroom
                )
                VALUES (
                    NEWID(),
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
GO