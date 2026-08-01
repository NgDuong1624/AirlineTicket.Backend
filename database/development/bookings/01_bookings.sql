-- =====================================================
-- DEV: Sample Bookings
-- Schema: bookings
-- Depends on: development/users/01_sample_users.sql
-- =====================================================

INSERT INTO bookings.bookings (id, user_id, pnr_code, total_price, currency, status, contact_email, contact_phone, special_requests, is_deleted, created_at, updated_at) VALUES
('B1B0F2A8-9B2F-4A9B-89E3-4E80D77BC901', 'E2E3F4C5-1A2B-3C4D-5E6F-7A8B9C0D1E2F', 'PNR123', 160.00, 'USD', 2, 'client@gmail.com', '0987654321', 'No Special Requests', FALSE, NOW(), NOW())
ON CONFLICT (id) DO NOTHING;
