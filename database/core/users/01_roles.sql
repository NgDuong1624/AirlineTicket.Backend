-- =====================================================
-- CORE: Roles (Required for system operation)
-- Schema: users
-- =====================================================

INSERT INTO users.roles (id, name, description) VALUES
(0, 'role.system_admin.name', 'role.system_admin.desc'),
(1, 'role.airline_admin.name', 'role.airline_admin.desc'),
(2, 'role.airline_staff.name', 'role.airline_staff.desc'),
(3, 'role.user_client.name', 'role.user_client.desc')
ON CONFLICT (id) DO NOTHING;
