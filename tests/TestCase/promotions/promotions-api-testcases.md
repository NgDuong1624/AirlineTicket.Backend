# Promotions & Fare Alerts Module API Test Cases

## Module Overview
- **Base Paths**: `/api/promotions`, `/api/fare-alerts`, `/api/partner/coupons`, `/api/partner/campaigns`, `/api/admin/coupons`, `/api/admin/campaigns`
- **Authentication**: Public, Customer (Authenticated), PartnerOnly, AdminOnly
- **Enums**: DiscountType (`0`: Percentage, `1`: FixedAmount)

---

## Checklist Summary

- [ ] **GET /api/promotions**
  - [ ] `TC-PRM-CUP-001`: Public list all available active coupons returns 200 OK
  - [ ] `TC-PRM-CUP-002`: Expired or reached-limit coupons excluded from public list

- [ ] **GET /api/promotions/campaigns**
  - [ ] `TC-PRM-CMP-001`: Public list active promotional campaigns returns 200 OK
  - [ ] `TC-PRM-CMP-002`: Campaigns with `isFeatured: true` prioritized

- [ ] **Customer Fare Alerts (`/api/fare-alerts`)**
  - [ ] `TC-PRM-ALR-001`: `POST /api/fare-alerts` creates route fare tracking alert returns 201 Created
  - [ ] `TC-PRM-ALR-002`: `POST /api/fare-alerts` with target price <= 0 returns 400 Bad Request
  - [ ] `TC-PRM-ALR-003`: `POST /api/fare-alerts` with past departure date returns 400 Bad Request
  - [ ] `TC-PRM-ALR-004`: `GET /api/fare-alerts` returns user's active fare alerts with price history trends
  - [ ] `TC-PRM-ALR-005`: `PATCH /api/fare-alerts/{id}` updates target price or toggles active status returns 200 OK
  - [ ] `TC-PRM-ALR-006`: `PATCH /api/fare-alerts/{id}` for another user's alert returns 403/404
  - [ ] `TC-PRM-ALR-007`: `DELETE /api/fare-alerts/{id}` cancels fare alert returns 200/204
  - [ ] `TC-PRM-ALR-008`: Unauthenticated requests to `/api/fare-alerts` return 401 Unauthorized

- [ ] **Partner Coupons Management (`/api/partner/coupons`)**
  - [ ] `TC-PRM-PART-001`: Partner gets paginated coupons scoped to airline returns 200 OK
  - [ ] `TC-PRM-PART-002`: Partner creates airline-scoped coupon returns 201 Created
  - [ ] `TC-PRM-PART-003`: Partner creates coupon with duplicate code returns 409 Conflict
  - [ ] `TC-PRM-PART-004`: Partner updates coupon details returns 200 OK
  - [ ] `TC-PRM-PART-005`: Partner soft deletes coupon returns 200/204
  - [ ] `TC-PRM-PART-006`: Partner updating/deleting coupon of another airline returns 403/404

- [ ] **Partner Campaigns Management (`/api/partner/campaigns`)**
  - [ ] `TC-PRM-PART-007`: Partner gets paginated campaigns scoped to airline returns 200 OK
  - [ ] `TC-PRM-PART-008`: Partner creates airline-scoped campaign returns 201 Created
  - [ ] `TC-PRM-PART-009`: Partner updates campaign details returns 200 OK
  - [ ] `TC-PRM-PART-010`: Partner soft deletes campaign returns 200/204

- [ ] **Admin Coupons Management (`/api/admin/coupons`)**
  - [ ] `TC-PRM-ADM-001`: Admin lists all global coupons paginated returns 200 OK
  - [ ] `TC-PRM-ADM-002`: Admin creates global coupon returns 201 Created
  - [ ] `TC-PRM-ADM-003`: Admin updates global coupon returns 200 OK
  - [ ] `TC-PRM-ADM-004`: Admin deletes global coupon returns 200/204

- [ ] **Admin Campaigns Management (`/api/admin/campaigns`)**
  - [ ] `TC-PRM-ADM-005`: Admin lists all global campaigns paginated returns 200 OK
  - [ ] `TC-PRM-ADM-006`: Admin creates global campaign returns 201 Created
  - [ ] `TC-PRM-ADM-007`: Admin updates global campaign returns 200 OK
  - [ ] `TC-PRM-ADM-008`: Admin deletes global campaign returns 200/204

---

## Detailed Test Case Specifications

### TC-PRM-ALR-001: Customer creates route fare alert
- **Endpoint**: `POST /api/fare-alerts`
- **Auth**: Customer JWT
- **Request Body**:
  ```json
  {
    "originAirportId": "1fa85f64-5717-4562-b3fc-2c963f66afa6",
    "destinationAirportId": "2fa85f64-5717-4562-b3fc-2c963f66afa6",
    "departureDate": "2026-11-15T00:00:00Z",
    "targetPrice": 50.00,
    "currency": "USD"
  }
  ```
- **Assertions**:
  - HTTP Status: `200 OK` or `201 Created`
  - Response contains `id`, `targetPrice: 50.00`, `isActive: true`.

### TC-PRM-PART-002: Partner creates airline coupon
- **Endpoint**: `POST /api/partner/coupons`
- **Auth**: Partner JWT (AirlineId `A1`)
- **Request Body**:
  ```json
  {
    "code": "FLYVN20",
    "description": "20% discount on autumn flights",
    "discountType": 0,
    "discountValue": 20,
    "minOrderValue": 100,
    "maxDiscountAmount": 50,
    "startDate": "2026-09-01T00:00:00Z",
    "endDate": "2026-10-31T23:59:59Z",
    "usageLimit": 500,
    "isActive": true
  }
  ```
- **Assertions**:
  - HTTP Status: `201 Created`
  - `airlineId` is auto-populated with `A1`.
  - `code` is saved as `"FLYVN20"`.

### TC-PRM-ADM-002: Admin creates global coupon
- **Endpoint**: `POST /api/admin/coupons`
- **Auth**: Admin JWT
- **Request Body**:
  ```json
  {
    "code": "GLOBAL10",
    "description": "10 USD off for any booking",
    "discountType": 1,
    "discountValue": 10,
    "minOrderValue": 50,
    "startDate": "2026-09-01T00:00:00Z",
    "endDate": "2026-12-31T23:59:59Z",
    "isActive": true
  }
  ```
- **Assertions**:
  - HTTP Status: `201 Created`
  - `airlineId` is null (global promotion).
