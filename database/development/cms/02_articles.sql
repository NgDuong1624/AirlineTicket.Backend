-- =====================================================
-- DEV: Sample Articles
-- Schema: cms
-- Depends on: 01_categories.sql, development/users/01_sample_users.sql
-- =====================================================

INSERT INTO cms.articles (id, category_id, author_id, title, slug, summary, content, thumbnail_url, published_at, status, view_count, is_deleted, created_at) VALUES
('AE10E2D4-BCDE-4F01-2345-6789ABCDEF01', 'CA22E2D4-BCDE-4F01-2345-6789ABCDEF02', 'D4B0F2A8-9B2F-4A9B-89E3-4E80D77BC901', 'Cẩm nang du lịch Phú Quốc từ A đến Z năm 2025', 'cam-nang-du-lich-phu-quoc-2025', 'Những lưu ý quan trọng khi đi du lịch Phú Quốc tự túc.', 'Bài viết này cung cấp toàn bộ kinh nghiệm bay, đặt khách sạn, ẩm thực và địa điểm vui chơi tại Phú Quốc cho du khách.', 'https://images.phuquoc.vn/thumbnail.jpg', NOW(), 1, 245, FALSE, NOW())
ON CONFLICT (id) DO NOTHING;
