# Airline Ticket Booking System API List

This document provides a **comprehensive** list of all API endpoints in the system, categorized by access role.

---

## 1. Public APIs (Anonymous)

APIs that do not require authentication, serving public-facing functionalities.

### 1.1. Authentication & Account
| Method | Endpoint | Description |
| :--- | :--- | :--- |
| **POST** | `/api/auth/login` | User login (returns JWT token) |
| **POST** | `/api/auth/google` | Login with Google |
| **POST** | `/api/auth/register` | Register new account |
| **POST** | `/api/auth/refresh` | Refresh access token |
| **POST** | `/api/auth/logout` | Logout and revoke session |

### 1.2. Flight Search
| Method | Endpoint | Description |
| :--- | :--- | :--- |
| **POST** | `/api/flights` | Search for flights |
| **POST** | `/api/flights/round-trip` | Search for round-trip flights |
| **GET** | `/api/flights/{id}` | Flight details |
| **GET** | `/api/flights/trending` | Trending flights |
| **GET** | `/api/flights/{id}/seats` | Flight seat map |
| **GET** | `/api/airports` | List/search airports |
| **GET** | `/api/airlines` | List airlines |
| **GET** | `/api/routes` | List routes |

### 1.3. Booking & Payment
| Method | Endpoint | Description |
| :--- | :--- | :--- |
| **POST** | `/api/bookings` | Create a new booking |
| **GET** | `/api/bookings/{id}` | Booking details |
| **PUT** | `/api/bookings/{id}` | Update booking information |
| **DELETE** | `/api/bookings/{id}` | Cancel booking |
| **GET** | `/api/bookings/search` | Search booking (by PNR) |
| **POST** | `/api/bookings/{id}/pay` | Process booking payment |
| **GET** | `/api/tickets/{id}` | Get e-ticket information |

### 1.4. Promotions
| Method | Endpoint | Description |
| :--- | :--- | :--- |
| **GET** | `/api/promotions` | List active promotions |
| **GET** | `/api/promotions/campaigns` | List promotion campaigns |
| **POST** | `/api/promotions/apply` | Apply promotion code |

### 1.5. System & Interactions
| Method | Endpoint | Description |
| :--- | :--- | :--- |
| **GET** | `/api/health/live` | Check liveness |
| **GET** | `/api/health` | Check readiness |
| **GET** | `/api/notifications/status` | Notification module status |
| **GET** | `/api/v1/interactions` | Interactions module status |
| **POST** | `/api/v1/qa/ask` | Ask AI Travel Assistant a question |

---

## 2. Authenticated APIs (Logged-in Users)

APIs requiring a valid JWT token, regardless of user role.

| Method | Endpoint | Description |
| :--- | :--- | :--- |
| **GET** | `/api/auth/me` | Currently logged-in user's information |
| **PUT** | `/api/auth/language` | Update preferred language |
| **GET** | `/api/bookings/user/{id}` | User's booking history |
| **GET** | `/api/bookings/user/{id}/stats` | User's booking statistics |
| **GET** | `/api/notifications` | Paginated list of user notifications |
| **GET** | `/api/notifications/unread-count` | Unread notification count |
| **PUT** | `/api/notifications/{id}/read` | Mark a specific notification as read |
| **PUT** | `/api/notifications/read-all` | Mark all notifications as read |
| **DELETE** | `/api/notifications/{id}` | Soft delete a notification |

---

## 3. Staff APIs (Role: PartnerOrStaff / StaffOnly)

APIs for airline staff or partners.

| Method | Endpoint | Description | Auth |
| :--- | :--- | :--- | :--- |
| **GET** | `/api/staff/flights` | Flight list (staff portal) | PartnerOrStaff |
| **GET** | `/api/staff/flights/{id}/seats` | Flight seat map | PartnerOrStaff |
| **GET** | `/api/bookings` | List all bookings | PartnerOrStaff |
| **POST** | `/api/staff/bookings` | Create booking for customer | StaffOnly |
| **GET** | `/api/staff/sales` | Ticket sales board | StaffOnly |

---

## 4. Partner APIs (Role: PartnerOnly)

APIs exclusively for partners (airline management). Data is typically scoped by the partner's `AirlineId`.

### 4.1. Flight & Infrastructure Management
| Method | Endpoint | Description |
| :--- | :--- | :--- |
| **GET** | `/api/partner/routes` | List routes |
| **POST** | `/api/partner/routes` | Create new route |
| **PUT** | `/api/partner/routes/{id}` | Update route |
| **DELETE** | `/api/partner/routes/{id}` | Delete route |
| **GET** | `/api/partner/airplanes` | List airplanes |
| **GET** | `/api/partner/airplanes/models` | List aircraft models |
| **POST** | `/api/partner/airplanes` | Create new airplane |
| **PUT** | `/api/partner/airplanes/{id}` | Update airplane |
| **DELETE** | `/api/partner/airplanes/{id}` | Delete airplane |
| **GET** | `/api/partner/flights` | List flights |
| **POST** | `/api/partner/flights` | Create flight |
| **PUT** | `/api/partner/flights/{id}` | Update flight |
| **DELETE** | `/api/partner/flights/{id}` | Delete flight |
| **GET** | `/api/partner/aircraft` | List aircraft configurations |
| **POST** | `/api/partner/aircraft` | Create aircraft configuration |
| **PUT** | `/api/partner/aircraft/{id}` | Update aircraft configuration |
| **DELETE** | `/api/partner/aircraft/{id}` | Delete aircraft configuration |
| **POST** | `/api/flights/admin` | Create flight (Partner) |

### 4.2. Booking & Promotion Management
| Method | Endpoint | Description |
| :--- | :--- | :--- |
| **GET** | `/api/partner/bookings` | List partner's bookings |
| **PUT** | `/api/partner/bookings/{id}` | Update booking status |
| **GET** | `/api/partner/coupons` | List coupons |
| **POST** | `/api/partner/coupons` | Create new coupon |
| **PUT** | `/api/partner/coupons/{id}` | Update coupon |
| **DELETE** | `/api/partner/coupons/{id}` | Delete coupon |
| **GET** | `/api/partner/campaigns` | List campaigns |
| **POST** | `/api/partner/campaigns` | Create new campaign |
| **PUT** | `/api/partner/campaigns/{id}` | Update campaign |
| **DELETE** | `/api/partner/campaigns/{id}` | Delete campaign |

### 4.3. Personnel & System
| Method | Endpoint | Description |
| :--- | :--- | :--- |
| **GET** | `/api/partner/staff` | List staff members |
| **POST** | `/api/partner/staff` | Create new staff member |
| **PUT** | `/api/partner/staff/{id}` | Update staff member |
| **DELETE** | `/api/partner/staff/{id}` | Delete staff member |
| **PATCH** | `/api/partner/staff/{id}/status` | Update staff member status |
| **GET** | `/api/partner/staff/my-airline` | List all members of the same airline |
| **GET** | `/api/partner/settings` | View airline information |
| **PUT** | `/api/partner/settings` | Update airline information |
| **GET** | `/api/partner/dashboard` | Partner dashboard statistics |
| **GET** | `/api/partner/dashboard/sales-summary` | Sales summary overview |
| **GET** | `/api/partner/dashboard/occupancy-rates` | Occupancy rates |
| **GET** | `/api/partner/dashboard/revenue-trends` | Revenue trends |
| **GET** | `/api/partner/logs` | Partner's system logs |

---

## 5. Admin APIs (Role: AdminOnly)

APIs for system administrators, with full access privileges.

### 5.1. User & Permission Management
| Method | Endpoint | Description |
| :--- | :--- | :--- |
| **GET** | `/api/admin/users` | List users |
| **POST** | `/api/admin/users` | Create user |
| **GET** | `/api/admin/users/{id}` | User details |
| **PUT** | `/api/admin/users/{id}` | Update user |
| **DELETE** | `/api/admin/users/{id}` | Delete user |
| **PATCH** | `/api/admin/users/{id}/status` | Update user status |
| **GET** | `/api/admin/users/permissions` | List permissions |

### 5.2. Flight Infrastructure Management
| Method | Endpoint | Description |
| :--- | :--- | :--- |
| **GET** | `/api/admin/airports` | List airports |
| **POST** | `/api/admin/airports` | Create airport |
| **PUT** | `/api/admin/airports/{id}` | Update airport |
| **DELETE** | `/api/admin/airports/{id}` | Delete airport |
| **GET** | `/api/admin/airlines` | List airlines |
| **POST** | `/api/admin/airlines` | Create airline |
| **PUT** | `/api/admin/airlines/{id}` | Update airline |
| **DELETE** | `/api/admin/airlines/{id}` | Delete airline |
| **GET** | `/api/admin/aircraft-models` | List aircraft models |
| **GET** | `/api/admin/aircraft-models/{id}` | Aircraft model details |
| **POST** | `/api/admin/aircraft-models` | Create aircraft model |
| **PUT** | `/api/admin/aircraft-models/{id}` | Update aircraft model |
| **DELETE** | `/api/admin/aircraft-models/{id}` | Delete aircraft model |
| **GET** | `/api/admin/flights` | List flights |
| **POST** | `/api/admin/flights` | Create flight |
| **PUT** | `/api/admin/flights/{id}` | Update flight |
| **DELETE** | `/api/admin/flights/{id}` | Delete flight |

### 5.3. Booking & Promotion Management
| Method | Endpoint | Description |
| :--- | :--- | :--- |
| **GET** | `/api/admin/bookings` | List bookings |
| **GET** | `/api/admin/bookings/{id}` | Booking details |
| **PUT** | `/api/admin/bookings/{id}` | Update booking |
| **PUT** | `/api/admin/bookings/{id}/status` | Update booking status |
| **DELETE** | `/api/admin/bookings/{id}` | Cancel booking |
| **GET** | `/api/admin/coupons` | List coupons |
| **POST** | `/api/admin/coupons` | Create coupon |
| **PUT** | `/api/admin/coupons/{id}` | Update coupon |
| **DELETE** | `/api/admin/coupons/{id}` | Delete coupon |
| **GET** | `/api/admin/campaigns` | List campaigns |
| **POST** | `/api/admin/campaigns` | Create campaign |
| **PUT** | `/api/admin/campaigns/{id}` | Update campaign |
| **DELETE** | `/api/admin/campaigns/{id}` | Delete campaign |

### 5.4. System & Dashboard
| Method | Endpoint | Description |
| :--- | :--- | :--- |
| **GET** | `/api/admin/dashboard` | Admin dashboard statistics |
| **GET** | `/api/admin/settings` | View system settings |
| **PUT** | `/api/admin/settings` | Update system settings |
| **GET** | `/api/admin/logs` | System logs |
| **GET** | `/api/admin/notifications/templates` | List notification templates |
| **GET** | `/api/admin/notifications/templates/{id}` | Notification template details |
| **PUT** | `/api/admin/notifications/templates/{id}` | Update notification template |

---

## 6. Realtime Hubs (SignalR WebSocket)

| Hub | URL | Methods | Description |
| :--- | :--- | :--- | :--- |
| **SeatHub** | `/hubs/seats` | `JoinFlight(flightId)`, `LeaveFlight(flightId)` | Real-time seat tracking. Server → Client: `SeatUpdated(flightId, seatNumber, isAvailable)` |
| **SupportChatHub** | `/hubs/support` | `CustomerJoinChat(airlineId, customerName)`, `StaffRegister(airlineId, staffName)`, `SendMessageToAirline(message)`, `SendMessageToCustomer(connectionId, message)` | Live chat Customer ↔ Staff, auto-assign |
| **NotificationHub** | `/hubs/notifications` | Client → Server: (none — server push only) | Push real-time notifications to users. Server → Client: `ReceiveNotification(notification)` |
