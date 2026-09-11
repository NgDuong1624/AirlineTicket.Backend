# Logs Module

## Overview
The **Logs Module** provides centralized logging, audit trail tracking, and diagnostic querying across the system. It captures informational, warning, error, and critical security and operational events, allowing System Administrators and Airline Partners to inspect system events through paginated, filtered API endpoints.

---

## How It Works (For End Users & Clients)
1. **Event Capture**: Modules across the backend write structured events to the `SystemLogs` table (via MediatR behaviors, Serilog sinks, and middleware), capturing severity levels, source modules, exceptions, correlation IDs, and actor identifiers.
2. **Admin Querying**: System Administrators can query logs across the entire platform, filtering by log level, date, search keyword, or specific airline ID.
3. **Partner Scoping**: Airline Partners can query logs strictly partitioned by their `AirlineId` claim.

---

## Domain Entities & Data Model

### SystemLog
Represents an individual structured log entry.
- `Id` (Guid): Unique identifier.
- `Type` (LogType): Enum identifying log type (e.g., `Audit`, `System`, `Error`, `Security`).
- `Metadata` (string?): Serialized JSON payload containing state before/after changes for audit tracking.
- `Level` (string): Severity level (`"Info"`, `"Warning"`, `"Error"`, `"Critical"`).
- `Message` (string): Human-readable log message.
- `Source` (string?): Component, endpoint, or class name generating the log entry.
- `Exception` (string?): Stack trace and exception details if an error occurred.
- `UserId` (Guid?): FK to user performing the action.
- `AirlineId` (Guid?): FK to associated airline.
- `IpAddress` (string?): Client IP address.
- `IsSystemLog` (bool): Whether this is a global system event vs an airline tenant event.
- `CreatedAt` (DateTime): UTC timestamp of log creation.

---

## API Reference

### Authentication Roles
- **AdminOnly**: System Administrator role (`Role = 0`).
- **PartnerOnly**: Airline Partner role (`Role = 1`).

### Endpoints

| Method | Path | Auth | Description | Input Type | Output Type |
|--------|------|------|-------------|------------|-------------|
| **GET** | `/api/admin/logs` | AdminOnly | Retrieves system-wide logs with multi-field filtering. | Query params `level?: string, search?: string, airlineId?: string, date?: DateTime, pageIndex?: int, pageNumber?: int, pageSize?: int` | `PagedResult<SystemLogDto>` |
| **GET** | `/api/partner/logs` | PartnerOnly | Retrieves logs scoped to the authenticated partner's airline. | Query params `level?: string, search?: string, date?: DateTime, pageIndex?: int, pageNumber?: int, pageSize?: int` | `PagedResult<SystemLogDto>` |

### Query Parameters
| Parameter | Type | Applicable Role | Description |
|-----------|------|-----------------|-------------|
| `level` | string? | Admin, Partner | Filter by log severity (`"Info"`, `"Warning"`, `"Error"`, `"Critical"`). |
| `search` | string? | Admin, Partner | Text search matching message, source, exception, or IP address. |
| `airlineId` | string? | Admin only | Filter by specific airline ID or pass `"system"` for platform logs. |
| `date` | DateTime? | Admin, Partner | Filter by date of event. |
| `pageIndex` / `pageNumber` | int | Admin, Partner | Page number for pagination (default: 1). |
| `pageSize` | int | Admin, Partner | Number of items per page (default: 10, range: 1–100). |
