BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [catalog].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729141203_AddCatalogProductCategories'
)
BEGIN
    CREATE TABLE [catalog].[ProductCategories] (
        [Id] uniqueidentifier NOT NULL,
        [OrganisationId] uniqueidentifier NOT NULL,
        [Code] varchar(50) NOT NULL,
        [Name] nvarchar(100) NOT NULL,
        [Description] nvarchar(1000) NOT NULL DEFAULT N'',
        [Status] varchar(20) NOT NULL,
        [CreatedAt] datetimeoffset(7) NOT NULL,
        [CreatedBy] uniqueidentifier NOT NULL,
        [LastModifiedAt] datetimeoffset(7) NULL,
        [LastModifiedBy] uniqueidentifier NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_ProductCategories] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [catalog].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729141203_AddCatalogProductCategories'
)
BEGIN
    CREATE INDEX [IX_Products_ProductCategoryId] ON [catalog].[Products] ([ProductCategoryId]);
END;

IF NOT EXISTS (
    SELECT * FROM [catalog].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729141203_AddCatalogProductCategories'
)
BEGIN
    CREATE INDEX [IX_ProductCategories_OrganisationId_Status] ON [catalog].[ProductCategories] ([OrganisationId], [Status]);
END;

IF NOT EXISTS (
    SELECT * FROM [catalog].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729141203_AddCatalogProductCategories'
)
BEGIN
    CREATE UNIQUE INDEX [UX_ProductCategories_OrganisationId_Code] ON [catalog].[ProductCategories] ([OrganisationId], [Code]);
END;

IF NOT EXISTS (
    SELECT * FROM [catalog].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729141203_AddCatalogProductCategories'
)
BEGIN
    ALTER TABLE [catalog].[Products] ADD CONSTRAINT [FK_Products_ProductCategories_ProductCategoryId] FOREIGN KEY ([ProductCategoryId]) REFERENCES [catalog].[ProductCategories] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [catalog].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729141203_AddCatalogProductCategories'
)
BEGIN
    INSERT INTO [catalog].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260729141203_AddCatalogProductCategories', N'10.0.10');
END;

COMMIT;
GO
