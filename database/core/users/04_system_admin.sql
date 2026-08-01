-- =====================================================
-- CORE: System Admin User (Required for system administration)
-- Schema: users
-- Depends on: core/users/01_roles.sql
-- =====================================================
-- Default password: use bcrypt hash below (password: 123456)

INSERT INTO users.users (id, email, email_confirmed, password_hash, full_name, phone, is_active, is_deleted, created_at, updated_at, role) VALUES
('D4B0F2A8-9B2F-4A9B-89E3-4E80D77BC901', 'admin@airlineticket.com', TRUE, '$2b$12$d7mwUGX/x5jcRrxdNAFqJekces0IQ2f9F4mC24iVGZUpwM2pF1Qly', 'Hệ Thống Admin', '0123456789', TRUE, FALSE, NOW(), NOW(), 0)
ON CONFLICT (id) DO NOTHING;
