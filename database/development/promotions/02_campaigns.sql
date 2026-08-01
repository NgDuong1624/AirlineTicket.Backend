-- =====================================================
-- DEV: Sample Campaigns
-- Schema: promotions
-- =====================================================

INSERT INTO promotions.campaigns (id, title, banner_url, content, start_date, end_date, is_featured, is_deleted) VALUES
('CC13E2D4-BCDE-4F01-2345-6789ABCDEF01', 'Siêu Hè Rực Rỡ 2025', 'https://images.vietnamairlines.com/banners/summer-2025.jpg', 'Giảm giá lên đến 20% các chặng bay nội địa và quốc tế dịp hè từ 01/06 đến 31/08/2025.', NOW(), NOW() + INTERVAL '3 months', TRUE, FALSE),
('CC23E2D4-BCDE-4F01-2345-6789ABCDEF02', 'Mùa Thu Vàng', 'https://images.vietnamairlines.com/banners/autumn-2025.jpg', 'Đón thu vàng cùng ngập tràn khuyến mãi vé bay khứ hồi giá cực tốt.', NOW() + INTERVAL '3 months', NOW() + INTERVAL '5 months', FALSE, FALSE)
ON CONFLICT (id) DO NOTHING;
