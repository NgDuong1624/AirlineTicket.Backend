# Promotions Module

## Overview
The **Promotions Module** manages discount campaigns, coupons, and real-time Smart Fare Alerts. It supports both global (admin) and airline-scoped (partner) promotions, handles discount validation during booking checkout, and orchestrates user fare tracking with instant push notifications when prices drop.

---

## How It Works (For End Users & Clients)
1. **Campaign Creation**: System administrators and airline partners create promotional campaigns with a title, banner, content, and validity periods (`StartDate` to `EndDate`). Campaigns can be highlighted as featured.
2. **Coupon Management**: Coupons are created with unique codes and either associated with an airline or applied globally. Discount structures include:
   - **Percentage**: Percentage off the flight price.
   - **FixedAmount**: Fixed monetary discount value.
   - **Usage Conditions**: Minimum order values, maximum discount caps, and usage limits.
3. **Application & Verification**: During booking checkout, customers apply promotion codes (`POST /api/promotions/apply`). The system validates active dates, usage limits, and airline eligibility, returning calculated discount amounts.
4. **Smart Fare Alerts**: Customers set target price alerts on specific routes and dates (`POST /api/fare-alerts`). Background workers evaluate fare trends, and when a flight's lowest fare drops to or below the target price, alerts are broadcast via `FareAlertHub` (`/hubs/fare-alerts`) and pushed as notifications.

---

## Domain Entities & Data Model

### Campaign
Represents a promotional campaign.
- `Id` (Guid): Unique identifier.
- `Title` (string): Campaign title.
- `BannerUrl` (string?): URL to campaign banner image.
- `Content` (string?): Campaign details or description.
- `StartDate` (DateTime): Campaign start date.
- `EndDate` (DateTime): Campaign end date.
- `IsFeatured` (bool): Whether the campaign is featured on the client portal.
- `IsDeleted` (bool): Soft delete flag.
- `AirlineId` (Guid?): FK to the airline if airline-specific (null for global).

### Coupon
Represents a discount coupon.
- `Id` (Guid): Unique identifier.
- `Code` (string): Unique uppercase coupon code.
- `Description` (string?): Coupon description and terms.
- `DiscountType` (DiscountType): Type of discount (`0` = Percentage, `1` = FixedAmount).
- `DiscountValue` (decimal): Discount value (percentage or fixed amount).
- `MinOrderValue` (decimal?): Minimum order value required.
- `MaxDiscountAmount` (decimal?): Maximum discount cap for percentage discounts.
- `StartDate` (DateTime): Coupon validity start date.
- `EndDate` (DateTime): Coupon validity end date.
- `UsageLimit` (int?): Maximum allowed usage count (null for unlimited).
- `UsageCount` (int): Number of times coupon has been redeemed.
- `IsActive` (bool): Active status.
- `IsDeleted` (bool): Soft delete flag.
- `AirlineId` (Guid?): FK to airline if partner-scoped (null for global).

### FareAlert
Represents a customer's fare tracking subscription on a route.
- `Id` (Guid): Unique identifier.
- `UserId` (Guid): FK to `users.users`.
- `OriginAirportId` (Guid): FK to `flights.airports`.
- `DestinationAirportId` (Guid): FK to `flights.airports`.
- `DepartureDate` (DateTime): Scheduled departure date.
- `ReturnDate` (DateTime?): Optional return date for round trips.
- `TargetPrice` (decimal): Customer's desired price threshold.
- `CurrentLowestPrice` (decimal): Current lowest tracked price.
- `LastNotifiedPrice` (decimal?): Lowest fare at time of last notification.
- `Currency` (string): Currency code (default: `VND`).
- `IsActive` (bool): Whether alert is actively tracked.
- `LastCheckedAt` (DateTime): Timestamp of last evaluation worker pass.
- `LastNotifiedAt` (DateTime?): Timestamp of last notification dispatch.

---

## API Reference

### Authentication Roles
- **AdminOnly**: Requires authentication as a System Admin.
- **PartnerOnly**: Requires authentication as an Airline Partner.
- **Authenticated**: Requires a valid JWT Bearer token.

### Endpoints

#### Public Customer Endpoints
| Method | Path | Auth | Description | Input Type | Output Type |
|--------|------|------|-------------|------------|-------------|
| **GET** | `/api/promotions` | None | Get active public promotions. | None | `List<PromotionDto>` |
| **GET** | `/api/promotions/campaigns` | None | Get active promotion campaigns. | None | `List<CampaignDto>` |
| **POST** | `/api/promotions/apply` | None | Validate and apply a promotion coupon to a flight booking. | `ApplyPromotionRequest { promoCode: string, flightId: Guid, originalAmount: decimal }` | `ApplyPromotionResultDto` |

#### Customer Smart Fare Alert Endpoints
| Method | Path | Auth | Description | Input Type | Output Type |
|--------|------|------|-------------|------------|-------------|
| **POST** | `/api/fare-alerts` | Authenticated | Create route fare tracking alert. | `CreateFareAlertRequest { originAirportId: Guid, destinationAirportId: Guid, departureDate: DateTime, returnDate?: DateTime, targetPrice: decimal, currentLowestPrice: decimal, currency: string }` | `{ id: Guid }` |
| **GET** | `/api/fare-alerts` | Authenticated | Get current user's fare alerts with price history trends. | None | `List<FareAlertDto>` |
| **PATCH** | `/api/fare-alerts/{id}` | Authenticated | Update target price or toggle alert active status. | Route param `id: Guid`, `UpdateFareAlertRequest { targetPrice?: decimal, isActive?: bool }` | `{ success: true }` |
| **DELETE** | `/api/fare-alerts/{id}` | Authenticated | Delete/cancel a user fare alert. | Route param `id: Guid` | `{ success: true }` |

#### Partner Endpoints (`PartnerOnly` Role)
| Method | Path | Auth | Description | Input Type | Output Type |
|--------|------|------|-------------|------------|-------------|
| **GET** | `/api/partner/coupons` | PartnerOnly | Get paginated list of airline-scoped coupons. | Query params `pageIndex?: number, pageSize?: number` | `PagedResult<CouponDto>` |
| **POST** | `/api/partner/coupons` | PartnerOnly | Create a new airline-scoped coupon. | `PartnerCouponRequest { code: string, description?: string, discountType?: int, discountValue: decimal, minOrderValue?: decimal, maxDiscountAmount?: decimal, startDate: DateTime, endDate: DateTime, usageLimit?: int, isActive?: bool }` | `{ id: Guid }` |
| **PUT** | `/api/partner/coupons/{id}` | PartnerOnly | Update an existing airline-scoped coupon. | Route param `id: Guid`, `PartnerCouponRequest` | `200 OK` |
| **DELETE** | `/api/partner/coupons/{id}` | PartnerOnly | Delete an airline-scoped coupon. | Route param `id: Guid` | `204 NoContent` |
| **GET** | `/api/partner/campaigns` | PartnerOnly | Get paginated list of airline-scoped campaigns. | Query params `pageIndex?: number, pageSize?: number` | `PagedResult<CampaignDto>` |
| **POST** | `/api/partner/campaigns` | PartnerOnly | Create a new airline-scoped campaign. | `PartnerCampaignRequest { title: string, bannerUrl?: string, content?: string, startDate: DateTime, endDate: DateTime, isFeatured?: bool }` | `{ id: Guid }` |
| **PUT** | `/api/partner/campaigns/{id}` | PartnerOnly | Update an existing airline-scoped campaign. | Route param `id: Guid`, `PartnerCampaignRequest` | `200 OK` |
| **DELETE** | `/api/partner/campaigns/{id}` | PartnerOnly | Delete an airline-scoped campaign. | Route param `id: Guid` | `204 NoContent` |

#### Admin Endpoints (`AdminOnly` Role)
| Method | Path | Auth | Description | Input Type | Output Type |
|--------|------|------|-------------|------------|-------------|
| **GET** | `/api/admin/coupons` | AdminOnly | Get paginated list of all global coupons. | Query params `pageIndex?: number, pageSize?: number` | `PagedResult<CouponDto>` |
| **POST** | `/api/admin/coupons` | AdminOnly | Create a new global coupon. | `CreatePromotionRequest { name: string, promoCode: string, discountType: string, discountValue: decimal, maxUsage: int, startDate: DateTime, endDate: DateTime }` | `{ id: Guid }` |
| **PUT** | `/api/admin/coupons/{id}` | AdminOnly | Update an existing global coupon. | Route param `id: Guid`, `UpdatePromotionRequest { name: string, discountValue: decimal, endDate: DateTime }` | `{ message: string }` |
| **DELETE** | `/api/admin/coupons/{id}` | AdminOnly | Delete a global coupon. | Route param `id: Guid` | `204 NoContent` |
| **GET** | `/api/admin/campaigns` | AdminOnly | Get paginated list of all global campaigns. | Query params `pageIndex?: number, pageSize?: number` | `PagedResult<CampaignDto>` |
| **POST** | `/api/admin/campaigns` | AdminOnly | Create a new global campaign. | `AdminCampaignRequest { title: string, bannerUrl?: string, content?: string, startDate: DateTime, endDate: DateTime, isFeatured?: bool }` | `{ id: Guid }` |
| **PUT** | `/api/admin/campaigns/{id}` | AdminOnly | Update an existing global campaign. | Route param `id: Guid`, `AdminCampaignRequest` | `200 OK` |
| **DELETE** | `/api/admin/campaigns/{id}` | AdminOnly | Delete a global campaign. | Route param `id: Guid` | `204 NoContent` |

---

## Real-Time Smart Fare Alerts (SignalR)

### Hub Endpoint
- **URL**: `/hubs/fare-alerts`
- **Authentication**: Required (JWT Bearer via query string `?access_token=...`)

### Client → Server Methods
- `SubscribeRoute(string originAirportId, string destinationAirportId, string departureDate)`: Join a real-time price monitoring channel for a specific route and departure date.
- `UnsubscribeRoute(string originAirportId, string destinationAirportId, string departureDate)`: Leave the route group.

### Server → Client Events
- `PriceDropped(FareAlertNotificationDto payload)`: Dispatched to the user group when a tracked route price drops below the user's target price.
- `RoutePriceUpdated(RoutePriceUpdateDto payload)`: Broadcast to subscribers when the lowest price for a route changes.
