-- =====================================================
-- AIRLINE TICKET SYSTEM - INSTEAD OF DELETE TRIGGERS
-- FOR SOFT DELETE IMPLEMENTATION IN SQL SERVER
-- =====================================================

USE [AirlineTicketDb]
GO

-- 1. Triggers cho Schema Users (was identity)
CREATE TRIGGER [dbo].[TR_Users_SoftDelete]
ON [dbo].[Users]
INSTEAD OF DELETE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE [dbo].[Users]
    SET [IsActive] = 0, [IsDeleted] = 1, [UpdatedAt] = GETUTCDATE()
    WHERE [Id] IN (SELECT [Id] FROM deleted);
END
GO

-- 2. Triggers cho Schema Flights
CREATE TRIGGER [dbo].[TR_Airlines_SoftDelete]
ON [dbo].[Airlines]
INSTEAD OF DELETE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE [dbo].[Airlines]
    SET [IsDeleted] = 1
    WHERE [Id] IN (SELECT [Id] FROM deleted);
END
GO

CREATE TRIGGER [dbo].[TR_Airports_SoftDelete]
ON [dbo].[Airports]
INSTEAD OF DELETE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE [dbo].[Airports]
    SET [IsDeleted] = 1
    WHERE [Id] IN (SELECT [Id] FROM deleted);
END
GO

CREATE TRIGGER [dbo].[TR_Airplanes_SoftDelete]
ON [dbo].[Airplanes]
INSTEAD OF DELETE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE [dbo].[Airplanes]
    SET [IsDeleted] = 1
    WHERE [Id] IN (SELECT [Id] FROM deleted);
END
GO

CREATE TRIGGER [dbo].[TR_Routes_SoftDelete]
ON [dbo].[Routes]
INSTEAD OF DELETE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE [dbo].[Routes]
    SET [IsDeleted] = 1
    WHERE [Id] IN (SELECT [Id] FROM deleted);
END
GO

CREATE TRIGGER [dbo].[TR_Flights_SoftDelete]
ON [dbo].[Flights]
INSTEAD OF DELETE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE [dbo].[Flights]
    SET [IsDeleted] = 1, [UpdatedAt] = GETUTCDATE()
    WHERE [Id] IN (SELECT [Id] FROM deleted);
END
GO

-- 3. Triggers cho Schema Bookings
CREATE TRIGGER [dbo].[TR_Bookings_SoftDelete]
ON [dbo].[Bookings]
INSTEAD OF DELETE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE [dbo].[Bookings]
    SET [IsDeleted] = 1, [UpdatedAt] = GETUTCDATE()
    WHERE [Id] IN (SELECT [Id] FROM deleted);
END
GO

-- 4. Triggers cho Schema Promotions
CREATE TRIGGER [dbo].[TR_Coupons_SoftDelete]
ON [dbo].[Coupons]
INSTEAD OF DELETE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE [dbo].[Coupons]
    SET [IsDeleted] = 1
    WHERE [Id] IN (SELECT [Id] FROM deleted);
END
GO

CREATE TRIGGER [dbo].[TR_Campaigns_SoftDelete]
ON [dbo].[Campaigns]
INSTEAD OF DELETE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE [dbo].[Campaigns]
    SET [IsDeleted] = 1
    WHERE [Id] IN (SELECT [Id] FROM deleted);
END
GO

-- 5. Triggers cho Schema Interactions
CREATE TRIGGER [dbo].[TR_Reviews_SoftDelete]
ON [dbo].[Reviews]
INSTEAD OF DELETE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE [dbo].[Reviews]
    SET [IsDeleted] = 1
    WHERE [Id] IN (SELECT [Id] FROM deleted);
END
GO

-- 6. Triggers cho Schema CMS
CREATE TRIGGER [dbo].[TR_Categories_SoftDelete]
ON [dbo].[Categories]
INSTEAD OF DELETE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE [dbo].[Categories]
    SET [IsDeleted] = 1
    WHERE [Id] IN (SELECT [Id] FROM deleted);
END
GO

CREATE TRIGGER [dbo].[TR_Articles_SoftDelete]
ON [dbo].[Articles]
INSTEAD OF DELETE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE [dbo].[Articles]
    SET [IsDeleted] = 1
    WHERE [Id] IN (SELECT [Id] FROM deleted);
END
GO
