# CMS Module

## Overview
The **CMS (Content Management System) Module** handles the management of articles, categories, and system-wide dashboard statistics. It provides aggregated data views for both System Administrators and Airline Partners, pulling data across multiple modules (Bookings, Flights, Users, etc.) to generate comprehensive dashboards.

---

## How It Works (For End Users & Clients)
1. **Dashboards**: The module exposes high-level statistics. 
   - **Admin Dashboard**: Shows total revenue, total bookings, new users, total flights, recent partner activity, and critical system logs.
   - **Partner Dashboard**: Shows airline-specific metrics like active aircraft, total staff, today's bookings, monthly revenue, recent flights, and recent bookings.
2. **Settings**: Manages global system settings like service fees, default currency, and maintenance mode.
3. **Content Management**: While the database schema supports `Articles` and `Categories` for a blog or news section, the current API implementation focuses primarily on dashboards and settings.

---

## Domain Entities & Data Model

### Article
Represents a news post, travel guide, or promotional article.
- `Id` (Guid): Unique identifier.
- `CategoryId` (Guid): FK to the associated `Category`.
- `AuthorId` (Guid): FK to the user who wrote the article.
- `Title` (string): Article title.
- `Slug` (string): URL-friendly identifier (unique).
- `Summary` (string?): Short description.
- `Content` (string): Full HTML/Markdown content.
- `ThumbnailUrl` (string?): URL to the cover image.
- `PublishedAt` (DateTime?): Timestamp of publication.
- `Status` (int): `0` = Draft, `1` = Published, `2` = Archived.
- `ViewCount` (int): Number of times the article was viewed.
- `IsDeleted` (bool): Soft delete flag.
- `CreatedAt` (DateTime): UTC timestamp of creation.

### Category
Represents a grouping for articles.
- `Id` (Guid): Unique identifier.
- `Name` (string): Category name.
- `Slug` (string): URL-friendly identifier (unique).
- `IsDeleted` (bool): Soft delete flag.

---

## API Reference

### Authentication Roles
- **AdminOnly**: Requires authentication as a System Admin.
- **PartnerOnly**: Requires authentication as an Airline Admin.

### Endpoints

#### Admin Endpoints
| Method | Path | Auth | Description | Input Type | Output Type |
|--------|------|------|-------------|------------|-------------|
| **GET** | `/api/admin/dashboard` | AdminOnly | Retrieves system-wide statistics, recent partners, and critical logs. | None | `unknown (Admin Dashboard metrics)` |
| **GET** | `/api/admin/settings` | AdminOnly | Retrieves global system settings (e.g., Service Fee, Currency). | None | `AdminSettings { siteName: string, maintenanceMode: boolean, maxBookingPerUser: string, holdLimit: string, commissionFee: string }` |
| **PUT** | `/api/admin/settings` | AdminOnly | Updates global system settings. | `AdminSettings { siteName: string, maintenanceMode: boolean, maxBookingPerUser: string, holdLimit: string, commissionFee: string }` | `AdminSettings` |

#### Partner Endpoints
| Method | Path | Auth | Description | Input Type | Output Type |
|--------|------|------|-------------|------------|-------------|
| **GET** | `/api/partner/dashboard` | PartnerOnly | Retrieves airline-specific statistics, recent flights, and recent bookings. | None | `unknown (Partner Dashboard metrics)` |

---

## Cross-Module Data Aggregation
The CMS module utilizes raw SQL queries (via Dapper) to aggregate data efficiently across multiple schemas without relying on complex Entity Framework joins. 
- The `DashboardRepository` queries tables from the `bookings`, `flights`, `identity`, and `logs` schemas to compile the dashboard responses.