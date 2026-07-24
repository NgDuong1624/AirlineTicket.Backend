# Logs Module

## Overview
The **Logs Module** provides a centralized read-only view of system and partner activity logs. It captures diagnostic and audit information (info, warnings, errors, and critical events) and allows system administrators and airline partners to query these logs with filtering and pagination.

---

## How It Works (For End Users & Clients)
1. **Log Generation**: Various modules throughout the system write log entries to the `SystemLogs` table, capturing levels (Info, Warning, Error, Critical), messages, source modules, exceptions, and contextual data (User ID, Airline ID, IP address).
2. **Log Retrieval**: 
   - System Administrators can query all logs with optional filters (`Level`, `Search` text, `AirlineId`, `IsSystemLog`) and pagination.
   - Airline Partners can query logs scoped to their airline with the same filtering and pagination.

---

## Domain Entities & Data Model

### SystemLog
Represents a single log entry in the system.
- `Id` (Guid): Unique identifier.
- `Type` (LogType): Type of log entry (enum from BuildingBlocks).
- `Metadata` (string?): JSON data (e.g., old/new values for audit purposes).
- `Level` (string): Severity level (e.g., `Info`, `Warning`, `Error`, `Critical`).
- `Message` (string): The log message.
- `Source` (string?): The component or module that generated the log.
- `Exception` (string?): Full exception details if an error occurred.
- `UserId` (Guid?): FK to the user associated with the log entry.
- `AirlineId` (Guid?): FK to the airline associated with the log entry.
- `IpAddress` (string?): IP address of the user at the time the log was generated.
- `IsSystemLog` (bool): Whether this is a system-level log (not associated with any specific airline).
- `CreatedAt` (DateTime): UTC timestamp of when the log was created.

---

## API Reference

### Authentication Roles
- **AdminOnly**: Requires authentication as a System Admin.
- **PartnerOnly**: Requires authentication as an Airline Admin.

### Endpoints

| Method | Path | Auth | Description |
|--------|------|------|-------------|
| **GET** | `/api/admin/logs` | AdminOnly | Retrieves all system logs. Supports `level`, `search`, `airlineId`, `isSystemLog`, `pageIndex`, and `pageSize` query parameters. |
| **GET** | `/api/partner/logs` | PartnerOnly | Retrieves logs scoped to the authenticated partner's airline. Supports `level`, `search`, `pageIndex`, and `pageSize` query parameters. The `airlineId` is automatically extracted from the token. |

### Query Parameters
| Parameter | Type | Description |
|-----------|------|-------------|
| `level` | string? | Filter by log level (e.g., `Error`, `Warning`, `Info`, `Critical`). |
| `search` | string? | Full-text search across message, source, exception, and IP address fields. |
| `airlineId` | string? | (Admin only) Filter by a specific airline's ID. Use `"system"` to filter for system-level logs. |
| `isSystemLog` | bool? | (Admin only) Filter to show only system logs or non-system logs. |
| `pageIndex` | int | Page number for pagination (default: 1, minimum: 1). |
| `pageSize` | int | Number of items per page (default: 10, minimum: 1, maximum: 100). |
