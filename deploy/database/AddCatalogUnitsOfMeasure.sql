BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [catalog].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728124918_AddCatalogUnitsOfMeasure'
)
BEGIN
    CREATE TABLE [catalog].[UnitsOfMeasure] (
        [Id] uniqueidentifier NOT NULL,
        [Code] varchar(10) NOT NULL,
        [Name] nvarchar(50) NOT NULL,
        [Status] varchar(20) NOT NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_UnitsOfMeasure] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_UnitsOfMeasure_Code] CHECK ([Code] IN ('EA', 'KG', 'L', 'M'))
    );
END;

IF NOT EXISTS (
    SELECT * FROM [catalog].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728124918_AddCatalogUnitsOfMeasure'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Code', N'Name', N'Status') AND [object_id] = OBJECT_ID(N'[catalog].[UnitsOfMeasure]'))
        SET IDENTITY_INSERT [catalog].[UnitsOfMeasure] ON;
    EXEC(N'INSERT INTO [catalog].[UnitsOfMeasure] ([Id], [Code], [Name], [Status])
    VALUES (''10000000-0000-0000-0000-000000000001'', ''EA'', N''Each'', ''Active''),
    (''10000000-0000-0000-0000-000000000002'', ''KG'', N''Kilogram'', ''Active''),
    (''10000000-0000-0000-0000-000000000003'', ''L'', N''Litre'', ''Active''),
    (''10000000-0000-0000-0000-000000000004'', ''M'', N''Metre'', ''Active'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Code', N'Name', N'Status') AND [object_id] = OBJECT_ID(N'[catalog].[UnitsOfMeasure]'))
        SET IDENTITY_INSERT [catalog].[UnitsOfMeasure] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [catalog].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728124918_AddCatalogUnitsOfMeasure'
)
BEGIN
    CREATE INDEX [IX_Products_UnitOfMeasureId] ON [catalog].[Products] ([UnitOfMeasureId]);
END;

IF NOT EXISTS (
    SELECT * FROM [catalog].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728124918_AddCatalogUnitsOfMeasure'
)
BEGIN
    CREATE UNIQUE INDEX [UX_UnitsOfMeasure_Code] ON [catalog].[UnitsOfMeasure] ([Code]);
END;

IF NOT EXISTS (
    SELECT * FROM [catalog].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728124918_AddCatalogUnitsOfMeasure'
)
BEGIN
    ALTER TABLE [catalog].[Products] ADD CONSTRAINT [FK_Products_UnitsOfMeasure_UnitOfMeasureId] FOREIGN KEY ([UnitOfMeasureId]) REFERENCES [catalog].[UnitsOfMeasure] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [catalog].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728124918_AddCatalogUnitsOfMeasure'
)
BEGIN
    INSERT INTO [catalog].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260728124918_AddCatalogUnitsOfMeasure', N'10.0.10');
END;

COMMIT;
GO
