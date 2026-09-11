# Airline Ticket Booking System API List

This document provides a **comprehensive** list of all API endpoints and SignalR real-time hubs in the backend system, categorized by access role and operational module.

---

## 1. Public APIs (Anonymous)

APIs that do not require authentication, serving public-facing functionalities.

### 1.1. Authentication & Account
| Method | Endpoint | Description |
| :--- | :--- | :--- |
| **POST** | `/api/auth/login` | User login with email/password (returns JWT access & refresh tokens) |
| **POST** | `/api/auth/google` | Login with Google ID token |
| **POST** | `/api/auth/register` | Register new customer account |
| **POST** | `/api/auth/refresh` | Refresh expired access token using refresh token |
| **POST** | `/api/auth/logout` | Logout and revoke refresh token session |

### 1.2. Flight Search, Catalog & Live Radar
| Method | Endpoint | Description |
| :--- | :--- | :--- |
| **GET** | `/api/airports` | List or search airports by keyword (pagination support) |
| **GET** | `/api/airlines` | List all registered airlines |
| **GET** | `/api/routes` | List flight routes (pagination support) |
| **POST** | `/api/flights` | Search one-way flights by route, date, class, price range, airlines, stops |
| **POST** | `/api/flights/round-trip` | Search round-trip flights |
| **GET** | `/api/flights/{id}` | Get flight details by ID |
| **GET** | `/api/flights/trending` | List trending flights/routes |
| **GET** | `/api/flights/{id}/seats` | Get seat map and availability for a flight |
| **GET** | `/api/flights/radar/active` | Get active airborne flights for real-time sky radar map |
| **GET** | `/api/flights/{id}/telemetry` | Get live flight telemetry, GPS coordinates, altitude, speed, bearing |
| **GET** | `/api/flights/status/{flightNumber}` | Get flight status timeline, gates, baggage carousel, delay info by flight number |

### 1.3. Booking & Payment
| Method | Endpoint | Description |
| :--- | :--- | :--- |
| **POST** | `/api/bookings` | Create a new flight booking (reserves seats, saves passenger details) |
| **GET** | `/api/bookings/{id}` | Get booking details by ID |
| **PUT** | `/api/bookings/{id}` | Update booking passenger and contact information |
| **DELETE** | `/api/bookings/{id}` | Cancel booking and release seats |
| **GET** | `/api/bookings/search` | Search booking by PNR code |
| **POST** | `/api/payments/checkout` | Initiate multi-gateway checkout session (Stripe, PayPal, VNPay, MoMo) |
| **GET** | `/api/payments/{bookingId}/status` | Get booking payment transaction status |
| **POST** | `/api/payments/webhooks/stripe` | Stripe asynchronous webhook callback |
| **POST** | `/api/payments/webhooks/paypal` | PayPal asynchronous webhook callback |
| **POST** | `/api/payments/webhooks/vnpay` | VNPay IPN webhook callback |
| **POST** | `/api/payments/webhooks/momo` | MoMo IPN webhook callback |
| **GET** | `/api/tickets/{id}` | Get e-ticket information |

### 1.4. Promotions
| Method | Endpoint | Description |
| :--- | :--- | :--- |
| **GET** | `/api/promotions` | List active promotional coupons |
| **GET** | `/api/promotions/campaigns` | List active promotional campaigns |
| **POST** | `/api/promotions/apply` | Validate and apply promotion code to order amount |

### 1.5. System & Interactions
| Method | Endpoint | Description |
| :--- | :--- | :--- |
| **GET** | `/api/health/live` | Health check liveness probe |
| **GET** | `/api/health` | Health check readiness probe (database connectivity) |
| **GET** | `/api/notifications/status` | Notification module health and configuration status |
| **GET** | `/api/v1/interactions` | Interactions module health status |
| **POST** | `/api/v1/qa/ask` | Ask AI Travel Assistant a question with chat history |

---

## 2. Authenticated APIs (Logged-in Users)

APIs requiring a valid JWT Bearer token, accessible by registered customers or privileged users.

### 2.1. User Profile
| Method | Endpoint | Description |
| :--- | :--- | :--- |
| **GET** | `/api/auth/me` | Currently logged-in user profile |
| **PUT** | `/api/auth/language` | Update user preferred language |

### 2.2. User Bookings
| Method | Endpoint | Description |
| :--- | :--- | :--- |
| **GET** | `/api/bookings/user/{id}` | User booking history (accessible by account owner or staff/admin) |
| **GET** | `/api/bookings/user/{id}/stats` | User booking statistics (total bookings, spent amount) |

### 2.3. User Notifications
| Method | Endpoint | Description |
| :--- | :--- | :--- |
| **GET** | `/api/notifications` | Paginated list of notifications for the authenticated user |
| **GET** | `/api/notifications/unread-count` | Unread notification count for the authenticated user |
| **PUT** | `/api/notifications/{id}/read` | Mark a specific notification as read |
| **PUT** | `/api/notifications/read-all` | Mark all notifications as read for current user |
| **DELETE** | `/api/notifications/{id}` | Soft delete a notification |

### 2.4. Smart Fare Alerts
| Method | Endpoint | Description |
| :--- | :--- | :--- |
| **POST** | `/api/fare-alerts` | Create a route fare tracking alert |
| **GET** | `/api/fare-alerts` | Get user active fare alerts with route price trend history |
| **PATCH** | `/api/fare-alerts/{id}` | Update target price or toggle alert active status |
| **DELETE** | `/api/fare-alerts/{id}` | Delete/cancel user fare alert |

---

## 3. Staff APIs (Role: PartnerOrStaff / StaffOnly)

APIs for airline staff and partners.

| Method | Endpoint | Description | Required Policy |
| :--- | :--- | :--- | :--- |
| **GET** | `/api/staff/flights` | Flight list with seat capacity summary for staff portal | `PartnerOrStaff` |
| **GET** | `/api/staff/flights/{id}/seats` | Flight seat map and reservation status | `PartnerOrStaff` |
| **PATCH** | `/api/flights/{id}/status-gate` | Update flight status, gate assignments, baggage carousel, delay minutes | `PartnerOrStaff` |
| **GET** | `/api/bookings` | List all bookings across the system with filtering | `PartnerOrStaff` |
| **POST** | `/api/staff/bookings` | Create a booking on behalf of a call-in customer | `StaffOnly` |
| **GET** | `/api/staff/sales` | Ticket sales board data | `StaffOnly` |
| **GET** | `/api/partner/staff/my-airline` | List all staff members belonging to the same airline | `PartnerOrStaff` |

---

## 4. Partner APIs (Role: PartnerOnly)

APIs exclusively for airline partners. Data operations are scoped by the partner's `AirlineId` claim.

### 4.1. Flight & Infrastructure Management
| Method | Endpoint | Description |
| :--- | :--- | :--- |
| **GET** | `/api/partner/routes` | List airline routes (paginated) |
| **POST** | `/api/partner/routes` | Create new route for airline |
| **PUT** | `/api/partner/routes/{id}` | Update existing route |
| **DELETE** | `/api/partner/routes/{id}` | Delete route |
| **GET** | `/api/partner/airplanes` | List airline airplanes (paginated) |
| **GET** | `/api/partner/airplanes/models` | List available aircraft models for airplane configuration |
| **POST** | `/api/partner/airplanes` | Create new airplane |
| **PUT** | `/api/partner/airplanes/{id}` | Update airplane |
| **DELETE** | `/api/partner/airplanes/{id}` | Delete airplane |
| **GET** | `/api/partner/flights` | List airline flights (paginated, filters: search, status, departureDate) |
| **POST** | `/api/partner/flights` | Create new flight |
| **PUT** | `/api/partner/flights/{id}` | Update flight departure time |
| **DELETE** | `/api/partner/flights/{id}` | Delete flight |
| **GET** | `/api/partner/aircraft` | List aircraft configurations (paginated) |
| **POST** | `/api/partner/aircraft` | Create aircraft configuration |
| **PUT** | `/api/partner/aircraft/{id}` | Update aircraft configuration |
| **DELETE** | `/api/partner/aircraft/{id}` | Delete aircraft configuration |
| **POST** | `/api/flights/admin` | Create flight (legacy partner endpoint) |

### 4.2. Booking & Promotion Management
| Method | Endpoint | Description |
| :--- | :--- | :--- |
| **GET** | `/api/partner/bookings` | List airline bookings (paginated) |
| **PUT** | `/api/partner/bookings/{id}` | Update booking status |
| **GET** | `/api/partner/coupons` | List airline coupons (paginated) |
| **POST** | `/api/partner/coupons` | Create airline coupon |
| **PUT** | `/api/partner/coupons/{id}` | Update airline coupon |
| **DELETE** | `/api/partner/coupons/{id}` | Delete airline coupon |
| **GET** | `/api/partner/campaigns` | List airline promotional campaigns (paginated) |
| **POST** | `/api/partner/campaigns` | Create airline campaign |
| **PUT** | `/api/partner/campaigns/{id}` | Update airline campaign |
| **DELETE** | `/api/partner/campaigns/{id}` | Delete airline campaign |

### 4.3. Personnel, Settings, Analytics & Logs
| Method | Endpoint | Description |
| :--- | :--- | :--- |
| **GET** | `/api/partner/staff` | List partner staff members (paginated) |
| **POST** | `/api/partner/staff` | Create new partner staff member |
| **PUT** | `/api/partner/staff/{id}` | Update partner staff member details |
| **DELETE** | `/api/partner/staff/{id}` | Delete partner staff member |
| **PATCH** | `/api/partner/staff/{id}/status` | Toggle partner staff active status |
| **GET** | `/api/partner/settings` | Get partner airline profile settings |
| **PUT** | `/api/partner/settings` | Update partner airline profile settings |
| **GET** | `/api/partner/dashboard` | Partner overview dashboard statistics |
| **GET** | `/api/partner/dashboard/sales-summary` | Partner sales summary metrics |
| **GET** | `/api/partner/dashboard/occupancy-rates` | Flight seat occupancy rates |
| **GET** | `/api/partner/dashboard/revenue-trends` | Revenue trends over time |
| **GET** | `/api/partner/logs` | Query system logs scoped to the partner airline |

---

## 5. Admin APIs (Role: AdminOnly)

APIs for system administrators with global access across all airline partitions.

### 5.1. User & Permission Management
| Method | Endpoint | Description |
| :--- | :--- | :--- |
| **GET** | `/api/admin/users` | List all users (paginated, filters: search, airlineId, roleId) |
| **POST** | `/api/admin/users` | Create user (any role: Admin, Partner, Staff, Customer) |
| **GET** | `/api/admin/users/{id}` | User profile details by ID |
| **PUT** | `/api/admin/users/{id}` | Update user profile |
| **DELETE** | `/api/admin/users/{id}` | Soft delete user |
| **PATCH** | `/api/admin/users/{id}/status` | Toggle user active status |
| **GET** | `/api/admin/users/permissions` | List all permissions (paginated) |

### 5.2. Flight Infrastructure Management
| Method | Endpoint | Description |
| :--- | :--- | :--- |
| **GET** | `/api/admin/airports` | List all airports (paginated, keyword search) |
| **POST** | `/api/admin/airports` | Create new airport |
| **PUT** | `/api/admin/airports/{id}` | Update airport details |
| **DELETE** | `/api/admin/airports/{id}` | Soft delete airport |
| **GET** | `/api/admin/airlines` | List all airlines (paginated) |
| **POST** | `/api/admin/airlines` | Create new airline |
| **PUT** | `/api/admin/airlines/{id}` | Update airline details |
| **DELETE** | `/api/admin/airlines/{id}` | Soft delete airline |
| **GET** | `/api/admin/aircraft-models` | List aircraft models (paginated) |
| **GET** | `/api/admin/aircraft-models/{id}` | Get aircraft model details and seat templates |
| **POST** | `/api/admin/aircraft-models` | Create aircraft model with seat templates |
| **PUT** | `/api/admin/aircraft-models/{id}` | Update aircraft model and seat templates |
| **DELETE** | `/api/admin/aircraft-models/{id}` | Delete aircraft model |
| **GET** | `/api/admin/flights` | List all system flights (paginated) |
| **POST** | `/api/admin/flights` | Create flight with automated seat inventory generation |
| **PUT** | `/api/admin/flights/{id}` | Update flight schedule |
| **DELETE** | `/api/admin/flights/{id}` | Cancel/delete flight |

### 5.3. Booking & Promotion Management
| Method | Endpoint | Description |
| :--- | :--- | :--- |
| **GET** | `/api/admin/bookings` | List all system bookings (paginated, filters: search, status, date) |
| **GET** | `/api/admin/bookings/{id}` | Get booking details by ID |
| **PUT** | `/api/admin/bookings/{id}` | Update booking passenger and contact information |
| **PUT** | `/api/admin/bookings/{id}/status` | Update booking status directly |
| **DELETE** | `/api/admin/bookings/{id}` | Cancel booking |
| **GET** | `/api/admin/coupons` | List global promotional coupons (paginated) |
| **POST** | `/api/admin/coupons` | Create global coupon |
| **PUT** | `/api/admin/coupons/{id}` | Update global coupon |
| **DELETE** | `/api/admin/coupons/{id}` | Delete global coupon |
| **GET** | `/api/admin/campaigns` | List global campaigns (paginated) |
| **POST** | `/api/admin/campaigns` | Create global campaign |
| **PUT** | `/api/admin/campaigns/{id}` | Update global campaign |
| **DELETE** | `/api/admin/campaigns/{id}` | Delete global campaign |

### 5.4. System, Dashboard, Notifications & Logs
| Method | Endpoint | Description | Required Policy |
| :--- | :--- | :--- | :--- |
| **GET** | `/api/admin/dashboard` | System-wide administrator metrics and critical activity logs | `AdminOnly` |
| **GET** | `/api/admin/settings` | Retrieve global system settings | `PartnerOnly` (or Admin) |
| **PUT** | `/api/admin/settings` | Update global system settings | `PartnerOnly` (or Admin) |
| **GET** | `/api/admin/logs` | Query all system logs (filters: level, search, airlineId, date) | `AdminOnly` |
| **GET** | `/api/admin/notifications/templates` | List all localized notification templates | `AdminOnly` |
| **GET** | `/api/admin/notifications/templates/{id}` | Get notification template details by ID | `AdminOnly` |
| **PUT** | `/api/admin/notifications/templates/{id}` | Update notification template content and locale | `AdminOnly` |

---

## 6. Realtime Hubs (SignalR WebSocket)

Real-time communication channels powered by ASP.NET Core SignalR and Redis backplane.

| Hub | URL | Authentication | Client → Server Methods | Server → Client Events | Description |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **SeatHub** | `/hubs/seats` | None | `JoinFlight(flightId)`, `LeaveFlight(flightId)` | `SeatUpdated(flightId, seatNumber, isAvailable)` | Real-time seat inventory locks and availability changes |
| **SupportChatHub** | `/hubs/support` | Optional (token or connection query) | `CustomerJoinChat(airlineId, customerName)`, `StaffRegister(airlineId, staffName)`, `SendMessageToAirline(message)`, `SendMessageToCustomer(connectionId, message)` | `SystemMessage(message)`, `AgentAssigned(staffName)`, `NewCustomerChat(connectionId, customerName)`, `ReceiveMessage(fromId, role, name, message)`, `CustomerDisconnected(connectionId, customerName)` | Live customer support chat with automatic staff load balancing |
| **NotificationHub** | `/hubs/notifications` | Required (JWT via `?access_token=...`) | *(none — server push)* | `ReceiveNotification(notification)` | Instant delivery of system, flight, booking, and alert notifications |
| **FareAlertHub** | `/hubs/fare-alerts` | Required (JWT via `?access_token=...`) | `SubscribeRoute(originAirportId, destinationAirportId, departureDate)`, `UnsubscribeRoute(originAirportId, destinationAirportId, departureDate)` | `PriceDropped(FareAlertNotificationDto payload)`, `RoutePriceUpdated(RoutePriceUpdateDto payload)` | Real-time price drop alerts and route price monitoring |
| **FlightTrackerHub** | `/hubs/flight-tracker` | None | `JoinFlightTracking(flightId)`, `LeaveFlightTracking(flightId)`, `JoinGlobalRadar()`, `LeaveGlobalRadar()` | `TelemetryUpdated(FlightTelemetryDto telemetry)`, `GlobalRadarTick(List<AircraftMapPinDto> activePlanes)`, `FlightStatusChanged(FlightStatusChangedDto statusUpdate)` | Live global radar map ticks, telemetry feeds, and flight status transitions |
