# Promotions Module

## Overview
The **Promotions Module** manages discount campaigns and coupons, supporting both global (admin) and airline-scoped (partner) promotions. It provides functionalities to create, update, and manage promotional offers to drive sales and customer engagement.

---

## How It Works (For End Users & Clients)
1. **Campaign Creation**: System administrators or airline partners can create promotional campaigns with a title, banner, content, and a validity period (`StartDate` to `EndDate`). Campaigns can be marked as featured.
2. **Coupon Management**: Coupons are generated with unique codes and associated with a campaign or used standalone. Coupons can have different discount types:
   - **Percentage**: A percentage off the total price.
   - **FixedAmount**: A fixed monetary amount off the total price.
   - **Conditions**: Coupons can have minimum order values, maximum discount amounts, and usage limits.
3. **Application**: During the booking process, users can apply a valid coupon code to receive discounts. The system validates the coupon's validity, usage count, and applicable conditions.
4. **Automated Updates**: A background service (`PromotionStatusUpdaterJob`) periodically updates the status of promotions and coupons based on their start and end dates.

---

## Domain Entities & Data Model

### Campaign
Represents a promotional campaign.
- `Id` (Guid): Unique identifier.
- `Title` (string): Campaign title.
- `BannerUrl` (string?): URL to the campaign's banner image.
- `Content` (string?): Campaign details or description.
- `StartDate` (DateTime): Campaign start date.
- `EndDate` (DateTime): Campaign end date.
- `IsFeatured` (bool): Whether the campaign is featured on the front page.
- `IsDeleted` (bool): Soft delete flag.
- `AirlineId` (Guid?): FK to the airline, if the campaign is airline-specific. Null for global campaigns.

### Coupon
Represents a discount coupon.
- `Id` (Guid): Unique identifier.
- `Code` (string): Unique coupon code.
- `Description` (string?): Description of the coupon's benefits.
- `DiscountType` (DiscountType): Type of discount (`0` = Percentage, `1` = FixedAmount).
- `DiscountValue` (decimal): The value of the discount (percentage or fixed amount).
- `MinOrderValue` (decimal?): Minimum order value required to use the coupon.
- `MaxDiscountAmount` (decimal?): Maximum discount amount allowed for percentage-based coupons.
- `StartDate` (DateTime): Coupon validity start date.
- `EndDate` (DateTime): Coupon validity end date.
- `UsageLimit` (int?): Maximum number of times the coupon can be used. Null for unlimited.
- `UsageCount` (int): Current number of times the coupon has been used.
- `IsActive` (bool): Whether the coupon is currently active.
- `IsDeleted` (bool): Soft delete flag.
- `AirlineId` (Guid?): FK to the airline, if the coupon is airline-specific. Null for global coupons.

### FareAlert
Represents a user fare tracking alert for a route.
- `Id` (Guid): Unique identifier.
- `UserId` (Guid): FK to `users.users`.
- `OriginAirportId` (Guid): FK to `flights.airports`.
- `DestinationAirportId` (Guid): FK to `flights.airports`.
- `DepartureDate` (DateTime): Scheduled departure date.
- `ReturnDate` (DateTime?): Optional return date.
- `TargetPrice` (decimal): Desired notification price.
- `CurrentLowestPrice` (decimal): Current lowest tracked fare.
- `LastNotifiedPrice` (decimal?): Fare price at last notification.
- `Currency` (string): Currency code (default: `VND`).
- `IsActive` (bool): Active tracking status.
- `LastCheckedAt` (DateTime): Timestamp of last worker evaluation.
- `LastNotifiedAt` (DateTime?): Timestamp of last notification (24h cooldown).

---

## API Reference

### Authentication Roles
- **AdminOnly**: Requires authentication as a System Admin.
- **PartnerOnly**: Requires authentication as an Airline Admin.

### Endpoints

#### Public / Customer Endpoints
| Method | Path | Auth | Description | Input Type | Output Type |
|--------|------|------|-------------|------------|-------------|
| **GET** | `/api/promotions` | None | Get all available promotion coupons. | None | `{ coupons: Coupon[] { id: string, code: string, description?: string, discountType: number, discountValue: number, minOrderValue?: number, maxDiscountAmount?: number, startDate: string, endDate: string, usageLimit?: number, usageCount: number, isActive: boolean } }` |
| **GET** | `/api/promotions/campaigns` | None | Get all active promotional campaigns. | None | `{ campaigns: Campaign[] { id: string, title: string, bannerUrl?: string, content?: string, startDate: string, endDate: string, isFeatured: boolean, airlineId?: string \| null } }` |

#### Customer Fare Alert Endpoints (Authenticated)
| Method | Path | Auth | Description | Input Type | Output Type |
|--------|------|------|-------------|------------|-------------|
| **POST** | `/api/fare-alerts` | User | Create route fare tracking alert. | `{ originAirportId: string, destinationAirportId: string, departureDate: string, returnDate?: string, targetPrice: number, currency?: string }` | `FareAlertDto` |
| **GET** | `/api/fare-alerts` | User | Get user fare alerts with price history trends. | None | `FareAlertDto[]` |
| **PATCH** | `/api/fare-alerts/{id}` | User | Update target price or toggle status. | `{ targetPrice?: number, isActive?: boolean }` | `FareAlertDto` |
| **DELETE** | `/api/fare-alerts/{id}` | User | Delete/cancel user fare alert. | None | `void` |

#### Partner Endpoints (`PartnerOnly` Role)
| Method | Path | Auth | Description | Input Type | Output Type |
|--------|------|------|-------------|------------|-------------|
| **GET** | `/api/partner/coupons` | PartnerOnly | Get a paginated list of airline-scoped coupons. | None (Query params `pageIndex: number, pageSize: number`) | `PagedResult<Coupon> { items: Coupon[], totalCount: number, pageNumber: number, pageSize: number, totalPages: number, hasNextPage: boolean, hasPreviousPage: boolean }` |
| **POST** | `/api/partner/coupons` | PartnerOnly | Create a new airline-scoped coupon. | `Partial<Coupon> { code: string, description?: string, discountType: number, discountValue: number, minOrderValue?: number, maxDiscountAmount?: number, startDate: string, endDate: string, usageLimit?: number, isActive: boolean }` | `Coupon` |
| **PUT** | `/api/partner/coupons/{id}` | PartnerOnly | Update an existing airline-scoped coupon. | `Partial<Coupon>` | `Coupon` |
| **DELETE** | `/api/partner/coupons/{id}` | PartnerOnly | Soft-delete an airline-scoped coupon. | None | `void` |
| **GET** | `/api/partner/campaigns` | PartnerOnly | Get a paginated list of airline-scoped campaigns. | None (Query params `pageIndex: number, pageSize: number`) | `PagedResult<Campaign> { items: Campaign[], totalCount: number, pageNumber: number, pageSize: number, totalPages: number, hasNextPage: boolean, hasPreviousPage: boolean }` |
| **POST** | `/api/partner/campaigns` | PartnerOnly | Create a new airline-scoped campaign. | `Partial<Campaign> { title: string, bannerUrl?: string, content?: string, startDate: string, endDate: string, isFeatured: boolean }` | `Campaign` |
| **PUT** | `/api/partner/campaigns/{id}` | PartnerOnly | Update an existing airline-scoped campaign. | `Partial<Campaign>` | `Campaign` |
| **DELETE** | `/api/partner/campaigns/{id}` | PartnerOnly | Soft-delete an airline-scoped campaign. | None | `void` |

#### Admin Endpoints (`AdminOnly` Role)
| Method | Path | Auth | Description | Input Type | Output Type |
|--------|------|------|-------------|------------|-------------|
| **GET** | `/api/admin/coupons` | AdminOnly | Get a paginated list of all global coupons. | None (Query params `pageIndex: number, pageSize: number`) | `PagedResult<Coupon> { items: Coupon[], totalCount: number, pageNumber: number, pageSize: number, totalPages: number, hasNextPage: boolean, hasPreviousPage: boolean }` |
| **POST** | `/api/admin/coupons` | AdminOnly | Create a new global coupon. | `Partial<Coupon> { code: string, description?: string, discountType: number, discountValue: number, minOrderValue?: number, maxDiscountAmount?: number, startDate: string, endDate: string, usageLimit?: number, isActive: boolean }` | `Coupon` |
| **PUT** | `/api/admin/coupons/{id}` | AdminOnly | Update an existing global coupon. | `Partial<Coupon>` | `Coupon` |
| **DELETE** | `/api/admin/coupons/{id}` | AdminOnly | Soft-delete a global coupon. | None | `void` |
| **GET** | `/api/admin/campaigns` | AdminOnly | Get a paginated list of all global campaigns. | None (Query params `pageIndex: number, pageSize: number`) | `PagedResult<Campaign> { items: Campaign[], totalCount: number, pageNumber: number, pageSize: number, totalPages: number, hasNextPage: boolean, hasPreviousPage: boolean }` |
| **POST** | `/api/admin/campaigns` | AdminOnly | Create a new global campaign. | `Partial<Campaign> { title: string, bannerUrl?: string, content?: string, startDate: string, endDate: string, isFeatured: boolean }` | `Campaign` |
| **PUT** | `/api/admin/campaigns/{id}` | AdminOnly | Update an existing global campaign. | `Partial<Campaign>` | `Campaign` |
| **DELETE** | `/api/admin/campaigns/{id}` | AdminOnly | Soft-delete a global campaign. | None | `void` |
