-- =====================================================
-- DEV: Sample Payments
-- Schema: bookings
-- Depends on: 01_bookings.sql
-- =====================================================

INSERT INTO bookings.payments (id, booking_id, provider, provider_transaction_id, amount, currency, status, concurrency_version, raw_response, created_at) VALUES
('FF10F2A8-9B2F-4A9B-89E3-4E80D77BC901', 'B1B0F2A8-9B2F-4A9B-89E3-4E80D77BC901', 1, 'TXN-987654321', 160.00, 1, 3, 0, '{"status": "succeeded", "charge_id": "ch_123"}', NOW())
ON CONFLICT (id) DO NOTHING;
