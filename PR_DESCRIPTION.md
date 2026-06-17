# PR Title: feat(users): implement RBAC system

## Description
This pull request introduces a Role-Based Access Control (RBAC) system for the user module, enabling fine-grained permission management for airline ticket operations.

## Changes Made
- Implemented RBAC entities: `Permission`, `Role`, `UserRole`, `RolePermission`, `UserPermissionScope`.
- Added EF Core configurations for all RBAC entities.
- Created migrations for RBAC tables and soft-delete triggers.
- Configured `UserDbContext` to support the new RBAC structure.
- Added `UserDbContextFactory` for design-time migration support.

## Type of Change
- [ ] Bug fix
- [x] New feature
- [ ] Documentation update
- [ ] Refactoring
- [ ] CI/CD / DevOps config
