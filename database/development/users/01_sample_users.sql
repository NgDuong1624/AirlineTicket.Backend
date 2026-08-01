-- =====================================================
-- DEV: Sample Users (Client)
-- Schema: users
-- Depends on: core/users/01_roles.sql
-- =====================================================
-- Default password for all: use bcrypt hash below

INSERT INTO users.users (id, email, email_confirmed, password_hash, full_name, phone, is_active, is_deleted, created_at, updated_at, role) VALUES
('E2E3F4C5-1A2B-3C4D-5E6F-7A8B9C0D1E2F', 'client@gmail.com', TRUE, '$2b$12$d7mwUGX/x5jcRrxdNAFqJekces0IQ2f9F4mC24iVGZUpwM2pF1Qly', 'Nguyễn Văn A', '0987654321', TRUE, FALSE, NOW(), NOW(), 3)
ON CONFLICT (id) DO NOTHING;
