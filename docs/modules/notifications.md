# Notifications Module

## Overview
Handles outbound multi-channel communication (Email, SMS, Push) for the platform.

## Domain Entities
- **Notification** — Outbox record for a single message to be sent.
  - Fields: `Id`, `UserId?`, `Recipient`, `Subject?`, `Content`, `Type` (0 Email, 1 SMS, 2 Push), `Status` (0 Pending, 1 Sent, 2 Failed), `RetryCount`, `ErrorMessage?`, `SentAt?`, `CreatedAt`
- **NotificationTemplate** — Pre-defined message templates for automated communications.
  - Fields: `Id`, `Code` (e.g. `BOOKING_CONFIRMED`), `Subject`, `BodyTemplate`, `Language`

## Key Interfaces (Application/Contracts)
- `INotificationRepository` — Persistence operations (Add, GetPending, GetAll, MarkSent, MarkFailed)
- `ITemplateRepository` — Manage NotificationTemplate CRUD
- `INotificationSender` — Routes to the correct channel sender based on `Notification.Type`
- `IEmailSender` — Sends email (logging placeholder)
- `ISmsSender` — Sends SMS (logging placeholder)

## Background Service
- `NotificationProcessingBackgroundService` — Runs every 10 seconds, polls pending notifications from DB, dispatches via `INotificationSender`, and marks Sent/Failed.

## API Endpoints
| Method | Path | Auth | Description |
|--------|------|------|-------------|
| GET | `/api/v1/notifications` | - | Status check |
| GET | `/api/v1/notifications/manage` | PartnerOrStaff | List sent notifications (last 100) |
| GET | `/api/v1/notifications/manage/templates` | PartnerOrStaff | List templates |
| POST | `/api/v1/notifications/manage/templates` | PartnerOrStaff | Create template |

## Sender Configuration (appsettings.json)
```json
"Email": {
  "SmtpHost": "smtp.example.com",
  "SmtpPort": 587,
  "SmtpUsername": "",
  "SmtpPassword": "",
  "FromAddress": "noreply@skyviet.com",
  "FromName": "SkyViet"
}
```
