-- =====================================================
-- DEV: Sample Flight Seats
-- Schema: flights
-- Depends on: 03_flights.sql
-- =====================================================

-- Seat configuration for VN213
INSERT INTO flights.flight_seats (id, flight_id, seat_number, seat_class, price_override, is_available, is_extra_legroom) VALUES
(gen_random_uuid(), 'F1F3E2D4-BCDE-4F01-2345-6789ABCDEF01', '01A', 2, 150.00, TRUE, TRUE),
(gen_random_uuid(), 'F1F3E2D4-BCDE-4F01-2345-6789ABCDEF01', '01B', 2, 150.00, TRUE, TRUE),
(gen_random_uuid(), 'F1F3E2D4-BCDE-4F01-2345-6789ABCDEF01', '10A', 0, NULL, TRUE, FALSE),
(gen_random_uuid(), 'F1F3E2D4-BCDE-4F01-2345-6789ABCDEF01', '10B', 0, NULL, TRUE, FALSE),
(gen_random_uuid(), 'F1F3E2D4-BCDE-4F01-2345-6789ABCDEF01', '11A', 0, NULL, TRUE, FALSE),
(gen_random_uuid(), 'F1F3E2D4-BCDE-4F01-2345-6789ABCDEF01', '11B', 0, NULL, TRUE, FALSE)
ON CONFLICT (id) DO NOTHING;

-- Seat configuration for VJ120
INSERT INTO flights.flight_seats (id, flight_id, seat_number, seat_class, price_override, is_available, is_extra_legroom) VALUES
(gen_random_uuid(), 'F2F3E2D4-BCDE-4F01-2345-6789ABCDEF02', '01A', 1, 80.00, TRUE, TRUE),
(gen_random_uuid(), 'F2F3E2D4-BCDE-4F01-2345-6789ABCDEF02', '01B', 1, 80.00, TRUE, TRUE),
(gen_random_uuid(), 'F2F3E2D4-BCDE-4F01-2345-6789ABCDEF02', '05A', 0, NULL, TRUE, FALSE),
(gen_random_uuid(), 'F2F3E2D4-BCDE-4F01-2345-6789ABCDEF02', '05B', 0, NULL, TRUE, FALSE)
ON CONFLICT (id) DO NOTHING;
