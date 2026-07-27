IF OBJECT_ID(N'[catalog].[__EFMigrationsHistory]') IS NULL
BEGIN
    IF SCHEMA_ID(N'catalog') IS NULL EXEC(N'CREATE SCHEMA [catalog];');
    CREATE TABLE [catalog].[__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [catalog].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260726103005_InitialCatalogProducts'
)
BEGIN
    IF SCHEMA_ID(N'catalog') IS NULL EXEC(N'CREATE SCHEMA [catalog];');
END;

IF NOT EXISTS (
    SELECT * FROM [catalog].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260726103005_InitialCatalogProducts'
)
BEGIN
    CREATE TABLE [catalog].[Products] (
        [Id] uniqueidentifier NOT NULL,
        [OrganisationId] uniqueidentifier NOT NULL,
        [SKU] varchar(50) NOT NULL,
        [Name] nvarchar(100) NOT NULL,
        [Description] nvarchar(1000) NOT NULL DEFAULT N'',
        [UnitOfMeasureId] uniqueidentifier NOT NULL,
        [ProductCategoryId] uniqueidentifier NULL,
        [TaxCategoryId] uniqueidentifier NOT NULL,
        [Status] varchar(20) NOT NULL,
        [CreatedAt] datetimeoffset(7) NOT NULL,
        [CreatedBy] uniqueidentifier NOT NULL,
        [LastModifiedAt] datetimeoffset(7) NULL,
        [LastModifiedBy] uniqueidentifier NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_Products] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [catalog].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260726103005_InitialCatalogProducts'
)
BEGIN
    CREATE INDEX [IX_Products_OrganisationId_Status] ON [catalog].[Products] ([OrganisationId], [Status]);
END;

IF NOT EXISTS (
    SELECT * FROM [catalog].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260726103005_InitialCatalogProducts'
)
BEGIN
    CREATE UNIQUE INDEX [UX_Products_OrganisationId_Sku] ON [catalog].[Products] ([OrganisationId], [SKU]);
END;

IF NOT EXISTS (
    SELECT * FROM [catalog].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260726103005_InitialCatalogProducts'
)
BEGIN
    INSERT INTO [catalog].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260726103005_InitialCatalogProducts', N'10.0.10');
END;

COMMIT;
GO

