-- =====================================================
-- TEST: Minimal Users for integration tests
-- Schema: users
-- Depends on: core/users/01_roles.sql
-- =====================================================
INSERT INTO users.users (id, email, email_confirmed, password_hash, full_name, phone, is_active, is_deleted, created_at, updated_at, role) VALUES
('00000000-0000-0000-0000-000000000001', 'test1@example.com', TRUE, '$2b$12$d7mwUGX/x5jcRrxdNAFqJekces0IQ2f9F4mC24iVGZUpwM2pF1Qly', 'Test User 1', '0000000001', TRUE, FALSE, NOW(), NOW(), 3),
('00000000-0000-0000-0000-000000000002', 'test2@example.com', TRUE, '$2b$12$d7mwUGX/x5jcRrxdNAFqJekces0IQ2f9F4mC24iVGZUpwM2pF1Qly', 'Test User 2', '0000000002', TRUE, FALSE, NOW(), NOW(), 3)
ON CONFLICT (id) DO NOTHING;
