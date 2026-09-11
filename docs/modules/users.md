# Users Module

## Overview
The **Users Module** manages authentication, authorization, user profiles, and administrative staff management. It handles user registration, local credential login, Google OAuth integration, token refreshing, role-based access control (RBAC), and airline staff scoping.

---

## How It Works (For End Users & Clients)
1. **Registration & Login**: Users register with an email and password. Upon login (`POST /api/auth/login`), the system validates credentials using BCrypt password hashing and returns an `accessToken` (JWT) and a `refreshToken`.
2. **Google OAuth**: Users can log in using their Google account (`POST /api/auth/google`). The system validates the Google ID token, provisions an account if not already existing, and returns JWT tokens.
3. **Token Refresh & Revocation**: When the access token expires, clients call `POST /api/auth/refresh` with the refresh token. Calling `POST /api/auth/logout` invalidates the active refresh token session.
4. **Authorization & RBAC**: The system uses JWT claims (`role`, `AirlineId`, `sub`) to enforce role-based access control policies (`AdminOnly`, `PartnerOnly`, `PartnerOrStaff`, `StaffOnly`).
5. **Staff Management**: Airline partners manage staff members scoped strictly to their `AirlineId`. System administrators have global privileges to manage all users and query system permissions.

---

## Domain Entities & Data Model

### User
Represents a user account in the system.
- `Id` (Guid): Unique identifier.
- `Email` (string): Unique email address.
- `EmailConfirmed` (bool): Whether email has been verified.
- `PasswordHash` (string): BCrypt hashed password.
- `FullName` (string): User's full name.
- `Phone` (string?): Phone number.
- `Role` (UserRole): User's role (`0` = Admin, `1` = Partner, `2` = Staff, `3` = Customer).
- `AirlineId` (Guid?): FK to the airline if the user is an airline partner or staff member.
- `AvatarUrl` (string?): URL to the user's avatar image.
- `LanguagePreference` (string): Preferred language code (default: `vi`).
- `GoogleId` (string?): Google account ID for OAuth logins.
- `AuthProvider` (string?): Authentication provider (e.g., `Local`, `Google`).
- `LastLoginAt` (DateTime?): UTC timestamp of last login.
- `IsActive` (bool): Whether the account is active.
- `IsDeleted` (bool): Soft delete flag.
- `RefreshToken` (string?): Active refresh token.
- `RefreshTokenExpiryTime` (DateTime?): Expiration time of the refresh token.
- `CreatedAt` (DateTime): UTC timestamp of creation.
- `UpdatedAt` (DateTime): UTC timestamp of last update.

### Role
Represents a system role.
- `Id` (int): Unique identifier.
- `Name` (string): Role name (e.g., `Admin`, `Partner`, `Staff`, `Customer`).
- `Description` (string?): Role description.

### Permission
Represents an action permission in the system.
- `Id` (int): Unique identifier.
- `Code` (string): Unique permission code (e.g., `CREATE_FLIGHT`, `SELL_TICKET`).
- `Name` (string): Permission display name.
- `Description` (string?): Permission description.

---

## API Reference

### Authentication Roles & Policies
- **AdminOnly**: System Administrator role (`Role = 0`).
- **PartnerOnly**: Airline Partner role (`Role = 1`).
- **PartnerOrStaff**: Airline Partner (`Role = 1`) or Airline Staff (`Role = 2`) or System Admin (`Role = 0`).
- **StaffOnly**: Airline Staff role (`Role = 2`).
- **Authenticated**: Any valid JWT token.

### Endpoints

#### Authentication & Profile (`/api/auth`)
| Method | Path | Auth | Description | Input Type | Output Type |
|--------|------|------|-------------|------------|-------------|
| **POST** | `/api/auth/login` | None | Authenticate with email and password. | `LoginUserRequest { email: string, password: string }` | `{ accessToken: string, refreshToken: string, user: UserProfileDto }` |
| **POST** | `/api/auth/google` | None | Authenticate with Google ID token. | `GoogleLoginRequest { idToken: string }` | `{ accessToken: string, refreshToken: string, user: UserProfileDto }` |
| **POST** | `/api/auth/register` | None | Register a new customer account. | `RegisterUserRequest { email: string, password: string, fullName: string, phone: string, languagePreference?: string }` | `{ userId: Guid }` |
| **GET** | `/api/auth/me` | Authenticated | Get current authenticated user's profile. | None | `UserProfileDto` |
| **PUT** | `/api/auth/language` | Authenticated | Update user's preferred language code. | `UpdateLanguageRequest { language: string }` | `200 OK` |
| **POST** | `/api/auth/refresh` | None | Refresh an expired access token using a refresh token. | `RefreshTokenRequest { refreshToken: string }` | `{ accessToken: string, refreshToken: string, user: UserProfileDto }` |
| **POST** | `/api/auth/logout` | None | Revoke the active refresh token session. | `LogoutRequest { refreshToken: string }` | `200 OK` |

#### Admin User Management (`/api/admin/users`)
| Method | Path | Auth | Description | Input Type | Output Type |
|--------|------|------|-------------|------------|-------------|
| **GET** | `/api/admin/users` | AdminOnly | Get paginated list of all users. | Query params `search?: string, airlineId?: Guid, roleId?: int, pageNumber?: int, pageIndex?: int, pageSize?: int` | `PagedResult<UserDto>` |
| **POST** | `/api/admin/users` | AdminOnly | Create a new user account. | `AdminUserRequest { email: string, fullName: string, phone?: string, roleId?: int, isActive?: bool, password?: string, airlineId?: Guid, languagePreference?: string }` | `{ id: Guid }` |
| **GET** | `/api/admin/users/{id}` | AdminOnly | Get user details by ID. | Route param `id: Guid` | `UserDto` |
| **PUT** | `/api/admin/users/{id}` | AdminOnly | Update user profile and role. | Route param `id: Guid`, `AdminUserRequest` | `200 OK` |
| **DELETE** | `/api/admin/users/{id}` | AdminOnly | Soft delete a user account. | Route param `id: Guid` | `204 NoContent` |
| **PATCH** | `/api/admin/users/{id}/status` | AdminOnly | Toggle user active status. | Route param `id: Guid`, `UpdateUserStatusRequest { isActive: int }` | `200 OK` |
| **GET** | `/api/admin/users/permissions` | AdminOnly | Get paginated list of all permissions. | Query params `pageNumber?: int, pageIndex?: int, pageSize?: int` | `PagedResult<PermissionDto>` |

#### Partner Staff Management (`/api/partner/staff`)
| Method | Path | Auth | Description | Input Type | Output Type |
|--------|------|------|-------------|------------|-------------|
| **GET** | `/api/partner/staff` | PartnerOnly | List staff members belonging to partner's airline. | Query params `pageNumber?: int, pageIndex?: int, pageSize?: int` | `{ items: StaffDto[], totalCount: number }` |
| **POST** | `/api/partner/staff` | PartnerOnly | Create a new staff member for partner's airline. | `PartnerStaffRequest { email: string, fullName: string, phone?: string, isActive?: bool, password?: string }` | `{ id: Guid }` |
| **PUT** | `/api/partner/staff/{id}` | PartnerOnly | Update staff member details. | Route param `id: Guid`, `PartnerStaffRequest` | `200 OK` |
| **DELETE** | `/api/partner/staff/{id}` | PartnerOnly | Delete a staff member from partner's airline. | Route param `id: Guid` | `204 NoContent` |
| **PATCH** | `/api/partner/staff/{id}/status` | PartnerOnly | Toggle staff member active status. | Route param `id: Guid`, `UpdateUserStatusRequest { isActive: int }` | `200 OK` |
| **GET** | `/api/partner/staff/my-airline` | PartnerOrStaff | List all members belonging to the current user's airline. | Query params `pageNumber?: int, pageIndex?: int, pageSize?: int` | `{ airlineId: Guid, items: StaffDto[], totalCount: number }` |
