-- =====================================================
-- DEV: Sample Passengers
-- Schema: bookings
-- Depends on: 01_bookings.sql
-- =====================================================

INSERT INTO bookings.passengers (id, booking_id, first_name, last_name, gender, date_of_birth, nationality, passport_number, passport_expiry_date) VALUES
('FA10F2A8-9B2F-4A9B-89E3-4E80D77BC901', 'B1B0F2A8-9B2F-4A9B-89E3-4E80D77BC901', 'Văn A', 'Nguyễn', 0, '1990-05-15', 'Vietnam', 'B1234567', '2030-05-15')
ON CONFLICT (id) DO NOTHING;
