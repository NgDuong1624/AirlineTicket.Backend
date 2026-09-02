# CMS Module API Test Cases

## Module Overview
- **Base Paths**: `/api/admin/dashboard`, `/api/admin/settings`, `/api/partner/dashboard`
- **Authentication**: AdminOnly, PartnerOnly

---

## Checklist Summary

- [ ] **GET /api/admin/dashboard**
  - [ ] `TC-CMS-ADM-DASH-001`: Admin gets system-wide aggregated metrics returns 200 OK
  - [ ] `TC-CMS-ADM-DASH-002`: Non-admin access returns 403 Forbidden
  - [ ] `TC-CMS-ADM-DASH-003`: Unauthenticated access returns 401 Unauthorized

- [ ] **GET /api/admin/settings**
  - [ ] `TC-CMS-ADM-SET-001`: Admin gets global system settings returns 200 OK
  - [ ] `TC-CMS-ADM-SET-002`: Non-admin access returns 403 Forbidden

- [ ] **PUT /api/admin/settings**
  - [ ] `TC-CMS-ADM-SET-003`: Admin updates global system settings returns 200 OK
  - [ ] `TC-CMS-ADM-SET-004`: Non-admin updates settings returns 403 Forbidden
  - [ ] `TC-CMS-ADM-SET-005`: Invalid settings payload returns 400 Bad Request

- [ ] **GET /api/partner/dashboard**
  - [ ] `TC-CMS-PART-DASH-001`: Partner gets airline-specific metrics returns 200 OK
  - [ ] `TC-CMS-PART-DASH-002`: Customer/Unauthenticated access returns 401/403

---

## Detailed Test Case Specifications

### TC-CMS-ADM-DASH-001: Admin dashboard statistics
- **Endpoint**: `GET /api/admin/dashboard`
- **Auth**: Admin JWT
- **Assertions**:
  - HTTP Status: `200 OK`
  - Response contains system statistics (total revenue, total bookings, new users, critical logs).

### TC-CMS-ADM-SET-003: Update admin settings
- **Endpoint**: `PUT /api/admin/settings`
- **Auth**: Admin JWT
- **Request Body**:
  ```json
  {
    "siteName": "AirlineTicket Portal",
    "maintenanceMode": false,
    "maxBookingPerUser": "10",
    "holdLimit": "15m",
    "commissionFee": "5%"
  }
  ```
- **Assertions**:
  - HTTP Status: `200 OK`
  - Response echoes updated configuration.
