-- =====================================================
-- TEST: Minimal Airports for integration tests
-- Schema: flights
-- =====================================================
INSERT INTO flights.airports (id, iata_code, name_en, name_vi, city_en, city_vi, country_code, timezone, latitude, longitude, is_active, is_deleted) VALUES
('00000000-0000-0000-0000-000000000011', 'TST', 'Test Airport 1', 'Sân bay thử nghiệm 1', 'Test City 1', 'Thành phố thử nghiệm 1', 'TS', 'UTC', 0.0, 0.0, TRUE, FALSE),
('00000000-0000-0000-0000-000000000012', 'TET', 'Test Airport 2', 'Sân bay thử nghiệm 2', 'Test City 2', 'Thành phố thử nghiệm 2', 'TS', 'UTC', 0.0, 0.0, TRUE, FALSE)
ON CONFLICT (id) DO NOTHING;
