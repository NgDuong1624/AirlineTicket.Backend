-- =====================================================
-- DEV: Sample Categories
-- Schema: cms
-- =====================================================

INSERT INTO cms.categories (id, name, slug, is_deleted) VALUES
('CA11E2D4-BCDE-4F01-2345-6789ABCDEF01', 'Tin Tức Khuyến Mãi', 'tin-tuc-khuyen-mai', FALSE),
('CA22E2D4-BCDE-4F01-2345-6789ABCDEF02', 'Cẩm Nang Du Lịch', 'cam-nang-du-lich', FALSE)
ON CONFLICT (id) DO NOTHING;
