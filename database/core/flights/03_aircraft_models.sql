-- =====================================================
-- CORE: Aircraft Models (Reference data)
-- Schema: flights
-- =====================================================

INSERT INTO flights.aircraft_models (id, name, manufacturer, total_seats, is_deleted) VALUES
('B7879000-BCDE-4F01-2345-6789ABCDEF01', 'Boeing 787-9 Dreamliner', 'Boeing', 294, FALSE),
('A3509000-BCDE-4F01-2345-6789ABCDEF02', 'Airbus A350-900', 'Airbus', 305, FALSE),
('A3212000-BCDE-4F01-2345-6789ABCDEF03', 'Airbus A321neo', 'Airbus', 230, FALSE)
ON CONFLICT (id) DO NOTHING;
