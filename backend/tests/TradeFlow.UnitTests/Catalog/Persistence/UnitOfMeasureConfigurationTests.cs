using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using TradeFlow.Modules.Catalog.Domain.Products;
using TradeFlow.Modules.Catalog.Domain.UnitsOfMeasure;
using TradeFlow.Modules.Catalog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace TradeFlow.UnitTests.Catalog.Persistence;

public sealed class UnitOfMeasureConfigurationTests
{
    [Fact]
    public void UnitOfMeasure_MapsToExpectedTableAndSchema()
    {
        using CatalogDbContext context = CreateContext();

        IEntityType entityType =
            GetEntityType<UnitOfMeasure>(context);

        Assert.Equal(
            "UnitsOfMeasure",
            entityType.GetTableName());

        Assert.Equal(
            "catalog",
            entityType.GetSchema());

        IKey primaryKey =
            entityType.FindPrimaryKey()
            ?? throw new InvalidOperationException(
                "UnitOfMeasure primary key was not configured.");

        IProperty idProperty =
            Assert.Single(primaryKey.Properties);

        Assert.Equal(
            nameof(UnitOfMeasure.Id),
            idProperty.Name);

        Assert.Equal(
            ValueGenerated.Never,
            idProperty.ValueGenerated);
    }

    [Fact]
    public void UnitOfMeasure_ConfiguresCodeCorrectly()
    {
        using CatalogDbContext context = CreateContext();

        IEntityType entityType =
            GetEntityType<UnitOfMeasure>(context);

        IProperty codeProperty =
            GetProperty(
                entityType,
                nameof(UnitOfMeasure.Code));

        Assert.Equal(10, codeProperty.GetMaxLength());
        Assert.Equal(false, codeProperty.IsUnicode());
        Assert.False(codeProperty.IsNullable);
    }

    [Fact]
    public void UnitOfMeasure_ConfiguresNameCorrectly()
    {
        using CatalogDbContext context = CreateContext();

        IEntityType entityType =
            GetEntityType<UnitOfMeasure>(context);

        IProperty nameProperty =
            GetProperty(
                entityType,
                nameof(UnitOfMeasure.Name));

        Assert.Equal(50, nameProperty.GetMaxLength());
        Assert.Equal(true, nameProperty.IsUnicode());
        Assert.False(nameProperty.IsNullable);
    }

    [Fact]
    public void UnitOfMeasure_ConfiguresStatusAsNonUnicodeString()
    {
        using CatalogDbContext context = CreateContext();

        IEntityType entityType =
            GetEntityType<UnitOfMeasure>(context);

        IProperty statusProperty =
            GetProperty(
                entityType,
                nameof(UnitOfMeasure.Status));

        Assert.Equal(20, statusProperty.GetMaxLength());
        Assert.Equal(false, statusProperty.IsUnicode());
        Assert.False(statusProperty.IsNullable);

        Assert.Equal(
            typeof(string),
            statusProperty.GetProviderClrType());
    }

    [Fact]
    public void UnitOfMeasure_ConfiguresRowVersionForConcurrency()
    {
        using CatalogDbContext context = CreateContext();

        IEntityType entityType =
            GetEntityType<UnitOfMeasure>(context);

        IProperty rowVersionProperty =
            GetProperty(
                entityType,
                nameof(UnitOfMeasure.RowVersion));

        Assert.True(
            rowVersionProperty.IsConcurrencyToken);

        Assert.Equal(
            ValueGenerated.OnAddOrUpdate,
            rowVersionProperty.ValueGenerated);
    }

    [Fact]
    public void UnitOfMeasure_ConfiguresUniqueCodeIndex()
    {
        using CatalogDbContext context = CreateContext();

        IEntityType entityType =
            GetEntityType<UnitOfMeasure>(context);

        IIndex codeIndex =
            entityType.GetIndexes()
                .Single(index =>
                    index.Properties.Count == 1 &&
                    index.Properties[0].Name ==
                        nameof(UnitOfMeasure.Code));

        Assert.True(codeIndex.IsUnique);

        Assert.Equal(
            "UX_UnitsOfMeasure_Code",
            codeIndex.GetDatabaseName());
    }

    [Fact]
    public void UnitOfMeasure_ConfiguresSupportedCodeCheckConstraint()
    {
        using CatalogDbContext context = CreateContext();

        IEntityType entityType =
            GetEntityType<UnitOfMeasure>(context);

        var checkConstraint =
            Assert.Single(
                entityType.GetCheckConstraints());

        Assert.Equal(
            "CK_UnitsOfMeasure_Code",
            checkConstraint.Name);

        Assert.Equal(
            "[Code] IN ('EA', 'KG', 'L', 'M')",
            checkConstraint.Sql);
    }

    [Fact]
    public void UnitOfMeasure_SeedsExpectedSystemUnits()
    {
        using CatalogDbContext context = CreateContext();

        IEntityType entityType =
            GetEntityType<UnitOfMeasure>(context);

        IReadOnlyList<IDictionary<string, object?>>
            seedData =
                entityType.GetSeedData().ToList();

        Dictionary<string, (Guid Id, string Name)>
            expectedUnits =
                new()
                {
                    ["EA"] = (
                        Guid.Parse(
                            "10000000-0000-0000-0000-000000000001"),
                        "Each"),

                    ["KG"] = (
                        Guid.Parse(
                            "10000000-0000-0000-0000-000000000002"),
                        "Kilogram"),

                    ["L"] = (
                        Guid.Parse(
                            "10000000-0000-0000-0000-000000000003"),
                        "Litre"),

                    ["M"] = (
                        Guid.Parse(
                            "10000000-0000-0000-0000-000000000004"),
                        "Metre")
                };

        Assert.Equal(
            expectedUnits.Count,
            seedData.Count);

        foreach (IDictionary<string, object?> row in seedData)
        {
            string code =
                Assert.IsType<string>(
                    row[nameof(UnitOfMeasure.Code)]);

            Assert.True(
                expectedUnits.TryGetValue(
                    code,
                    out var expected));

            Assert.Equal(
                expected.Id,
                Assert.IsType<Guid>(
                    row[nameof(UnitOfMeasure.Id)]));

            Assert.Equal(
                expected.Name,
                Assert.IsType<string>(
                    row[nameof(UnitOfMeasure.Name)]));

            Assert.Equal(
                UnitOfMeasureStatus.Active,
                Assert.IsType<UnitOfMeasureStatus>(
                    row[nameof(UnitOfMeasure.Status)]));
        }
    }

    [Fact]
    public void Product_ConfiguresRestrictedUnitOfMeasureForeignKey()
    {
        using CatalogDbContext context = CreateContext();

        IEntityType productEntityType =
            GetEntityType<Product>(context);

        IForeignKey foreignKey =
            productEntityType.GetForeignKeys()
                .Single(key =>
                    key.PrincipalEntityType.ClrType ==
                    typeof(UnitOfMeasure));

        IProperty foreignKeyProperty =
            Assert.Single(foreignKey.Properties);

        Assert.Equal(
            nameof(Product.UnitOfMeasureId),
            foreignKeyProperty.Name);

        Assert.Equal(
            DeleteBehavior.Restrict,
            foreignKey.DeleteBehavior);

        Assert.Equal(
            "FK_Products_UnitsOfMeasure_UnitOfMeasureId",
            foreignKey.GetConstraintName());
    }

    private static CatalogDbContext CreateContext()
    {
        DbContextOptions<CatalogDbContext> options =
            new DbContextOptionsBuilder<CatalogDbContext>()
                .UseSqlServer(
                    "Server=localhost;"
                    + "Database=TradeFlowModelTests;"
                    + "User Id=sa;"
                    + "Password=NotUsedForModelTests123!;"
                    + "TrustServerCertificate=True")
                .Options;

        return new CatalogDbContext(options);
    }

    private static IEntityType GetEntityType<TEntity>(
    CatalogDbContext context)
    {
        IModel designTimeModel =
            context.GetService<IDesignTimeModel>().Model;

        return designTimeModel.FindEntityType(typeof(TEntity))
            ?? throw new InvalidOperationException(
                $"Entity '{typeof(TEntity).Name}' "
                + "was not found in the EF Core design-time model.");
    }

    private static IProperty GetProperty(
        IEntityType entityType,
        string propertyName)
    {
        return entityType.FindProperty(propertyName)
            ?? throw new InvalidOperationException(
                $"Property '{propertyName}' "
                + "was not configured.");
    }
}
