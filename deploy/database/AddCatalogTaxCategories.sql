BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [catalog].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730113938_AddCatalogTaxCategories'
)
BEGIN
    CREATE TABLE [catalog].[TaxCategories] (
        [Id] uniqueidentifier NOT NULL,
        [Code] varchar(30) NOT NULL,
        [Name] nvarchar(100) NOT NULL,
        [Description] nvarchar(500) NOT NULL DEFAULT N'',
        [Rate] decimal(5,4) NOT NULL,
        [Treatment] varchar(20) NOT NULL,
        [Status] varchar(20) NOT NULL,
        [EffectiveFrom] date NOT NULL,
        [EffectiveTo] date NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_TaxCategories] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_TaxCategories_EffectivePeriod] CHECK ([EffectiveTo] IS NULL OR [EffectiveTo] >= [EffectiveFrom]),
        CONSTRAINT [CK_TaxCategories_Rate] CHECK ([Rate] >= 0 AND [Rate] <= 1),
        CONSTRAINT [CK_TaxCategories_TreatmentRate] CHECK (([Treatment] = 'StandardRated' AND [Rate] > 0) OR ([Treatment] IN ('ZeroRated', 'Exempt') AND [Rate] = 0))
    );
END;

IF NOT EXISTS (
    SELECT * FROM [catalog].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730113938_AddCatalogTaxCategories'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Code', N'Description', N'EffectiveFrom', N'EffectiveTo', N'Name', N'Rate', N'Status', N'Treatment') AND [object_id] = OBJECT_ID(N'[catalog].[TaxCategories]'))
        SET IDENTITY_INSERT [catalog].[TaxCategories] ON;
    EXEC(N'INSERT INTO [catalog].[TaxCategories] ([Id], [Code], [Description], [EffectiveFrom], [EffectiveTo], [Name], [Rate], [Status], [Treatment])
    VALUES (''20000000-0000-0000-0000-000000000001'', ''GST15'', N''Standard-rated supplies at 15% GST.'', ''2010-10-01'', NULL, N''Standard GST'', 0.15, ''Active'', ''StandardRated''),
    (''20000000-0000-0000-0000-000000000002'', ''GST0'', N''Zero-rated supplies charged at 0% GST.'', ''2010-10-01'', NULL, N''Zero-rated GST'', 0.0, ''Active'', ''ZeroRated''),
    (''20000000-0000-0000-0000-000000000003'', ''EXEMPT'', N''Supplies that are exempt from GST.'', ''2010-10-01'', NULL, N''GST exempt'', 0.0, ''Active'', ''Exempt'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Code', N'Description', N'EffectiveFrom', N'EffectiveTo', N'Name', N'Rate', N'Status', N'Treatment') AND [object_id] = OBJECT_ID(N'[catalog].[TaxCategories]'))
        SET IDENTITY_INSERT [catalog].[TaxCategories] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [catalog].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730113938_AddCatalogTaxCategories'
)
BEGIN
    CREATE INDEX [IX_Products_TaxCategoryId] ON [catalog].[Products] ([TaxCategoryId]);
END;

IF NOT EXISTS (
    SELECT * FROM [catalog].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730113938_AddCatalogTaxCategories'
)
BEGIN
    CREATE INDEX [IX_TaxCategories_Status_EffectiveFrom_EffectiveTo] ON [catalog].[TaxCategories] ([Status], [EffectiveFrom], [EffectiveTo]);
END;

IF NOT EXISTS (
    SELECT * FROM [catalog].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730113938_AddCatalogTaxCategories'
)
BEGIN
    CREATE UNIQUE INDEX [UX_TaxCategories_Code] ON [catalog].[TaxCategories] ([Code]);
END;

IF NOT EXISTS (
    SELECT * FROM [catalog].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730113938_AddCatalogTaxCategories'
)
BEGIN
    ALTER TABLE [catalog].[Products] ADD CONSTRAINT [FK_Products_TaxCategories_TaxCategoryId] FOREIGN KEY ([TaxCategoryId]) REFERENCES [catalog].[TaxCategories] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [catalog].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730113938_AddCatalogTaxCategories'
)
BEGIN
    INSERT INTO [catalog].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260730113938_AddCatalogTaxCategories', N'10.0.10');
END;

COMMIT;
GO
