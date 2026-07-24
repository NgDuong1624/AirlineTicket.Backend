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

---

## API Reference

### Authentication Roles
- **AdminOnly**: Requires authentication as a System Admin.
- **PartnerOnly**: Requires authentication as an Airline Admin.

### Endpoints

| Method | Path | Auth | Description |
|--------|------|------|-------------|
| **POST** | `/api/admin/coupons` | AdminOnly | Create a new global coupon. |
| **GET** | `/api/admin/coupons` | AdminOnly | Get a paginated list of all global coupons. |
| **PUT** | `/api/admin/coupons/{id:guid}` | AdminOnly | Update an existing global coupon. |
| **DELETE** | `/api/admin/coupons/{id:guid}` | AdminOnly | Soft-delete a global coupon. |

*(Note: Partner-specific endpoints are available under `/api/partner/...` following similar CRUD operations but scoped to the partner's airline.)*
