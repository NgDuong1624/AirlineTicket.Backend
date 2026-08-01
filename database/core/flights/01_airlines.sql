-- =====================================================
-- CORE: Airlines (Reference data)
-- Schema: flights
-- =====================================================

INSERT INTO flights.airlines (id, iata_code, name, logo_url, base_country, is_active, is_deleted, created_at) VALUES
('A1F3E2D4-BCDE-4F01-2345-6789ABCDEF01', 'VN', 'Vietnam Airlines', 'https://images.vietnamairlines.com/logos/vna-logo.png', 'Vietnam', TRUE, FALSE, NOW()),
('B2F3E2D4-BCDE-4F01-2345-6789ABCDEF02', 'VJ', 'VietJet Air', 'https://www.vietjetair.com/static/media/logo.8efdcd6f.svg', 'Vietnam', TRUE, FALSE, NOW()),
('C3F3E2D4-BCDE-4F01-2345-6789ABCDEF03', 'QH', 'Bamboo Airways', 'https://www.bambooairways.com/logo.png', 'Vietnam', TRUE, FALSE, NOW()),
('D4F3E2D4-BCDE-4F01-2345-6789ABCDEF04', 'VU', 'Vietravel Airlines', 'https://www.vietravelairlines.com/logo.png', 'Vietnam', TRUE, FALSE, NOW()),
('E1F3E2D4-BCDE-4F01-2345-6789ABCDEF27', 'SQ', 'Singapore Airlines', 'https://www.singaporeair.com/logo.png', 'Singapore', TRUE, FALSE, NOW()),
('E2F3E2D4-BCDE-4F01-2345-6789ABCDEF28', 'JL', 'Japan Airlines', 'https://www.jal.co.jp/logo.png', 'Japan', TRUE, FALSE, NOW()),
('E3F3E2D4-BCDE-4F01-2345-6789ABCDEF29', 'KE', 'Korean Air', 'https://www.koreanair.com/logo.png', 'South Korea', TRUE, FALSE, NOW()),
('E4F3E2D4-BCDE-4F01-2345-6789ABCDEF30', 'CX', 'Cathay Pacific', 'https://www.cathaypacific.com/logo.png', 'Hong Kong', TRUE, FALSE, NOW())
ON CONFLICT (id) DO NOTHING;
