-- =====================================================
-- Seed Notification Templates (i18n)
-- Locales: vi, en, zh, ja, ko, fr
-- =====================================================

USE [master]
GO

-- 1. English (en)
IF NOT EXISTS (SELECT 1 FROM [notifications].[NotificationTemplates] WHERE [Code] = 'FLIGHT_CREATED' AND [Language] = 'en')
BEGIN
    INSERT INTO [notifications].[NotificationTemplates] ([Id], [Code], [Subject], [BodyTemplate], [Language], [CreatedAt])
    VALUES (NEWID(), 'FLIGHT_CREATED', 'New Flight Created: {{FlightNumber}}', 'A new flight {{FlightNumber}} from {{Origin}} to {{Destination}} has been created by a partner.', 'en', GETUTCDATE())
END
GO

-- 2. Vietnamese (vi)
IF NOT EXISTS (SELECT 1 FROM [notifications].[NotificationTemplates] WHERE [Code] = 'FLIGHT_CREATED' AND [Language] = 'vi')
BEGIN
    INSERT INTO [notifications].[NotificationTemplates] ([Id], [Code], [Subject], [BodyTemplate], [Language], [CreatedAt])
    VALUES (NEWID(), 'FLIGHT_CREATED', 'Chuyến bay mới được tạo: {{FlightNumber}}', 'Chuyến bay mới {{FlightNumber}} từ {{Origin}} đến {{Destination}} vừa được tạo bởi đối tác.', 'vi', GETUTCDATE())
END
GO

-- 3. Chinese (zh)
IF NOT EXISTS (SELECT 1 FROM [notifications].[NotificationTemplates] WHERE [Code] = 'FLIGHT_CREATED' AND [Language] = 'zh')
BEGIN
    INSERT INTO [notifications].[NotificationTemplates] ([Id], [Code], [Subject], [BodyTemplate], [Language], [CreatedAt])
    VALUES (NEWID(), 'FLIGHT_CREATED', '新航班已创建: {{FlightNumber}}', '合作伙伴已创建从 {{Origin}} 到 {{Destination}} 的新航班 {{FlightNumber}}。', 'zh', GETUTCDATE())
END
GO

-- 4. Japanese (ja)
IF NOT EXISTS (SELECT 1 FROM [notifications].[NotificationTemplates] WHERE [Code] = 'FLIGHT_CREATED' AND [Language] = 'ja')
BEGIN
    INSERT INTO [notifications].[NotificationTemplates] ([Id], [Code], [Subject], [BodyTemplate], [Language], [CreatedAt])
    VALUES (NEWID(), 'FLIGHT_CREATED', '新しいフライトが作成されました: {{FlightNumber}}', 'パートナーによって {{Origin}} から {{Destination}} への新しいフライト {{FlightNumber}} が作成されました。', 'ja', GETUTCDATE())
END
GO

-- 5. Korean (ko)
IF NOT EXISTS (SELECT 1 FROM [notifications].[NotificationTemplates] WHERE [Code] = 'FLIGHT_CREATED' AND [Language] = 'ko')
BEGIN
    INSERT INTO [notifications].[NotificationTemplates] ([Id], [Code], [Subject], [BodyTemplate], [Language], [CreatedAt])
    VALUES (NEWID(), 'FLIGHT_CREATED', '새 항공편 생성됨: {{FlightNumber}}', '파트너가 {{Origin}}에서 {{Destination}}으로 가는 새 항공편 {{FlightNumber}}을(를) 생성했습니다.', 'ko', GETUTCDATE())
END
GO

-- 6. French (fr)
IF NOT EXISTS (SELECT 1 FROM [notifications].[NotificationTemplates] WHERE [Code] = 'FLIGHT_CREATED' AND [Language] = 'fr')
BEGIN
    INSERT INTO [notifications].[NotificationTemplates] ([Id], [Code], [Subject], [BodyTemplate], [Language], [CreatedAt])
    VALUES (NEWID(), 'FLIGHT_CREATED', 'Nouveau vol créé : {{FlightNumber}}', 'Un nouveau vol {{FlightNumber}} de {{Origin}} à {{Destination}} a été créé par un partenaire.', 'fr', GETUTCDATE())
END
GO