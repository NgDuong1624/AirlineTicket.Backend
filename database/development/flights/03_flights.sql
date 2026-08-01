-- =====================================================
-- DEV: Sample Flights
-- Schema: flights
-- Depends on: 01_airplanes.sql, 02_routes.sql
-- =====================================================

-- Basic sample flights
INSERT INTO flights.flights (id, route_id, airplane_id, flight_number, departure_time, arrival_time, base_price, currency, status, is_deleted, created_at, updated_at) VALUES
('F1F3E2D4-BCDE-4F01-2345-6789ABCDEF01', '91F3E2D4-BCDE-4F01-2345-6789ABCDEF01', '81F3E2D4-BCDE-4F01-2345-6789ABCDEF01', 'VN213', NOW() + INTERVAL '1 day', NOW() + INTERVAL '1 day' + INTERVAL '130 minutes', 80.00, 'USD', 0, FALSE, NOW(), NOW()),
('F2F3E2D4-BCDE-4F01-2345-6789ABCDEF02', '92F3E2D4-BCDE-4F01-2345-6789ABCDEF02', '83F3E2D4-BCDE-4F01-2345-6789ABCDEF03', 'VJ120', NOW() + INTERVAL '2 days', NOW() + INTERVAL '2 days' + INTERVAL '130 minutes', 50.00, 'USD', 0, FALSE, NOW(), NOW())
ON CONFLICT (id) DO NOTHING;
