# Missing Features & Components Analysis

Based on the comparison between `architecture_and_devops_plan.md` and the current project structure, the following features and components are missing or not fully implemented.

## 1. Missing Background Services (Worker Jobs)

### Module Bookings
- [ ] `CancelExpiredBookingsJob`: Auto-cancel pending bookings after 15-30 mins and return seats.
- [ ] `BookingRefundProcessor`: Async refund processing via Message Queue with retry logic.

### Module Notifications
- [ ] `EmailSmsMassSender`: Event consumer for async email/SMS sending.
- [ ] `FlightDelayNotifierJob`: Auto-notify passengers of delayed flights.

### Module Flights
- [ ] `FlightStatusAutomatorJob`: Auto-update flight status (Scheduled → Boarding → InAir → Landed).
- [ ] `FlightDelayDetectorJob`: Auto-detect delayed flights and trigger events.
- [ ] `CloseFlightSalesJob`: Lock seats and close sales 45-60 mins before departure.
- [ ] `CheckInReminderJob`: Send check-in reminders 24h before departure.
- [ ] `DynamicPricingJob`: Auto-adjust base price based on load factor.

### Module Promotions
- [ ] `PromotionStatusUpdaterJob`: Auto-update promotion status (Pending → Active → Expired).

### Module Users & Interactions
- [ ] `CleanExpiredTokensJob`: Clean up expired/revoked refresh tokens.
- [ ] `ChatbotTimeoutHandler`: Auto-close inactive chat sessions after 15-30 mins.

### Module Logs
- [ ] `LogRetentionCleanerJob`: Clean/compress old logs (30-90 days).
- [ ] `DailySalesReportJob`: Generate and send daily sales reports.

## 2. Missing Realtime (SignalR) Features
- [x] **Redis Backplane for SignalR**: Configure `AddStackExchangeRedis` for SignalR to support scaling out.

## 3. Missing Host Separation
- [x] **Unified Worker Host**: Consolidate individual worker projects into a single `AirlineTicket.Worker` host with `ModuleWorkerRegistration.cs`.
- [x] **Dedicated Realtime Host**: Move SignalR hubs from `AirlineTicket.Api` to a dedicated `AirlineTicket.SignalR` host.
