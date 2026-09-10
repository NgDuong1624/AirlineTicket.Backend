-- =====================================================
-- DEV: Sample Flights
-- Schema: flights
-- Depends on: 01_airplanes.sql, 02_routes.sql
-- =====================================================

-- Basic sample flights
INSERT INTO flights.flights (id, route_id, airplane_id, flight_number, departure_time, arrival_time, base_price, currency, status, is_deleted, created_at, updated_at) VALUES
('F1F3E2D4-BCDE-4F01-2345-6789ABCDEF01', '91F3E2D4-BCDE-4F01-2345-6789ABCDEF01', '81F3E2D4-BCDE-4F01-2345-6789ABCDEF01', 'VN213', NOW() + INTERVAL '1 day', NOW() + INTERVAL '1 day' + INTERVAL '130 minutes', 80.00, 'USD', 0, FALSE, NOW(), NOW()),
('F2F3E2D4-BCDE-4F01-2345-6789ABCDEF02', '92F3E2D4-BCDE-4F01-2345-6789ABCDEF02', '83F3E2D4-BCDE-4F01-2345-6789ABCDEF03', 'VJ120', NOW() + INTERVAL '2 days', NOW() + INTERVAL '2 days' + INTERVAL '130 minutes', 50.00, 'USD', 0, FALSE, NOW(), NOW()),
-- Live airborne flights for radar & flight-tracker demonstration
('F3F3E2D4-BCDE-4F01-2345-6789ABCDEF03', '91F3E2D4-BCDE-4F01-2345-6789ABCDEF01', '81F3E2D4-BCDE-4F01-2345-6789ABCDEF01', 'VN210', NOW() - INTERVAL '45 minutes', NOW() + INTERVAL '85 minutes', 95.00, 'USD', 1, FALSE, NOW(), NOW()),
('F4F3E2D4-BCDE-4F01-2345-6789ABCDEF04', '92F3E2D4-BCDE-4F01-2345-6789ABCDEF02', '83F3E2D4-BCDE-4F01-2345-6789ABCDEF03', 'VJ151', NOW() - INTERVAL '35 minutes', NOW() + INTERVAL '95 minutes', 65.00, 'USD', 1, FALSE, NOW(), NOW()),
('F5F3E2D4-BCDE-4F01-2345-6789ABCDEF05', '93F3E2D4-BCDE-4F01-2345-6789ABCDEF03', '84F3E2D4-BCDE-4F01-2345-6789ABCDEF04', 'QH202', NOW() - INTERVAL '25 minutes', NOW() + INTERVAL '60 minutes', 85.00, 'USD', 1, FALSE, NOW(), NOW())
ON CONFLICT (id) DO NOTHING;
