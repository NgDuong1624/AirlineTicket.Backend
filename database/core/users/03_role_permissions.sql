-- =====================================================
-- CORE: Role-Permission Mappings
-- Schema: users
-- Depends on: 01_roles.sql, 02_permissions.sql
-- =====================================================

-- System Admin (RoleId=0): All permissions
INSERT INTO users.role_permissions (role_id, permission_id)
SELECT 0, id FROM users.permissions
ON CONFLICT (role_id, permission_id) DO NOTHING;

-- Airline Admin (RoleId=1): Manage flights, airlines, bookings, staff
INSERT INTO users.role_permissions (role_id, permission_id)
SELECT 1, id FROM users.permissions
WHERE code IN ('CREATE_FLIGHT', 'SELL_TICKET', 'MANAGE_AIRLINE_STAFF', 'MANAGE_FLIGHTS', 'MANAGE_AIRLINES', 'MANAGE_BOOKINGS')
ON CONFLICT (role_id, permission_id) DO NOTHING;

-- Airline Staff (RoleId=2): Sell tickets, view bookings
INSERT INTO users.role_permissions (role_id, permission_id)
SELECT 2, id FROM users.permissions
WHERE code IN ('SELL_TICKET', 'MANAGE_FLIGHTS', 'MANAGE_BOOKINGS')
ON CONFLICT (role_id, permission_id) DO NOTHING;

-- User Client (RoleId=3): Basic permissions
INSERT INTO users.role_permissions (role_id, permission_id)
SELECT 3, id FROM users.permissions
WHERE code IN ('SELL_TICKET')
ON CONFLICT (role_id, permission_id) DO NOTHING;
