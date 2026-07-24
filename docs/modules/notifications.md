# Notifications Module

## Overview
The **Notifications Module** manages all outbound multi-channel communications (Email, SMS, Push, and SignalR) for the Airline Ticket platform. It operates as an asynchronous outbox system, ensuring reliable delivery even during high traffic or external provider downtime.

---

## How It Works (For End Users & Clients)
1. **Triggering a Notification**: When an event occurs (e.g., booking confirmation, flight delay), the system creates a `Notification` record in the database with a `Pending` status.
2. **Template Rendering**: The system resolves the appropriate `NotificationTemplate` based on the event code (e.g., `BOOKING_CONFIRMED`) and the user's preferred language (`vi` or `en`). It replaces placeholders (like `{{PassengerName}}` or `{{PnrCode}}`) with actual transaction data.
3. **Background Processing**: A background worker (`NotificationProcessingBackgroundService`) polls the database every 10 seconds for `Pending` notifications.
4. **Delivery**: The worker dispatches each notification to the correct channel provider (Email, SMS, or Push).
5. **Retry Mechanism**: If a delivery fails, the system logs the error, increments the `RetryCount`, and marks the status as `Failed`. The background worker will retry sending failed notifications up to a configured limit.

---

## Notification Channels
- **Email (Type 0)**: Used for rich-text communications like booking confirmations, e-tickets, and marketing campaigns.
- **SMS (Type 1)**: Used for urgent alerts, OTP verification, and quick updates.
- **Push (Type 2)**: Used for mobile app notifications (e.g., gate changes, check-in reminders).
- **SignalR (Type 3)**: Used for real-time in-app alerts while the user is actively browsing the website.

---

## Domain Entities & Data Model

### Notification
Represents an individual message queued or sent by the system.
- `Id` (Guid): Unique identifier.
- `UserId` (Guid?): Optional link to the recipient's user account.
- `Recipient` (string): Destination address (Email address, phone number, or device token).
- `Subject` (string?): Subject line (primarily for Emails).
- `Content` (string): The rendered message body.
- `Type` (int): Channel type (`0` = Email, `1` = SMS, `2` = Push, `3` = SignalR).
- `Status` (int): Delivery status (`0` = Pending, `1` = Sent, `2` = Failed).
- `RetryCount` (int): Number of failed delivery attempts.
- `ErrorMessage` (string?): Error details if the delivery failed.
- `SentAt` (DateTime?): UTC timestamp when successfully sent.
- `CreatedAt` (DateTime): UTC timestamp when the notification was queued.

### NotificationTemplate
Pre-defined message templates supporting localization and dynamic variables.
- `Id` (Guid): Unique identifier.
- `Code` (string): Unique template identifier (e.g., `BOOKING_CONFIRMED`, `FLIGHT_DELAYED`).
- `Subject` (string): Default subject line.
- `BodyTemplate` (string): Message body containing placeholders (e.g., `Hello {{PassengerName}}`).
- `Language` (string): Language code (e.g., `vi`, `en`).
- `CreatedAt` (DateTime): UTC timestamp when the template was created.

---

## API Reference

### Authentication Roles
- **PartnerOrStaff**: Requires authentication as an Airline Admin, Airline Staff, or System Admin.

### Endpoints

| Method | Path | Auth | Description | Request Body / Response |
|--------|------|------|-------------|-------------------------|
| **GET** | `/api/v1/notifications` | None | Health check endpoint for the notification module. | Returns status `200 OK`. |
| **GET** | `/api/v1/notifications/manage` | PartnerOrStaff | Retrieves the last 100 queued notifications. | Returns a list of `Notification` records. |
| **GET** | `/api/v1/notifications/manage/templates` | PartnerOrStaff | Retrieves all registered notification templates. | Returns a list of `NotificationTemplate` records. |
| **POST** | `/api/v1/notifications/manage/templates` | PartnerOrStaff | Registers a new notification template. | **JSON Body:**<br>```json{"code": "STRING", "subject": "STRING", "bodyTemplate": "STRING", "language": "STRING"}``` |

---

## Configuration (`appsettings.json`)
Configure SMTP credentials and external gateways under the notification section:
```json
{
  "Email": {
    "SmtpHost": "smtp.example.com",
    "SmtpPort": 587,
    "SmtpUsername": "your-username",
    "SmtpPassword": "your-password",
    "FromAddress": "noreply@skyviet.com",
    "FromName": "SkyViet"
  }
}
```
