# Users & Authentication Module API Test Cases

## Module Overview
- **Base Paths**: `/api/auth`, `/api/admin/users`, `/api/partner/staff`
- **Authentication**: JWT Bearer (`accessToken`), Refresh Token rotation
- **Roles**: SystemAdmin (`0`), Partner (`1`), Staff (`2`), Customer (`3`)

---

## Checklist Summary

- [ ] **POST /api/auth/login**
  - [ ] `TC-AUTH-LOGIN-001`: Valid credentials returns 200 OK with UserResponse and tokens
  - [ ] `TC-AUTH-LOGIN-002`: Invalid password returns 400/401 Unauthorized
  - [ ] `TC-AUTH-LOGIN-003`: Non-existent email returns 400/401 Unauthorized
  - [ ] `TC-AUTH-LOGIN-004`: Inactive user account login returns 403 Forbidden
  - [ ] `TC-AUTH-LOGIN-005`: Missing mandatory email/password returns 400 Bad Request

- [ ] **POST /api/auth/google**
  - [ ] `TC-AUTH-GOOGLE-001`: Valid Google ID token returns 200 OK with session tokens
  - [ ] `TC-AUTH-GOOGLE-002`: New Google account automatically registers customer user
  - [ ] `TC-AUTH-GOOGLE-003`: Invalid/expired Google ID token returns 401 Unauthorized
  - [ ] `TC-AUTH-GOOGLE-004`: Empty payload returns 400 Bad Request

- [ ] **POST /api/auth/register**
  - [ ] `TC-AUTH-REG-001`: Valid registration payload returns 200/201 with Customer profile and tokens
  - [ ] `TC-AUTH-REG-002`: Duplicate email returns 409 Conflict
  - [ ] `TC-AUTH-REG-003`: Weak password / invalid email format returns 400 Bad Request

- [ ] **GET /api/auth/me**
  - [ ] `TC-AUTH-ME-001`: Authenticated user returns 200 OK with current profile details
  - [ ] `TC-AUTH-ME-002`: Unauthenticated request returns 401 Unauthorized
  - [ ] `TC-AUTH-ME-003`: Expired token returns 401 Unauthorized

- [ ] **PUT /api/auth/language**
  - [ ] `TC-AUTH-LANG-001`: Authenticated user updates language preference (`vi`/`en`) returns 200/204
  - [ ] `TC-AUTH-LANG-002`: Invalid language code returns 400 Bad Request
  - [ ] `TC-AUTH-LANG-003`: Unauthenticated request returns 401 Unauthorized

- [ ] **POST /api/auth/refresh**
  - [ ] `TC-AUTH-REFRESH-001`: Valid refresh token returns 200 OK with new access token
  - [ ] `TC-AUTH-REFRESH-002`: Expired or revoked refresh token returns 401 Unauthorized
  - [ ] `TC-AUTH-REFRESH-003`: Malformed refresh token payload returns 400 Bad Request

- [ ] **POST /api/auth/logout**
  - [ ] `TC-AUTH-LOGOUT-001`: Authenticated user revokes refresh token returns 200/204
  - [ ] `TC-AUTH-LOGOUT-002`: Unauthenticated request returns 401 Unauthorized

- [ ] **GET /api/admin/users**
  - [ ] `TC-ADM-USERS-001`: Admin gets paginated users list with 200 OK
  - [ ] `TC-ADM-USERS-002`: Filter by roleId, airlineId, search keyword returns filtered records
  - [ ] `TC-ADM-USERS-003`: Non-admin role (Customer/Staff) returns 403 Forbidden
  - [ ] `TC-ADM-USERS-004`: Unauthenticated request returns 401 Unauthorized

- [ ] **POST /api/admin/users**
  - [ ] `TC-ADM-USERS-005`: Admin creates user with specified role returns 201 Created
  - [ ] `TC-ADM-USERS-006`: Partner/Staff creation without AirlineId returns 400 Bad Request
  - [ ] `TC-ADM-USERS-007`: Duplicate email returns 409 Conflict

- [ ] **GET /api/admin/users/{id}**
  - [ ] `TC-ADM-USERS-008`: Admin gets user by ID returns 200 OK
  - [ ] `TC-ADM-USERS-009`: Non-existent user ID returns 404 Not Found

- [ ] **PUT /api/admin/users/{id}**
  - [ ] `TC-ADM-USERS-010`: Admin updates user info returns 200 OK
  - [ ] `TC-ADM-USERS-011`: Admin updates non-existent user returns 404 Not Found

- [ ] **DELETE /api/admin/users/{id}**
  - [ ] `TC-ADM-USERS-012`: Admin soft deletes user returns 200/204
  - [ ] `TC-ADM-USERS-013`: Admin deletes non-existent user returns 404 Not Found

- [ ] **PATCH /api/admin/users/{id}/status**
  - [ ] `TC-ADM-USERS-014`: Admin toggles isActive flag returns 200/204
  - [ ] `TC-ADM-USERS-015`: Invalid isActive value returns 400 Bad Request

- [ ] **GET /api/admin/users/permissions**
  - [ ] `TC-ADM-PERM-001`: Admin lists paginated permissions returns 200 OK
  - [ ] `TC-ADM-PERM-002`: Non-admin access returns 403 Forbidden

- [ ] **GET /api/partner/staff**
  - [ ] `TC-PART-STAFF-001`: Partner gets paginated staff for own airline returns 200 OK
  - [ ] `TC-PART-STAFF-002`: Non-partner access returns 403 Forbidden

- [ ] **POST /api/partner/staff**
  - [ ] `TC-PART-STAFF-003`: Partner creates staff member auto-linked to partner's airline returns 201 Created
  - [ ] `TC-PART-STAFF-004`: Duplicate staff email returns 409 Conflict

- [ ] **PUT /api/partner/staff/{id}**
  - [ ] `TC-PART-STAFF-005`: Partner updates staff details returns 200 OK
  - [ ] `TC-PART-STAFF-006`: Partner updates staff belonging to different airline returns 403/404

- [ ] **DELETE /api/partner/staff/{id}**
  - [ ] `TC-PART-STAFF-007`: Partner soft deletes staff for own airline returns 200/204
  - [ ] `TC-PART-STAFF-008`: Partner deletes staff from another airline returns 403/404

- [ ] **PATCH /api/partner/staff/{id}/status**
  - [ ] `TC-PART-STAFF-009`: Partner toggles staff active status returns 200/204

- [ ] **GET /api/partner/staff/my-airline**
  - [ ] `TC-PART-STAFF-010`: Partner/Staff retrieves all airline staff list returns 200 OK
  - [ ] `TC-PART-STAFF-011`: Customer role returns 403 Forbidden

---

## Detailed Test Case Specifications

### TC-AUTH-LOGIN-001: Valid credentials returns 200 OK
- **Endpoint**: `POST /api/auth/login`
- **Auth**: None
- **Preconditions**: Active user exists with email `customer@example.com` and password `Password@123`.
- **Request Body**:
  ```json
  {
    "email": "customer@example.com",
    "password": "Password@123"
  }
  ```
- **Assertions**:
  - HTTP Status: `200 OK`
  - Response contains `accessToken`, `refreshToken`, and `user` object.
  - `user.email` equals `customer@example.com`.
  - `user.isActive` is `true`.

### TC-AUTH-LOGIN-002: Invalid password
- **Endpoint**: `POST /api/auth/login`
- **Auth**: None
- **Request Body**:
  ```json
  {
    "email": "customer@example.com",
    "password": "WrongPassword!"
  }
  ```
- **Assertions**:
  - HTTP Status: `400 Bad Request` or `401 Unauthorized`
  - Error message indicates invalid credentials without leaking specific field.

### TC-AUTH-REG-002: Duplicate email conflict
- **Endpoint**: `POST /api/auth/register`
- **Auth**: None
- **Preconditions**: Email `existing@example.com` is already registered.
- **Request Body**:
  ```json
  {
    "email": "existing@example.com",
    "password": "Password@123",
    "fullName": "Jane Doe"
  }
  ```
- **Assertions**:
  - HTTP Status: `409 Conflict` (or `400 Bad Request` with problem details duplicate email).

### TC-AUTH-REFRESH-001: Valid token refresh
- **Endpoint**: `POST /api/auth/refresh`
- **Auth**: None
- **Preconditions**: User has valid unexpired `refreshToken`.
- **Request Body**:
  ```json
  {
    "refreshToken": "valid-sample-refresh-token"
  }
  ```
- **Assertions**:
  - HTTP Status: `200 OK`
  - Body contains new non-empty `accessToken`.

### TC-ADM-USERS-001: Admin paginated user list
- **Endpoint**: `GET /api/admin/users?pageIndex=1&pageSize=10`
- **Auth**: Admin JWT
- **Assertions**:
  - HTTP Status: `200 OK`
  - Body contains `items`, `totalCount`, `pageNumber: 1`, `pageSize: 10`.

### TC-ADM-USERS-003: Non-admin gets forbidden
- **Endpoint**: `GET /api/admin/users`
- **Auth**: Customer JWT
- **Assertions**:
  - HTTP Status: `403 Forbidden`

### TC-PART-STAFF-003: Partner creates staff
- **Endpoint**: `POST /api/partner/staff`
- **Auth**: Partner JWT (AirlineId `A1`)
- **Request Body**:
  ```json
  {
    "email": "staff.vn@airline.com",
    "fullName": "Staff Member",
    "phone": "+84901234567"
  }
  ```
- **Assertions**:
  - HTTP Status: `201 Created`
  - Response `airlineId` equals `A1` and `roleId` corresponds to Staff (`2`).
