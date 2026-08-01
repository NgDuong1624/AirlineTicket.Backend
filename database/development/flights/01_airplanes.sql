-- =====================================================
-- DEV: Sample Airplanes
-- Schema: flights
-- Depends on: core/flights/01_airlines.sql, core/flights/03_aircraft_models.sql
-- =====================================================

INSERT INTO flights.airplanes (id, airline_id, model, registration_number, total_capacity, is_deleted) VALUES
('81F3E2D4-BCDE-4F01-2345-6789ABCDEF01', 'A1F3E2D4-BCDE-4F01-2345-6789ABCDEF01', 'Boeing 787-9 Dreamliner', 'VN-A861', 274, FALSE),
('82F3E2D4-BCDE-4F01-2345-6789ABCDEF02', 'A1F3E2D4-BCDE-4F01-2345-6789ABCDEF01', 'Airbus A350-900', 'VN-A886', 305, FALSE),
('83F3E2D4-BCDE-4F01-2345-6789ABCDEF03', 'B2F3E2D4-BCDE-4F01-2345-6789ABCDEF02', 'Airbus A321neo', 'VN-A600', 230, FALSE),
('84F3E2D4-BCDE-4F01-2345-6789ABCDEF04', 'C3F3E2D4-BCDE-4F01-2345-6789ABCDEF03', 'Boeing 787-9 Dreamliner', 'VN-A819', 294, FALSE)
ON CONFLICT (id) DO NOTHING;

-- Link airplanes to aircraft models
UPDATE flights.airplanes SET aircraft_model_id = 'B7879000-BCDE-4F01-2345-6789ABCDEF01'
WHERE id IN ('81F3E2D4-BCDE-4F01-2345-6789ABCDEF01', '84F3E2D4-BCDE-4F01-2345-6789ABCDEF04');

UPDATE flights.airplanes SET aircraft_model_id = 'A3509000-BCDE-4F01-2345-6789ABCDEF02'
WHERE id = '82F3E2D4-BCDE-4F01-2345-6789ABCDEF02';

UPDATE flights.airplanes SET aircraft_model_id = 'A3212000-BCDE-4F01-2345-6789ABCDEF03'
WHERE id = '83F3E2D4-BCDE-4F01-2345-6789ABCDEF03';

-- Generate airplane seats from model templates
DELETE FROM flights.airplane_seats;

INSERT INTO flights.airplane_seats (id, airplane_id, seat_number, seat_row, seat_column, seat_class, is_extra_legroom, price_multiplier)
SELECT 
    gen_random_uuid(),
    a.id,
    t.seat_number,
    t.seat_row,
    t.seat_column,
    t.seat_class,
    t.is_extra_legroom,
    t.price_multiplier
FROM flights.airplanes a
JOIN flights.aircraft_models am ON a.aircraft_model_id = am.id
JOIN flights.aircraft_model_seat_templates t ON am.id = t.aircraft_model_id
WHERE NOT EXISTS (
    SELECT 1 FROM flights.airplane_seats s
    WHERE s.airplane_id = a.id AND s.seat_number = t.seat_number
);
