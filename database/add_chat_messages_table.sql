-- SQL Script to create ChatMessages table manually if EF migrations are not run.
USE [AirlineTicketDb]
GO

IF OBJECT_ID('dbo.ChatMessages', 'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[ChatMessages] (
        [Id] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [PK_ChatMessages] PRIMARY KEY DEFAULT NEWID(),
        [AirlineId] UNIQUEIDENTIFIER NOT NULL,
        [SenderRole] NVARCHAR(20) NOT NULL,
        [SenderName] NVARCHAR(100) NOT NULL,
        [CustomerConnectionId] NVARCHAR(100) NULL,
        [StaffConnectionId] NVARCHAR(100) NULL,
        [Content] NVARCHAR(MAX) NOT NULL,
        [SentAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE()
    );

    CREATE INDEX [IX_ChatMessages_AirlineId_SentAt] ON [dbo].[ChatMessages] ([AirlineId], [SentAt]);

    PRINT 'Table ChatMessages created successfully.';
END
ELSE
BEGIN
    PRINT 'Table ChatMessages already exists.';
END
GO
