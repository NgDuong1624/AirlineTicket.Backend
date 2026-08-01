-- =====================================================
-- DEV: Sample Payments
-- Schema: bookings
-- Depends on: 01_bookings.sql
-- =====================================================

INSERT INTO bookings.payments (id, booking_id, transaction_id, amount, payment_method, provider_status, is_successful, raw_response, created_at) VALUES
('FF10F2A8-9B2F-4A9B-89E3-4E80D77BC901', 'B1B0F2A8-9B2F-4A9B-89E3-4E80D77BC901', 'TXN-987654321', 160.00, 'Stripe', 'succeeded', TRUE, '{status: succeeded, charge_id: ch_123}', NOW())
ON CONFLICT (id) DO NOTHING;
