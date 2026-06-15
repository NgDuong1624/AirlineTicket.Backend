-- =====================================================
-- AIRLINE TICKET SYSTEM - INSTEAD OF DELETE TRIGGERS
-- FOR SOFT DELETE IMPLEMENTATION IN SQL SERVER
-- =====================================================

USE [master]
GO

-- 1. Triggers cho Schema identity
CREATE TRIGGER [identity].[TR_Users_SoftDelete]
ON [identity].[Users]
INSTEAD OF DELETE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE [identity].[Users]
    SET [IsActive] = 0, [IsDeleted] = 1, [UpdatedAt] = GETUTCDATE()
    WHERE [Id] IN (SELECT [Id] FROM deleted);
END
GO

-- 2. Triggers cho Schema flights
CREATE TRIGGER [flights].[TR_Airlines_SoftDelete]
ON [flights].[Airlines]
INSTEAD OF DELETE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE [flights].[Airlines]
    SET [IsDeleted] = 1
    WHERE [Id] IN (SELECT [Id] FROM deleted);
END
GO

CREATE TRIGGER [flights].[TR_Airports_SoftDelete]
ON [flights].[Airports]
INSTEAD OF DELETE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE [flights].[Airports]
    SET [IsDeleted] = 1
    WHERE [Id] IN (SELECT [Id] FROM deleted);
END
GO

CREATE TRIGGER [flights].[TR_Airplanes_SoftDelete]
ON [flights].[Airplanes]
INSTEAD OF DELETE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE [flights].[Airplanes]
    SET [IsDeleted] = 1
    WHERE [Id] IN (SELECT [Id] FROM deleted);
END
GO

CREATE TRIGGER [flights].[TR_Routes_SoftDelete]
ON [flights].[Routes]
INSTEAD OF DELETE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE [flights].[Routes]
    SET [IsDeleted] = 1
    WHERE [Id] IN (SELECT [Id] FROM deleted);
END
GO

CREATE TRIGGER [flights].[TR_Flights_SoftDelete]
ON [flights].[Flights]
INSTEAD OF DELETE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE [flights].[Flights]
    SET [IsDeleted] = 1, [UpdatedAt] = GETUTCDATE()
    WHERE [Id] IN (SELECT [Id] FROM deleted);
END
GO

-- 3. Triggers cho Schema bookings
CREATE TRIGGER [bookings].[TR_Bookings_SoftDelete]
ON [bookings].[Bookings]
INSTEAD OF DELETE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE [bookings].[Bookings]
    SET [IsDeleted] = 1, [UpdatedAt] = GETUTCDATE()
    WHERE [Id] IN (SELECT [Id] FROM deleted);
END
GO

-- 4. Triggers cho Schema promotions
CREATE TRIGGER [promotions].[TR_Coupons_SoftDelete]
ON [promotions].[Coupons]
INSTEAD OF DELETE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE [promotions].[Coupons]
    SET [IsDeleted] = 1
    WHERE [Id] IN (SELECT [Id] FROM deleted);
END
GO

CREATE TRIGGER [promotions].[TR_Campaigns_SoftDelete]
ON [promotions].[Campaigns]
INSTEAD OF DELETE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE [promotions].[Campaigns]
    SET [IsDeleted] = 1
    WHERE [Id] IN (SELECT [Id] FROM deleted);
END
GO

-- 5. Triggers cho Schema interactions
CREATE TRIGGER [interactions].[TR_Reviews_SoftDelete]
ON [interactions].[Reviews]
INSTEAD OF DELETE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE [interactions].[Reviews]
    SET [IsDeleted] = 1
    WHERE [Id] IN (SELECT [Id] FROM deleted);
END
GO

-- 6. Triggers cho Schema cms
CREATE TRIGGER [cms].[TR_Categories_SoftDelete]
ON [cms].[Categories]
INSTEAD OF DELETE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE [cms].[Categories]
    SET [IsDeleted] = 1
    WHERE [Id] IN (SELECT [Id] FROM deleted);
END
GO

CREATE TRIGGER [cms].[TR_Articles_SoftDelete]
ON [cms].[Articles]
INSTEAD OF DELETE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE [cms].[Articles]
    SET [IsDeleted] = 1
    WHERE [Id] IN (SELECT [Id] FROM deleted);
END
GO
