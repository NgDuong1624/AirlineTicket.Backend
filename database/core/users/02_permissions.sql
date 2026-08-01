-- =====================================================
-- CORE: Permissions (Required for system operation)
-- Schema: users
-- =====================================================

INSERT INTO users.permissions (code, name, description) VALUES
('CREATE_FLIGHT', 'permission.create_flight.name', 'permission.create_flight.desc'),
('SELL_TICKET', 'permission.sell_ticket.name', 'permission.sell_ticket.desc'),
('MANAGE_AIRLINE_STAFF', 'permission.manage_airline_staff.name', 'permission.manage_airline_staff.desc'),
('MANAGE_USERS', 'permission.manage_users.name', 'permission.manage_users.desc'),
('MANAGE_FLIGHTS', 'permission.manage_flights.name', 'permission.manage_flights.desc'),
('MANAGE_AIRLINES', 'permission.manage_airlines.name', 'permission.manage_airlines.desc'),
('MANAGE_AIRPORTS', 'permission.manage_airports.name', 'permission.manage_airports.desc'),
('MANAGE_BOOKINGS', 'permission.manage_bookings.name', 'permission.manage_bookings.desc'),
('MANAGE_ARTICLES', 'permission.manage_articles.name', 'permission.manage_articles.desc'),
('MANAGE_PERMISSIONS', 'permission.manage_permissions.name', 'permission.manage_permissions.desc'),
('MANAGE_ROLES', 'permission.manage_roles.name', 'permission.manage_roles.desc')
ON CONFLICT (code) DO NOTHING;
