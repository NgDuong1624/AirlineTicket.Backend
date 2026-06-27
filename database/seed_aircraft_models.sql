USE [AirlineTicketDb]
GO

PRINT '==========================================================='
PRINT ' SEED SCRIPT: Aircraft Models and Seat Templates'
PRINT '==========================================================='

SET NOCOUNT ON;

-- 1. Define Aircraft Model IDs
DECLARE @ModelB787 UNIQUEIDENTIFIER = 'B7879000-BCDE-4F01-2345-6789ABCDEF01';
DECLARE @ModelA350 UNIQUEIDENTIFIER = 'A3509000-BCDE-4F01-2345-6789ABCDEF02';
DECLARE @ModelA321 UNIQUEIDENTIFIER = 'A3212000-BCDE-4F01-2345-6789ABCDEF03';

-- 2. Insert Aircraft Models
IF NOT EXISTS (SELECT 1 FROM [dbo].[AircraftModels] WHERE Id = @ModelB787)
BEGIN
    INSERT INTO [dbo].[AircraftModels] ([Id], [Name], [Manufacturer], [TotalSeats], [IsDeleted])
    VALUES (@ModelB787, 'Boeing 787-9 Dreamliner', 'Boeing', 294, 0);
END

IF NOT EXISTS (SELECT 1 FROM [dbo].[AircraftModels] WHERE Id = @ModelA350)
BEGIN
    INSERT INTO [dbo].[AircraftModels] ([Id], [Name], [Manufacturer], [TotalSeats], [IsDeleted])
    VALUES (@ModelA350, 'Airbus A350-900', 'Airbus', 305, 0);
END

IF NOT EXISTS (SELECT 1 FROM [dbo].[AircraftModels] WHERE Id = @ModelA321)
BEGIN
    INSERT INTO [dbo].[AircraftModels] ([Id], [Name], [Manufacturer], [TotalSeats], [IsDeleted])
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
    INSERT INTO [dbo].[AircraftModelSeatTemplates] ([Id], [AircraftModelId], [SeatNumber], [SeatRow], [SeatColumn], [SeatClass], [IsExtraLegroom], [PriceMultiplier])
    SELECT 
        NEWID(), 
        @ModelB787, 
        CAST(@Row AS VARCHAR(2)) + Col, 
        CAST(@Row AS VARCHAR(2)), 
        Col, 
        2, -- Business
        1, -- Extra legroom for Business
        2.5 -- 2.5x price
    FROM #Cols WHERE Col IN ('A', 'D', 'G', 'K')
    AND NOT EXISTS (SELECT 1 FROM [dbo].[AircraftModelSeatTemplates] WHERE AircraftModelId = @ModelB787 AND SeatNumber = CAST(@Row AS VARCHAR(2)) + Col);
    
    SET @Row = @Row + 1;
END

-- Premium Economy: Rows 10-15, 2-3-2 layout (A, C, D, F, G, H, K)
SET @Row = 10;
WHILE @Row <= 15
BEGIN
    INSERT INTO [dbo].[AircraftModelSeatTemplates] ([Id], [AircraftModelId], [SeatNumber], [SeatRow], [SeatColumn], [SeatClass], [IsExtraLegroom], [PriceMultiplier])
    SELECT 
        NEWID(), 
        @ModelB787, 
        CAST(@Row AS VARCHAR(2)) + Col, 
        CAST(@Row AS VARCHAR(2)), 
        Col, 
        1, -- Premium Economy
        0, 
        1.5 -- 1.5x price
    FROM #Cols WHERE Col IN ('A', 'C', 'D', 'F', 'G', 'H', 'K')
    AND NOT EXISTS (SELECT 1 FROM [dbo].[AircraftModelSeatTemplates] WHERE AircraftModelId = @ModelB787 AND SeatNumber = CAST(@Row AS VARCHAR(2)) + Col);
    
    SET @Row = @Row + 1;
END

-- Economy: Rows 20-45, 3-3-3 layout (A, B, C, D, E, F, G, H, K)
SET @Row = 20;
WHILE @Row <= 45
BEGIN
    INSERT INTO [dbo].[AircraftModelSeatTemplates] ([Id], [AircraftModelId], [SeatNumber], [SeatRow], [SeatColumn], [SeatClass], [IsExtraLegroom], [PriceMultiplier])
    SELECT 
        NEWID(), 
        @ModelB787, 
        CAST(@Row AS VARCHAR(2)) + Col, 
        CAST(@Row AS VARCHAR(2)), 
        Col, 
        0, -- Economy
        CASE WHEN @Row = 20 THEN 1 ELSE 0 END, -- Row 20 has extra legroom
        CASE WHEN @Row = 20 THEN 1.2 ELSE 1.0 END
    FROM #Cols WHERE Col IN ('A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'K')
    AND NOT EXISTS (SELECT 1 FROM [dbo].[AircraftModelSeatTemplates] WHERE AircraftModelId = @ModelB787 AND SeatNumber = CAST(@Row AS VARCHAR(2)) + Col);
    
    SET @Row = @Row + 1;
END


-- --- Airbus A350-900 Seat Template ---
-- Business Class: Rows 1-6, 1-2-1 layout (A, D, G, K)
SET @Row = 1;
WHILE @Row <= 6
BEGIN
    INSERT INTO [dbo].[AircraftModelSeatTemplates] ([Id], [AircraftModelId], [SeatNumber], [SeatRow], [SeatColumn], [SeatClass], [IsExtraLegroom], [PriceMultiplier])
    SELECT 
        NEWID(), 
        @ModelA350, 
        CAST(@Row AS VARCHAR(2)) + Col, 
        CAST(@Row AS VARCHAR(2)), 
        Col, 
        2, -- Business
        1, 
        2.5
    FROM #Cols WHERE Col IN ('A', 'D', 'G', 'K')
    AND NOT EXISTS (SELECT 1 FROM [dbo].[AircraftModelSeatTemplates] WHERE AircraftModelId = @ModelA350 AND SeatNumber = CAST(@Row AS VARCHAR(2)) + Col);
    
    SET @Row = @Row + 1;
END

-- Premium Economy: Rows 10-16, 2-4-2 layout (A, C, D, E, F, G, H, K)
SET @Row = 10;
WHILE @Row <= 16
BEGIN
    INSERT INTO [dbo].[AircraftModelSeatTemplates] ([Id], [AircraftModelId], [SeatNumber], [SeatRow], [SeatColumn], [SeatClass], [IsExtraLegroom], [PriceMultiplier])
    SELECT 
        NEWID(), 
        @ModelA350, 
        CAST(@Row AS VARCHAR(2)) + Col, 
        CAST(@Row AS VARCHAR(2)), 
        Col, 
        1, -- Premium Economy
        0, 
        1.5
    FROM #Cols WHERE Col IN ('A', 'C', 'D', 'E', 'F', 'G', 'H', 'K')
    AND NOT EXISTS (SELECT 1 FROM [dbo].[AircraftModelSeatTemplates] WHERE AircraftModelId = @ModelA350 AND SeatNumber = CAST(@Row AS VARCHAR(2)) + Col);
    
    SET @Row = @Row + 1;
END

-- Economy: Rows 20-46, 3-3-3 layout (A, B, C, D, E, F, G, H, K)
SET @Row = 20;
WHILE @Row <= 46
BEGIN
    INSERT INTO [dbo].[AircraftModelSeatTemplates] ([Id], [AircraftModelId], [SeatNumber], [SeatRow], [SeatColumn], [SeatClass], [IsExtraLegroom], [PriceMultiplier])
    SELECT 
        NEWID(), 
        @ModelA350, 
        CAST(@Row AS VARCHAR(2)) + Col, 
        CAST(@Row AS VARCHAR(2)), 
        Col, 
        0, -- Economy
        CASE WHEN @Row = 20 THEN 1 ELSE 0 END, 
        CASE WHEN @Row = 20 THEN 1.2 ELSE 1.0 END
    FROM #Cols WHERE Col IN ('A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'K')
    AND NOT EXISTS (SELECT 1 FROM [dbo].[AircraftModelSeatTemplates] WHERE AircraftModelId = @ModelA350 AND SeatNumber = CAST(@Row AS VARCHAR(2)) + Col);
    
    SET @Row = @Row + 1;
END


-- --- Airbus A321neo Seat Template ---
-- Business Class: Rows 1-2, 2-2 layout (A, C, H, K)
SET @Row = 1;
WHILE @Row <= 2
BEGIN
    INSERT INTO [dbo].[AircraftModelSeatTemplates] ([Id], [AircraftModelId], [SeatNumber], [SeatRow], [SeatColumn], [SeatClass], [IsExtraLegroom], [PriceMultiplier])
    SELECT 
        NEWID(), 
        @ModelA321, 
        CAST(@Row AS VARCHAR(2)) + Col, 
        CAST(@Row AS VARCHAR(2)), 
        Col, 
        2, -- Business
        1, 
        2.0
    FROM #Cols WHERE Col IN ('A', 'C', 'H', 'K')
    AND NOT EXISTS (SELECT 1 FROM [dbo].[AircraftModelSeatTemplates] WHERE AircraftModelId = @ModelA321 AND SeatNumber = CAST(@Row AS VARCHAR(2)) + Col);
    
    SET @Row = @Row + 1;
END

-- Economy: Rows 3-38, 3-3 layout (A, B, C, H, J, K)
SET @Row = 3;
WHILE @Row <= 38
BEGIN
    INSERT INTO [dbo].[AircraftModelSeatTemplates] ([Id], [AircraftModelId], [SeatNumber], [SeatRow], [SeatColumn], [SeatClass], [IsExtraLegroom], [PriceMultiplier])
    SELECT 
        NEWID(), 
        @ModelA321, 
        CAST(@Row AS VARCHAR(2)) + Col, 
        CAST(@Row AS VARCHAR(2)), 
        Col, 
        0, -- Economy
        CASE WHEN @Row IN (11, 12) THEN 1 ELSE 0 END, -- Exit rows
        CASE WHEN @Row IN (11, 12) THEN 1.2 ELSE 1.0 END
    FROM #Cols WHERE Col IN ('A', 'B', 'C', 'G', 'H', 'K') -- Map G to J for simplicity
    AND NOT EXISTS (SELECT 1 FROM [dbo].[AircraftModelSeatTemplates] WHERE AircraftModelId = @ModelA321 AND SeatNumber = CAST(@Row AS VARCHAR(2)) + Col);
    
    SET @Row = @Row + 1;
END

-- 4. Update existing Airplanes to link to Aircraft Models
PRINT 'Linking existing Airplanes to Aircraft Models...'

UPDATE [dbo].[Airplanes]
SET [AircraftModelId] = @ModelB787
WHERE [Id] IN ('81F3E2D4-BCDE-4F01-2345-6789ABCDEF01', '84F3E2D4-BCDE-4F01-2345-6789ABCDEF04');

UPDATE [dbo].[Airplanes]
SET [AircraftModelId] = @ModelA350
WHERE [Id] = '82F3E2D4-BCDE-4F01-2345-6789ABCDEF02';

UPDATE [dbo].[Airplanes]
SET [AircraftModelId] = @ModelA321
WHERE [Id] = '83F3E2D4-BCDE-4F01-2345-6789ABCDEF03';

-- 5. Generate AirplaneSeats for existing Airplanes from templates
PRINT 'Generating AirplaneSeats for existing Airplanes...'

-- Clear existing seats to avoid duplicates
DELETE FROM [dbo].[AirplaneSeats];

-- Insert seats for each airplane based on its model's template
INSERT INTO [dbo].[AirplaneSeats] ([Id], [AirplaneId], [SeatNumber], [SeatRow], [SeatColumn], [SeatClass], [IsExtraLegroom], [PriceMultiplier])
SELECT 
    NEWID(),
    a.Id,
    t.SeatNumber,
    t.SeatRow,
    t.SeatColumn,
    t.SeatClass,
    t.IsExtraLegroom,
    t.PriceMultiplier
FROM [dbo].[Airplanes] a
JOIN [dbo].[AircraftModelSeatTemplates] t ON a.AircraftModelId = t.AircraftModelId;

PRINT 'Aircraft Models and Seat Templates seeding completed successfully!'
PRINT '==========================================================='
GO