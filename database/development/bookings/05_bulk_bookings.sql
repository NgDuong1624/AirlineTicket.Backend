-- =====================================================
-- DEV: Bulk Generated Bookings (50 bookings)
-- Schema: bookings
-- Depends on: development/users, development/flights
-- Purpose: Generate large volume of bookings for development/demo
-- =====================================================

DO $$
DECLARE
    _bookingCount INTEGER := 0;
    _userId UUID;
    _flightId UUID;
    _seatId UUID;
    _bookingId UUID;
    _passengerId UUID;
BEGIN
    SELECT id INTO _userId FROM users.users WHERE email = 'client@gmail.com' LIMIT 1;

    WHILE _bookingCount < 50 LOOP
        _bookingCount := _bookingCount + 1;

        SELECT id INTO _flightId FROM flights.flights ORDER BY gen_random_uuid() LIMIT 1;
        SELECT id INTO _seatId FROM flights.flight_seats WHERE flight_id = _flightId AND is_available = TRUE ORDER BY gen_random_uuid() LIMIT 1;

        IF _seatId IS NOT NULL THEN
            _bookingId := gen_random_uuid();
            _passengerId := gen_random_uuid();

            INSERT INTO bookings.bookings (id, user_id, pnr_code, total_price, currency, status, contact_email, contact_phone, special_requests, is_deleted, created_at, updated_at)
            VALUES (_bookingId, _userId, 'PNR' || (1000 + _bookingCount)::TEXT, 100.00, 'USD', 1, 'client@gmail.com', '0987654321', NULL, FALSE, NOW(), NOW());

            INSERT INTO bookings.passengers (id, booking_id, first_name, last_name, gender, date_of_birth, nationality, passport_number, passport_expiry_date)
            VALUES (_passengerId, _bookingId, 'Passenger', (_bookingCount)::TEXT, 0, '1990-01-01', 'VN', 'PS' || (_bookingCount)::TEXT, '2030-01-01');

            INSERT INTO bookings.tickets (id, booking_id, passenger_id, flight_id, seat_id, ticket_number, gate, boarding_time, status)
            VALUES (gen_random_uuid(), _bookingId, _passengerId, _flightId, _seatId, 'TK' || (1000 + _bookingCount)::TEXT, 'Gate A', DATE_TRUNC('hour', NOW()) + INTERVAL '-1 hour', 0);

            INSERT INTO bookings.payments (id, booking_id, transaction_id, amount, payment_method, provider_status, is_successful, raw_response, created_at)
            VALUES (gen_random_uuid(), _bookingId, 'TXN' || (1000 + _bookingCount)::TEXT, 100.00, 'CreditCard', 'Success', TRUE, NULL, NOW());

            UPDATE flights.flight_seats SET is_available = FALSE WHERE id = _seatId;
        END IF;
    END LOOP;
END $$;
