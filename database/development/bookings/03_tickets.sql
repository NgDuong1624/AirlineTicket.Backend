-- =====================================================
-- DEV: Sample Tickets
-- Schema: bookings
-- Depends on: 01_bookings.sql, 02_passengers.sql, development/flights/03_flights.sql
-- =====================================================

INSERT INTO bookings.tickets (id, booking_id, passenger_id, flight_id, seat_id, ticket_number, gate, boarding_time, status) 
SELECT 
    'B110F2A8-9B2F-4A9B-89E3-4E80D77BC901', 
    'B1B0F2A8-9B2F-4A9B-89E3-4E80D77BC901', 
    'FA10F2A8-9B2F-4A9B-89E3-4E80D77BC901', 
    'F1F3E2D4-BCDE-4F01-2345-6789ABCDEF01', 
    fs.id, 
    'TK-VN-20250615-001', 
    'Gate 5', 
    NOW() + INTERVAL '1 day' - INTERVAL '40 minutes', 
    0
FROM flights.flight_seats fs 
WHERE fs.flight_id = 'F1F3E2D4-BCDE-4F01-2345-6789ABCDEF01' AND fs.seat_number = '10A'
ON CONFLICT (id) DO NOTHING;
