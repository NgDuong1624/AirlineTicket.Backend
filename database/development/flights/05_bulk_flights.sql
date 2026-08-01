-- =====================================================
-- DEV: Bulk Generated Flights (100 flights)
-- Schema: flights
-- Depends on: 02_routes.sql, 01_airplanes.sql
-- Purpose: Generate large volume of flights for development/demo
-- =====================================================

DO $$
DECLARE
    _count INTEGER := 0;
    _routeId UUID;
    _airplaneId UUID;
    _flightNumber VARCHAR(20);
    _flightId UUID;
    _departureTime TIMESTAMP;
    _arrivalTime TIMESTAMP;
    _basePrice DECIMAL(18,2);
    _seatCount INTEGER;
    _totalSeats INTEGER;
    _daysToAdd INTEGER;
    _hoursToAdd INTEGER;
    _minutesToAdd INTEGER;
    _estimatedDuration INTEGER;
BEGIN
    WHILE _count < 100 LOOP
        _count := _count + 1;

        -- Pick random route
        SELECT r.id, COALESCE(r.estimated_duration_minutes, 180)
        INTO _routeId, _estimatedDuration
        FROM flights.routes r ORDER BY gen_random_uuid() LIMIT 1;

        -- Pick random airplane
        SELECT id INTO _airplaneId FROM flights.airplanes ORDER BY gen_random_uuid() LIMIT 1;

        IF _routeId IS NOT NULL AND _airplaneId IS NOT NULL THEN
            _flightNumber := 'FL' || (1000 + _count)::TEXT;
            _flightId := gen_random_uuid();
            _daysToAdd := FLOOR(RANDOM() * 30)::INTEGER;
            _hoursToAdd := FLOOR(RANDOM() * 24)::INTEGER;
            _minutesToAdd := (FLOOR(RANDOM() * 12)::INTEGER) * 5;
            _basePrice := CAST(FLOOR(RANDOM() * 751 + 50) AS DECIMAL(18,2));

            _departureTime := NOW() + (_daysToAdd || ' days')::INTERVAL + (_hoursToAdd || ' hours')::INTERVAL + (_minutesToAdd || ' minutes')::INTERVAL;
            _arrivalTime := _departureTime + (_estimatedDuration || ' minutes')::INTERVAL;

            INSERT INTO flights.flights (id, route_id, airplane_id, flight_number, departure_time, arrival_time, base_price, currency, status, is_deleted, created_at, updated_at)
            VALUES (_flightId, _routeId, _airplaneId, _flightNumber, _departureTime, _arrivalTime, _basePrice, 'USD', 0, FALSE, NOW(), NOW())
            ON CONFLICT DO NOTHING;

            -- Generate 10-20 seats per flight
            _totalSeats := 10 + FLOOR(RANDOM() * 11)::INTEGER;
            _seatCount := 0;
            WHILE _seatCount < _totalSeats LOOP
                _seatCount := _seatCount + 1;
                INSERT INTO flights.flight_seats (id, flight_id, seat_number, seat_class, price_override, is_available, is_extra_legroom)
                VALUES (
                    gen_random_uuid(), _flightId,
                    _seatCount::TEXT || CASE WHEN _seatCount % 2 = 0 THEN 'A' ELSE 'B' END,
                    CASE WHEN _seatCount <= 2 THEN 2 ELSE 0 END,
                    CASE WHEN _seatCount <= 2 THEN _basePrice * 2 ELSE NULL END,
                    TRUE,
                    CASE WHEN _seatCount <= 4 THEN TRUE ELSE FALSE END
                ) ON CONFLICT DO NOTHING;
            END LOOP;
        END IF;
    END LOOP;
END $$;
