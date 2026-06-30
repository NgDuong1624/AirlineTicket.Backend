-- =====================================================
-- SEED ROLE PERMISSIONS
-- Maps roles to their default permissions
-- =====================================================

USE [AirlineTicketDb]
GO

-- System Admin (RoleId=0): Full access to all permissions
INSERT INTO [dbo].[RolePermissions] ([RoleId], [PermissionId])
SELECT 0, [Id] FROM [dbo].[Permissions];
GO

-- Airline Admin (RoleId=1): Manage flights, airlines, bookings, staff (no system-level permissions)
INSERT INTO [dbo].[RolePermissions] ([RoleId], [PermissionId])
SELECT 1, [Id] FROM [dbo].[Permissions]
WHERE [Code] IN ('CREATE_FLIGHT', 'SELL_TICKET', 'MANAGE_AIRLINE_STAFF', 'MANAGE_FLIGHTS', 'MANAGE_AIRLINES', 'MANAGE_BOOKINGS');
GO

-- Airline Staff (RoleId=2): Sell tickets, view bookings
INSERT INTO [dbo].[RolePermissions] ([RoleId], [PermissionId])
SELECT 2, [Id] FROM [dbo].[Permissions]
WHERE [Code] IN ('SELL_TICKET', 'MANAGE_FLIGHTS', 'MANAGE_BOOKINGS');
GO

-- User Client (RoleId=3): View-only permissions (no admin permissions)
INSERT INTO [dbo].[RolePermissions] ([RoleId], [PermissionId])
SELECT 3, [Id] FROM [dbo].[Permissions]
WHERE [Code] IN ('SELL_TICKET');
GO