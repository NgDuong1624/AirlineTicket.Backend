ALTER TABLE [logs].[SystemLogs] ADD [Metadata] NVARCHAR(MAX) NULL;
ALTER TABLE [logs].[SystemLogs] ADD [Type] INT NULL;
ALTER TABLE [logs].[SystemLogs] ADD [IsSystemLog] BIT NOT NULL DEFAULT 0;
GO
