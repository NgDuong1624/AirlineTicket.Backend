-- =====================================================
-- DEV: Sample Reviews
-- Schema: interactions
-- Depends on: development/users, development/flights
-- =====================================================

INSERT INTO interactions.reviews (id, user_id, airline_id, flight_id, rating, comment, is_verified_purchase, is_hidden, is_deleted, created_at) VALUES
('EE10E2D4-BCDE-4F01-2345-6789ABCDEF01', 'E2E3F4C5-1A2B-3C4D-5E6F-7A8B9C0D1E2F', 'A1F3E2D4-BCDE-4F01-2345-6789ABCDEF01', 'F1F3E2D4-BCDE-4F01-2345-6789ABCDEF01', 5, 'Dịch vụ của Vietnam Airlines rất tốt, bay đúng giờ, tiếp viên thân thiện.', TRUE, FALSE, FALSE, NOW())
ON CONFLICT (id) DO NOTHING;
