# Notifications Module

## Overview
The **Notifications Module** manages real-time in-app communications for Staff, Partner, and Admin roles. Notifications are triggered by system events (e.g., flight creation, status changes, profile updates, system errors) and delivered instantly via SignalR, with persistent storage for offline users.

---

## How It Works (For End Users & Clients)
1. **Triggering a Notification**: When a domain event occurs (e.g., `FlightCreatedEvent`), the system creates a `Notification` record in the database.
2. **Real-time Delivery**: The system immediately pushes the notification to the target user via `AirlineTicket.SignalR` (`NotificationHub`).
3. **State Management**: The frontend client (React/Zustand) receives the SignalR event, updates the unread count, and displays a toast/bell indicator.
4. **User Interaction**: When a user clicks a notification, it is marked as read via API, and the user is redirected to the relevant `ActionUrl`.

---

## Domain Entities & Data Model

### Notification
Represents an individual in-app alert.
- `Id` (Guid): Unique identifier.
- `UserId` (Guid?): Recipient User ID.
- `Type` (string): Notification category (e.g., `FlightCreated`, `FlightStatusChanged`, `SystemError`).
- `Severity` (int): `0` = Info, `1` = Critical.
- `Title` (string): Short title.
- `Content` (string?): Detailed message.
- `ActionUrl` (string?): Deep link to redirect on click (e.g., `/staff/flights/{id}/seats`).
- `ReferenceId` (Guid?): Source entity ID (FlightId, AirlineId, etc.).
- `ReferenceType` (string?): Source entity type (`Flight`, `Airline`, `Booking`).
- `IsRead` (bool): Read status.
- `IsDeleted` (bool): Soft delete flag.
- `CreatedAt` (DateTime): UTC timestamp.

---

## Backend Architecture

### 1. Domain Layer (`Modules/v1/Notifications`)
- `Notification` entity.
- `NotificationSeverity` enum.
- `NotificationTemplate` entity.
- `INotificationRepository`.
- `ITemplateRepository`.

### 2. Infrastructure Layer (`Modules/v1/Notifications`)
- `NotificationRepository` (EF Core).
- Entity Framework mapping for `Notification`.

### 3. Application Layer (`Modules/v1/Notifications`)
- **Commands**: `CreateNotificationCommand`, `MarkNotificationAsReadCommand`, `MarkAllNotificationsAsReadCommand`, `DeleteNotificationCommand`, `UpdateTemplateCommand`.
- **Queries**: `GetUnreadNotificationCountQuery`, `GetNotificationsQuery` (Paginated), `GetTemplatesQuery`, `GetTemplateByIdQuery`.
- **Event Handlers**: Listen to domain events to trigger `CreateNotificationCommand`.

### 4. Real-time Layer (`AirlineTicket.SignalR`)
- `NotificationHub`: Pushes notifications to specific users (`Clients.User(userId).SendAsync("ReceiveNotification", notification)`).

### 5. API Layer (`AirlineTicket.Api`)
- Minimal API Endpoints in `NotificationEndpoints.cs` for queries and commands.

---

## API Reference

### Endpoints

| Method | Path | Auth | Description | Input Type | Output Type |
|--------|------|------|-------------|------------|-------------|
| **GET** | `/api/notifications` | Authenticated | Retrieves paginated notifications for the current user. | None (Query params `pageNumber: number, pageSize: number, locale?: string`) | `Notification[] { id: string, userId?: string, type: string, severity: number, title: string, content?: string, actionUrl?: string, referenceId?: string, referenceType?: string, isRead: boolean, isDeleted: boolean, createdAt: string }` |
| **GET** | `/api/notifications/unread-count` | Authenticated | Retrieves the unread notification count. | None | `number` |
| **PUT** | `/api/notifications/{id}/read` | Authenticated | Marks a specific notification as read. | None | `void` |
| **PUT** | `/api/notifications/read-all` | Authenticated | Marks all notifications as read for the current user. | None | `void` |
| **DELETE** | `/api/notifications/{id}` | Authenticated | Soft deletes a notification. | None | `void` |
| **GET** | `/api/admin/notifications/templates` | AdminOnly | Retrieves all notification templates. | None | `unknown[]` |
| **GET** | `/api/admin/notifications/templates/{id}` | AdminOnly | Retrieves a notification template by ID. | None | `unknown` |
| **PUT** | `/api/admin/notifications/templates/{id}` | AdminOnly | Updates an existing notification template. | `unknown` | `unknown` |

---

## Integration Points (Per Role)
- **Staff**: Receives notification when Partner creates Flight. `ActionUrl` -> `/staff/flights/{flightId}/seats`.
- **Partner**: Receives notification when Flight status changes or Admin updates their profile.
- **Admin**: Receives notification when system error occurs.
