# Notifications Module

## Overview
The **Notifications Module** manages real-time and stored in-app communications for Customers, Staff, Partners, and System Administrators. Notifications are triggered by domain events (e.g., flight creation, schedule delays, booking confirmation, payment status, fare alerts) and delivered instantly via SignalR WebSockets with persistent storage in PostgreSQL for offline delivery.

---

## How It Works (For End Users & Clients)
1. **Triggering an Event**: When significant business events occur (e.g., `BookingConfirmedEvent`, `FlightStatusChangedEvent`, `PriceDroppedEvent`), event handlers invoke `CreateNotificationCommand`.
2. **Persistence & Localization**: Notifications are stored in the database with reference metadata (`ReferenceId`, `ReferenceType`, `ActionUrl`). System messages utilize localized `NotificationTemplate` records based on the user's preferred language.
3. **Real-Time Push**: `NotificationPusher` / `RedisNotificationPusher` broadcasts the notification payload across `AirlineTicket.SignalR` (`NotificationHub`) directly to the recipient's personal user group (`userId`) or airline staff group (`airline-staff-{airlineId}`).
4. **User Interaction**: Users view unread badges (`GET /api/notifications/unread-count`), fetch paginated notifications (`GET /api/notifications`), mark notifications as read individually or in bulk (`PUT /api/notifications/{id}/read`, `PUT /api/notifications/read-all`), or soft-delete them (`DELETE /api/notifications/{id}`).

---

## Domain Entities & Data Model

### Notification
Represents an individual in-app notification.
- `Id` (Guid): Unique identifier.
- `UserId` (Guid?): Recipient User ID (null for broadcast alerts).
- `Type` (string): Notification category (e.g., `FlightCreated`, `FlightStatusChanged`, `BookingConfirmed`, `PaymentSuccess`, `FareAlertPriceDrop`).
- `Severity` (int): `0` = Info, `1` = Critical.
- `Title` (string): Notification title.
- `Content` (string?): Detailed notification body.
- `ActionUrl` (string?): Deep link URL for client navigation (e.g., `/staff/flights/{id}/seats`, `/booking/{pnr}`).
- `ReferenceId` (Guid?): Source entity ID (FlightId, BookingId, AlertId, etc.).
- `ReferenceType` (string?): Source entity type (`Flight`, `Booking`, `Payment`, `FareAlert`).
- `IsRead` (bool): Read status.
- `IsDeleted` (bool): Soft delete flag.
- `CreatedAt` (DateTime): UTC timestamp of creation.

### NotificationTemplate
Represents a localized template for automated notification generation.
- `Id` (Guid): Unique identifier.
- `Code` (string): Template identifier code (e.g., `BOOKING_CONFIRMED`, `FLIGHT_DELAYED`).
- `Subject` (string): Localized title or subject template.
- `BodyTemplate` (string): Localized message template with placeholder variables.
- `Language` (string): ISO language code (`vi`, `en`, `zh`, `ja`, `ko`, `fr`).
- `IsDeleted` (bool): Soft delete flag.

---

## API Reference

### Authentication Roles
- **AdminOnly**: Requires authentication as a System Admin.
- **Authenticated**: Requires any valid JWT Bearer token.
- **Anonymous**: Public status probe.

### Endpoints

#### User Notifications & Status (`/api/notifications`)
| Method | Path | Auth | Description | Input Type | Output Type |
|--------|------|------|-------------|------------|-------------|
| **GET** | `/api/notifications/status` | None | Get notification module health and configuration status. | None | `NotificationStatusDto` |
| **GET** | `/api/notifications` | Authenticated | Get paginated notifications for current authenticated user. | Query params `pageNumber?: int, pageSize?: int, locale?: string` | `PagedResult<NotificationDto>` |
| **GET** | `/api/notifications/unread-count` | Authenticated | Get total unread notifications count. | None | `{ count: int }` |
| **PUT** | `/api/notifications/{id}/read` | Authenticated | Mark a specific notification as read. | Route param `id: Guid` | `204 NoContent` |
| **PUT** | `/api/notifications/read-all` | Authenticated | Mark all notifications as read for the current user. | None | `204 NoContent` |
| **DELETE** | `/api/notifications/{id}` | Authenticated | Soft delete a notification. | Route param `id: Guid` | `204 NoContent` |

#### Admin Notification Templates (`/api/admin/notifications/templates`)
| Method | Path | Auth | Description | Input Type | Output Type |
|--------|------|------|-------------|------------|-------------|
| **GET** | `/api/admin/notifications/templates` | AdminOnly | Get all notification templates. | None | `List<NotificationTemplateDto>` |
| **GET** | `/api/admin/notifications/templates/{id}` | AdminOnly | Get a notification template by ID. | Route param `id: Guid` | `NotificationTemplateDto` |
| **PUT** | `/api/admin/notifications/templates/{id}` | AdminOnly | Update an existing notification template. | Route param `id: Guid`, `UpdateTemplateRequest { subject: string, bodyTemplate: string, language: string }` | `204 NoContent` |

---

## Real-Time Notification Delivery (SignalR)

### Hub Endpoint
- **URL**: `/hubs/notifications` (also available at `/notificationHub`)
- **Authentication**: Required (JWT Bearer via query string `?access_token=...`)

### Group Subscriptions (Automatic on Connection)
- `userId`: Every connected user automatically joins a group named after their `UserId`.
- `airline-staff-{airlineId}`: Airline staff and partners automatically join an airline-scoped notification group.

### Server → Client Events
- `ReceiveNotification(NotificationDto notification)`: Pushed immediately when an in-app alert is generated for the user or their airline.
