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
| Method | Path | Auth | Description | Input Type | Output Type |
|--------|------|------|-------------|------------|-------------|
| **POST** | `/login` | None | Authenticate with email and password. Returns access and refresh tokens. | `{ email: string, password?: string }` | `{ user: UserResponse { id: string, email: string, fullName: string, roleId: number, phone?: string, avatarUrl?: string, languagePreference?: string, lastLoginAt?: string, role?: string, status?: string, isActive?: boolean, airlineId?: string, airlineName?: string, createdAt: string, updatedAt: string }, accessToken: string }` |
| **POST** | `/google` | None | Authenticate with Google ID token. Returns access and refresh tokens. | `{ idToken: string }` | `{ user: UserResponse, accessToken: string }` |
| **POST** | `/register` | None | Register a new customer account. | `Record<string, unknown>` | `{ user: UserResponse, accessToken: string }` |
| **GET** | `/me` | Authenticated | Get the profile of the currently logged-in user. | None | `User { id: string, email: string, emailConfirmed: boolean, fullName: string, phone?: string, avatarUrl?: string, languagePreference?: string, lastLoginAt?: string, role?: string, roleId: number, status?: string, isActive?: boolean, airlineId?: string, airlineName?: string, createdAt: string, updatedAt: string }` |
| **POST** | `/refresh` | None | Refresh an expired access token using a valid refresh token. | `{ refreshToken: string }` | `{ accessToken: string }` |
| **POST** | `/logout` | Authenticated | Revoke the current session and invalidate the refresh token. | None | `void` |

#### Admin User Management (`/api/admin/users`)
| Method | Path | Auth | Description | Input Type | Output Type |
|--------|------|-------------|------------|-------------|
| **GET** | `/` | AdminOnly | Get a paginated list of all users. | None (Query params `search?: string, airlineId?: string, roleId?: number, pageIndex: number, pageSize: number`) | `PagedResult<User> { items: User[], totalCount: number, pageNumber: number, pageSize: number, totalPages: number, hasNextPage: boolean, hasPreviousPage: boolean }` |
| **POST** | `/` | AdminOnly | Create a new user account (any role). | `Partial<User>` | `User` |
| **PUT** | `/{id}` | AdminOnly | Update user details. | `Partial<User>` | `User` |
| **GET** | `/{id}` | AdminOnly | Get details of a specific user. | None | `User` |
| **DELETE** | `/{id}` | AdminOnly | Soft-delete a user account. | None | `void` |
| **PATCH** | `/{id}/status` | AdminOnly | Toggle user active status (`IsActive`). | `{ isActive: number }` | `void` |
| **GET** | `/permissions` | AdminOnly | Get a paginated list of all permissions. | None (Query params `pageIndex: number, pageSize: number`) | `PagedResult<Permission> { items: Permission[] { id: string, code: string, name: string, description?: string, isActive?: boolean }, totalCount: number, pageNumber: number, pageSize: number, totalPages: number, hasNextPage: boolean, hasPreviousPage: boolean }` |

#### Partner Staff Management (`/api/partner/staff`)
| Method | Path | Auth | Description | Input Type | Output Type |
|--------|------|-------------|------------|-------------|
| **GET** | `/` | PartnerOnly | List staff members for the partner's airline. | None (Query params `pageIndex: number, pageSize: number`) | `PagedResult<Staff> { items: Staff[] { id: string, fullName: string, role?: string, roleId?: number, email: string, phone: string, isActive?: boolean, airlineId?: string, createdAt?: string }, totalCount: number, pageNumber: number, pageSize: number, totalPages: number, hasNextPage: boolean, hasPreviousPage: boolean }` |
| **POST** | `/` | PartnerOnly | Create a new staff member for the partner's airline. | `Partial<Staff>` | `Staff` |
| **PUT** | `/{id}` | PartnerOnly | Update staff member details. | `Partial<Staff>` | `Staff` |
| **DELETE** | `/{id}` | PartnerOnly | Soft-delete a staff member. | None | `void` |
| **PATCH** | `/{id}/status` | PartnerOnly | Toggle staff member active status. | `{ isActive: number }` | `void` |
| **GET** | `/my-airline` | PartnerOrStaff | List all staff members belonging to the authenticated user's airline. | None | `{ items: Staff[] }` |
