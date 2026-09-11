# CMS Module

## Overview
The **CMS (Content Management System) Module** aggregates high-level analytics and manages global system configurations. It compiles system-wide statistics for System Administrators and airline-specific performance metrics for Airline Partners, consolidating data across multiple modules (Bookings, Flights, Users, and Logs).

---

## How It Works (For End Users & Clients)
1. **Admin Dashboard**: Aggregates cross-module metrics, including gross revenue, completed bookings, new customer registrations, total scheduled flights, active airline partners, and recent critical system logs.
2. **Partner Dashboard**: Provides an airline-scoped performance view, including active aircraft fleet count, total registered staff, daily bookings, monthly revenue, and recent flight dispatches.
3. **Settings Management**: Manages system-wide platform settings such as commission fees, default currency, booking limits, and maintenance mode status.

---

## Domain Entities & Data Model

### Article
Represents travel guides, news posts, and promotional content.
- `Id` (Guid): Unique identifier.
- `CategoryId` (Guid): FK to the associated `Category`.
- `AuthorId` (Guid): FK to author in `users.users`.
- `Title` (string): Article title.
- `Slug` (string): URL-friendly identifier (unique).
- `Summary` (string?): Short summary.
- `Content` (string): Markdown or HTML content.
- `ThumbnailUrl` (string?): URL to cover image.
- `PublishedAt` (DateTime?): Publication timestamp.
- `Status` (int): `0` = Draft, `1` = Published, `2` = Archived.
- `ViewCount` (int): View counter.
- `IsDeleted` (bool): Soft delete flag.
- `CreatedAt` (DateTime): UTC timestamp of creation.

### Category
Represents an article category or taxonomy.
- `Id` (Guid): Unique identifier.
- `Name` (string): Category name.
- `Slug` (string): URL-friendly identifier (unique).
- `IsDeleted` (bool): Soft delete flag.

---

## API Reference

### Authentication Roles
- **AdminOnly**: System Administrator role (`Role = 0`).
- **PartnerOnly**: Airline Partner role (`Role = 1`).

### Endpoints

#### Admin Endpoints
| Method | Path | Auth | Description | Input Type | Output Type |
|--------|------|------|-------------|------------|-------------|
| **GET** | `/api/admin/dashboard` | AdminOnly | Retrieves system-wide overview statistics, partner activity, and recent critical logs. | None | `AdminDashboardDto` |
| **GET** | `/api/admin/settings` | PartnerOnly / Admin | Retrieves global system settings (e.g. commission fee, maintenance mode, booking limits). | None | `AdminSettingsDto` |
| **PUT** | `/api/admin/settings` | PartnerOnly / Admin | Updates global system settings. | `AdminSettingsDto` | `AdminSettingsDto` |

#### Partner Endpoints
| Method | Path | Auth | Description | Input Type | Output Type |
|--------|------|------|-------------|------------|-------------|
| **GET** | `/api/partner/dashboard` | PartnerOnly | Retrieves airline-specific dashboard statistics, recent flights, and booking summaries. | None | `PartnerDashboardDto` |

---

## Cross-Module Data Aggregation
The CMS module executes optimized direct SQL queries (via Dapper and raw SQL projections) to aggregate data across database schemas (`bookings`, `flights`, `identity`, `logs`) without requiring circular module dependencies.
