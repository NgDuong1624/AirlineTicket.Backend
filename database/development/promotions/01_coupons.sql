-- =====================================================
-- DEV: Sample Coupons
-- Schema: promotions
-- =====================================================

INSERT INTO promotions.coupons (id, code, description, discount_type, discount_value, min_order_value, max_discount_amount, start_date, end_date, usage_limit, usage_count, is_active, is_deleted) VALUES
('C1F3E2D4-BCDE-4F01-2345-6789ABCDEF01', 'SUMMER2025', 'Giảm giá 10% dịp Hè', 0, 10.00, 100.00, 50.00, NOW(), NOW() + INTERVAL '3 months', 1000, 0, TRUE, FALSE),
('C2F3E2D4-BCDE-4F01-2345-6789ABCDEF02', 'WELCOME10', 'Giảm 10$ cho đơn hàng đầu tiên', 1, 10.00, 0.00, 10.00, NOW(), NOW() + INTERVAL '1 year', 5000, 0, TRUE, FALSE)
ON CONFLICT (id) DO NOTHING;
