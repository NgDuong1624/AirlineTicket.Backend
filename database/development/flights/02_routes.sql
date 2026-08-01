-- =====================================================
-- DEV: Sample Routes
-- Schema: flights
-- Depends on: core/flights/01_airlines.sql, core/flights/02_airports.sql
-- =====================================================

-- Basic routes
INSERT INTO flights.routes (id, airline_id, origin_airport_id, destination_airport_id, distance_km, estimated_duration_minutes, is_deleted) VALUES
('91F3E2D4-BCDE-4F01-2345-6789ABCDEF01', 'A1F3E2D4-BCDE-4F01-2345-6789ABCDEF01', 'E5F3E2D4-BCDE-4F01-2345-6789ABCDEF05', 'F6F3E2D4-BCDE-4F01-2345-6789ABCDEF06', 1160, 130, FALSE), -- HAN -> SGN (VN)
('92F3E2D4-BCDE-4F01-2345-6789ABCDEF02', 'B2F3E2D4-BCDE-4F01-2345-6789ABCDEF02', 'F6F3E2D4-BCDE-4F01-2345-6789ABCDEF06', 'E5F3E2D4-BCDE-4F01-2345-6789ABCDEF05', 1160, 130, FALSE), -- SGN -> HAN (VJ)
('93F3E2D4-BCDE-4F01-2345-6789ABCDEF03', 'A1F3E2D4-BCDE-4F01-2345-6789ABCDEF01', 'E5F3E2D4-BCDE-4F01-2345-6789ABCDEF05', '07F3E2D4-BCDE-4F01-2345-6789ABCDEF07', 627, 85, FALSE)   -- HAN -> DAD (VN)
ON CONFLICT (id) DO NOTHING;

-- Generate hub routes from SGN and HAN to all airports
DO $$
DECLARE
    _hubSGN UUID := (SELECT id FROM flights.airports WHERE iata_code = 'SGN' LIMIT 1);
    _hubHAN UUID := (SELECT id FROM flights.airports WHERE iata_code = 'HAN' LIMIT 1);
BEGIN
    IF _hubSGN IS NULL OR _hubHAN IS NULL THEN
        RETURN;
    END IF;

    -- Routes from/to SGN
    INSERT INTO flights.routes (id, airline_id, origin_airport_id, destination_airport_id, distance_km, estimated_duration_minutes, is_deleted)
    SELECT gen_random_uuid(),
        (SELECT id FROM flights.airlines ORDER BY gen_random_uuid() LIMIT 1),
        _hubSGN, id,
        FLOOR(RANDOM() * 2000 + 500),
        FLOOR(RANDOM() * 180 + 60),
        FALSE
    FROM flights.airports WHERE id <> _hubSGN
    AND NOT EXISTS (
        SELECT 1 FROM flights.routes WHERE origin_airport_id = _hubSGN AND destination_airport_id = flights.airports.id
    );

    INSERT INTO flights.routes (id, airline_id, origin_airport_id, destination_airport_id, distance_km, estimated_duration_minutes, is_deleted)
    SELECT gen_random_uuid(),
        (SELECT id FROM flights.airlines ORDER BY gen_random_uuid() LIMIT 1),
        id, _hubSGN,
        FLOOR(RANDOM() * 2000 + 500),
        FLOOR(RANDOM() * 180 + 60),
        FALSE
    FROM flights.airports WHERE id <> _hubSGN
    AND NOT EXISTS (
        SELECT 1 FROM flights.routes WHERE origin_airport_id = flights.airports.id AND destination_airport_id = _hubSGN
    );

    -- Routes from/to HAN
    INSERT INTO flights.routes (id, airline_id, origin_airport_id, destination_airport_id, distance_km, estimated_duration_minutes, is_deleted)
    SELECT gen_random_uuid(),
        (SELECT id FROM flights.airlines ORDER BY gen_random_uuid() LIMIT 1),
        _hubHAN, id,
        FLOOR(RANDOM() * 2000 + 500),
        FLOOR(RANDOM() * 180 + 60),
        FALSE
    FROM flights.airports WHERE id <> _hubHAN
    AND NOT EXISTS (
        SELECT 1 FROM flights.routes WHERE origin_airport_id = _hubHAN AND destination_airport_id = flights.airports.id
    );

    INSERT INTO flights.routes (id, airline_id, origin_airport_id, destination_airport_id, distance_km, estimated_duration_minutes, is_deleted)
    SELECT gen_random_uuid(),
        (SELECT id FROM flights.airlines ORDER BY gen_random_uuid() LIMIT 1),
        id, _hubHAN,
        FLOOR(RANDOM() * 2000 + 500),
        FLOOR(RANDOM() * 180 + 60),
        FALSE
    FROM flights.airports WHERE id <> _hubHAN
    AND NOT EXISTS (
        SELECT 1 FROM flights.routes WHERE origin_airport_id = flights.airports.id AND destination_airport_id = _hubHAN
    );
END $$;
