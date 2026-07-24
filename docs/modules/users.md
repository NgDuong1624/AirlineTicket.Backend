# Users Module

## Overview
The **Users Module** manages authentication, authorization, and user administration. It handles user registration, login, profile management, role-based access control (RBAC), and Google OAuth integration.

---

## How It Works (For End Users & Clients)
1. **Registration & Login**: Users can register with an email and password. Upon login, the system validates credentials and returns an `accessToken` (JWT) and a `refreshToken`.
2. **Google OAuth**: Users can log in using their Google accounts. The system validates the Google ID token, creates a user account if it doesn't exist, and returns access/refresh tokens.
3. **Token Refresh**: When the access token expires, clients can request a new one using the refresh token.
4. **Authorization**: The system uses JWT claims to enforce role-based and permission-based access control. Permissions are embedded directly in the JWT token.
5. **User Management**: Administrators can manage users, update their roles, and toggle their active status. Airline partners can manage staff members scoped to their airline.

---

## Domain Entities & Data Model

### User
Represents a user account in the system.
- `Id` (Guid): Unique identifier.
- `Email` (string): Unique email address.
- `EmailConfirmed` (bool): Whether the email has been verified.
- `PasswordHash` (string): Hashed password (using BCrypt).
- `FullName` (string): User's full name.
- `Phone` (string?): Phone number.
- `Role` (UserRole): User's role (`0` = Admin, `1` = Partner, `2` = Staff, `3` = Customer).
- `AirlineId` (Guid?): FK to the airline, if the user is a partner or staff member.
- `AvatarUrl` (string?): URL to the user's avatar.
- `LanguagePreference` (string): Preferred language (default: `vi`).
- `GoogleId` (string?): Google account ID for OAuth.
- `AuthProvider` (string?): Authentication provider (e.g., `Local`, `Google`).
- `LastLoginAt` (DateTime?): UTC timestamp of the last login.
- `IsActive` (bool): Whether the account is active.
- `IsDeleted` (bool): Soft delete flag.
- `RefreshToken` (string?): Current refresh token.
- `RefreshTokenExpiryTime` (DateTime?): Expiration time of the refresh token.
- `CreatedAt` (DateTime): UTC timestamp of creation.
- `UpdatedAt` (DateTime): UTC timestamp of last update.

### Role
Represents a user role.
- `Id` (int): Unique identifier.
- `Name` (string): Role name (e.g., `role.system_admin.name`).
- `Description` (string?): Role description.

### Permission
Represents a specific permission in the system.
- `Id` (int): Unique identifier.
- `Code` (string): Unique permission code (e.g., `CREATE_FLIGHT`, `SELL_TICKET`).
- `Name` (string): Permission name.
- `Description` (string?): Permission description.

### RolePermission
Intermediate table mapping roles to permissions.
- `RoleId` (int): FK to `Role`.
- `PermissionId` (int): FK to `Permission`.

---

## API Reference

### Authentication Roles
- **AdminOnly**: Requires authentication as a System Admin.
- **PartnerOnly**: Requires authentication as an Airline Admin.
- **PartnerOrStaff**: Requires authentication as an Airline Admin, Airline Staff, or System Admin.

### Endpoints

#### Authentication Endpoints (`/api/auth`)
| Method | Path | Auth | Description |
|--------|------|------|-------------|
| **POST** | `/login` | None | Authenticate with email and password. Returns access and refresh tokens. |
| **POST** | `/google` | None | Authenticate with Google ID token. Returns access and refresh tokens. |
| **POST** | `/register` | None | Register a new customer account. |
| **GET** | `/me` | Authenticated | Get the profile of the currently logged-in user. |
| **POST** | `/refresh` | None | Refresh an expired access token using a valid refresh token. |
| **POST** | `/logout` | Authenticated | Revoke the current session and invalidate the refresh token. |

#### Admin User Management (`/api/admin/users`)
| Method | Path | Auth | Description |
|--------|------|------|-------------|
| **GET** | `/` | AdminOnly | Get a paginated list of all users. |
| **POST** | `/` | AdminOnly | Create a new user account (any role). |
| **PUT** | `/{id}` | AdminOnly | Update user details. |
| **GET** | `/{id}` | AdminOnly | Get details of a specific user. |
| **DELETE** | `/{id}` | AdminOnly | Soft-delete a user account. |
| **PATCH** | `/{id}/status` | AdminOnly | Toggle user active status (`IsActive`). |
| **GET** | `/permissions` | AdminOnly | Get a paginated list of all permissions. |

#### Partner Staff Management (`/api/partner/staff`)
| Method | Path | Auth | Description |
|--------|------|------|-------------|
| **GET** | `/` | PartnerOnly | List staff members for the partner's airline. |
| **POST** | `/` | PartnerOnly | Create a new staff member for the partner's airline. |
| **PUT** | `/{id}` | PartnerOnly | Update staff member details. |
| **DELETE** | `/{id}` | PartnerOnly | Soft-delete a staff member. |
| **PATCH** | `/{id}/status` | PartnerOnly | Toggle staff member active status. |
| **GET** | `/my-airline` | PartnerOrStaff | List all staff members belonging to the authenticated user's airline. |
