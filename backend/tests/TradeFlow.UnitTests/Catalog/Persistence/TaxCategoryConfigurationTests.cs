using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using TradeFlow.Modules.Catalog.Domain.Products;
using TradeFlow.Modules.Catalog.Domain.TaxCategories;
using TradeFlow.Modules.Catalog.Infrastructure.Persistence;

namespace TradeFlow.UnitTests.Catalog.Persistence;

public sealed class TaxCategoryConfigurationTests
{
    [Fact]
    public void TaxCategory_MapsToExpectedTableAndSchema()
    {
        using CatalogDbContext context = CreateContext();

        IEntityType entityType =
            GetEntityType<TaxCategory>(context);

        Assert.Equal(
            "TaxCategories",
            entityType.GetTableName());

        Assert.Equal(
            "catalog",
            entityType.GetSchema());

        IKey primaryKey =
            entityType.FindPrimaryKey()
            ?? throw new InvalidOperationException(
                "TaxCategory primary key was not configured.");

        IProperty idProperty =
            Assert.Single(primaryKey.Properties);

        Assert.Equal(
            nameof(TaxCategory.Id),
            idProperty.Name);

        Assert.Equal(
            ValueGenerated.Never,
            idProperty.ValueGenerated);
    }

    [Fact]
    public void TaxCategory_ConfiguresCodeCorrectly()
    {
        using CatalogDbContext context = CreateContext();

        IEntityType entityType =
            GetEntityType<TaxCategory>(context);

        IProperty codeProperty =
            GetProperty(
                entityType,
                nameof(TaxCategory.Code));

        Assert.False(codeProperty.IsNullable);
        Assert.Equal(30, codeProperty.GetMaxLength());
        Assert.Equal(false, codeProperty.IsUnicode());
    }

    [Fact]
    public void TaxCategory_ConfiguresNameCorrectly()
    {
        using CatalogDbContext context = CreateContext();

        IEntityType entityType =
            GetEntityType<TaxCategory>(context);

        IProperty nameProperty =
            GetProperty(
                entityType,
                nameof(TaxCategory.Name));

        Assert.False(nameProperty.IsNullable);
        Assert.Equal(100, nameProperty.GetMaxLength());
        Assert.Equal(true, nameProperty.IsUnicode());
    }

    [Fact]
    public void TaxCategory_ConfiguresDescriptionCorrectly()
    {
        using CatalogDbContext context = CreateContext();

        IEntityType entityType =
            GetEntityType<TaxCategory>(context);

        IProperty descriptionProperty =
            GetProperty(
                entityType,
                nameof(TaxCategory.Description));

        Assert.False(descriptionProperty.IsNullable);

        Assert.Equal(
            500,
            descriptionProperty.GetMaxLength());

        Assert.Equal(
            true,
            descriptionProperty.IsUnicode());

        Assert.Equal(
            string.Empty,
            descriptionProperty.GetDefaultValue());
    }

    [Fact]
    public void TaxCategory_ConfiguresRateAsDecimalFiveFour()
    {
        using CatalogDbContext context = CreateContext();

        IEntityType entityType =
            GetEntityType<TaxCategory>(context);

        IProperty rateProperty =
            GetProperty(
                entityType,
                nameof(TaxCategory.Rate));

        Assert.False(rateProperty.IsNullable);
        Assert.Equal(5, rateProperty.GetPrecision());
        Assert.Equal(4, rateProperty.GetScale());
    }

    [Fact]
    public void TaxCategory_ConfiguresTreatmentAsNonUnicodeString()
    {
        using CatalogDbContext context = CreateContext();

        IEntityType entityType =
            GetEntityType<TaxCategory>(context);

        IProperty treatmentProperty =
            GetProperty(
                entityType,
                nameof(TaxCategory.Treatment));

        Assert.False(treatmentProperty.IsNullable);
        Assert.Equal(20, treatmentProperty.GetMaxLength());
        Assert.Equal(false, treatmentProperty.IsUnicode());

        Assert.Equal(
            typeof(string),
            treatmentProperty.GetProviderClrType());
    }

    [Fact]
    public void TaxCategory_ConfiguresStatusAsNonUnicodeString()
    {
        using CatalogDbContext context = CreateContext();

        IEntityType entityType =
            GetEntityType<TaxCategory>(context);

        IProperty statusProperty =
            GetProperty(
                entityType,
                nameof(TaxCategory.Status));

        Assert.False(statusProperty.IsNullable);
        Assert.Equal(20, statusProperty.GetMaxLength());
        Assert.Equal(false, statusProperty.IsUnicode());

        Assert.Equal(
            typeof(string),
            statusProperty.GetProviderClrType());
    }

    [Fact]
    public void TaxCategory_ConfiguresEffectiveDatesCorrectly()
    {
        using CatalogDbContext context = CreateContext();

        IEntityType entityType =
            GetEntityType<TaxCategory>(context);

        IProperty effectiveFromProperty =
            GetProperty(
                entityType,
                nameof(TaxCategory.EffectiveFrom));

        IProperty effectiveToProperty =
            GetProperty(
                entityType,
                nameof(TaxCategory.EffectiveTo));

        Assert.False(effectiveFromProperty.IsNullable);

        Assert.Equal(
            "date",
            effectiveFromProperty.GetColumnType());

        Assert.True(effectiveToProperty.IsNullable);

        Assert.Equal(
            "date",
            effectiveToProperty.GetColumnType());
    }

    [Fact]
    public void TaxCategory_ConfiguresRowVersionForConcurrency()
    {
        using CatalogDbContext context = CreateContext();

        IEntityType entityType =
            GetEntityType<TaxCategory>(context);

        IProperty rowVersionProperty =
            GetProperty(
                entityType,
                nameof(TaxCategory.RowVersion));

        Assert.True(rowVersionProperty.IsConcurrencyToken);

        Assert.Equal(
            ValueGenerated.OnAddOrUpdate,
            rowVersionProperty.ValueGenerated);
    }

    [Fact]
    public void TaxCategory_ConfiguresCodeAsUniqueIndex()
    {
        using CatalogDbContext context = CreateContext();

        IEntityType entityType =
            GetEntityType<TaxCategory>(context);

        IIndex index = FindIndex(
            entityType,
            nameof(TaxCategory.Code));

        Assert.True(index.IsUnique);

        Assert.Equal(
            "UX_TaxCategories_Code",
            index.GetDatabaseName());
    }

    [Fact]
    public void TaxCategory_ConfiguresEffectiveDateQueryIndex()
    {
        using CatalogDbContext context = CreateContext();

        IEntityType entityType =
            GetEntityType<TaxCategory>(context);

        IIndex index = FindIndex(
            entityType,
            nameof(TaxCategory.Status),
            nameof(TaxCategory.EffectiveFrom),
            nameof(TaxCategory.EffectiveTo));

        Assert.False(index.IsUnique);

        Assert.Equal(
            "IX_TaxCategories_Status_EffectiveFrom_EffectiveTo",
            index.GetDatabaseName());
    }

    [Fact]
    public void TaxCategory_ConfiguresRateCheckConstraint()
    {
        using CatalogDbContext context = CreateContext();

        IEntityType entityType =
            GetEntityType<TaxCategory>(context);

        ICheckConstraint constraint =
            GetCheckConstraint(
                entityType,
                "CK_TaxCategories_Rate");

        Assert.Equal(
            "[Rate] >= 0 AND [Rate] <= 1",
            constraint.Sql);
    }

    [Fact]
    public void TaxCategory_ConfiguresTreatmentRateCheckConstraint()
    {
        using CatalogDbContext context = CreateContext();

        IEntityType entityType =
            GetEntityType<TaxCategory>(context);

        ICheckConstraint constraint =
            GetCheckConstraint(
                entityType,
                "CK_TaxCategories_TreatmentRate");

        Assert.Equal(
            "([Treatment] = 'StandardRated' AND [Rate] > 0) "
            + "OR ([Treatment] IN ('ZeroRated', 'Exempt') "
            + "AND [Rate] = 0)",
            constraint.Sql);
    }

    [Fact]
    public void TaxCategory_ConfiguresEffectivePeriodCheckConstraint()
    {
        using CatalogDbContext context = CreateContext();

        IEntityType entityType =
            GetEntityType<TaxCategory>(context);

        ICheckConstraint constraint =
            GetCheckConstraint(
                entityType,
                "CK_TaxCategories_EffectivePeriod");

        Assert.Equal(
            "[EffectiveTo] IS NULL "
            + "OR [EffectiveTo] >= [EffectiveFrom]",
            constraint.Sql);
    }

    [Fact]
    public void TaxCategory_ConfiguresExactlyThreeCheckConstraints()
    {
        using CatalogDbContext context = CreateContext();

        IEntityType entityType =
            GetEntityType<TaxCategory>(context);

        Assert.Equal(
            3,
            entityType.GetCheckConstraints().Count());
    }

    [Fact]
    public void TaxCategory_SeedsExpectedSystemCategories()
    {
        using CatalogDbContext context = CreateContext();

        IEntityType entityType =
            GetEntityType<TaxCategory>(context);

        IReadOnlyList<IDictionary<string, object?>>
            seedData =
                entityType.GetSeedData().ToList();

        Dictionary<
            string,
            (
                Guid Id,
                string Name,
                decimal Rate,
                TaxTreatment Treatment)>
            expectedCategories =
                new()
                {
                    ["GST15"] = (
                        Guid.Parse(
                            "20000000-0000-0000-0000-000000000001"),
                        "Standard GST",
                        0.1500m,
                        TaxTreatment.StandardRated),

                    ["GST0"] = (
                        Guid.Parse(
                            "20000000-0000-0000-0000-000000000002"),
                        "Zero-rated GST",
                        0m,
                        TaxTreatment.ZeroRated),

                    ["EXEMPT"] = (
                        Guid.Parse(
                            "20000000-0000-0000-0000-000000000003"),
                        "GST exempt",
                        0m,
                        TaxTreatment.Exempt)
                };

        Assert.Equal(
            expectedCategories.Count,
            seedData.Count);

        foreach (
            IDictionary<string, object?> row
            in seedData)
        {
            string code =
                Assert.IsType<string>(
                    row[nameof(TaxCategory.Code)]);

            Assert.True(
                expectedCategories.TryGetValue(
                    code,
                    out var expected));

            Assert.Equal(
                expected.Id,
                Assert.IsType<Guid>(
                    row[nameof(TaxCategory.Id)]));

            Assert.Equal(
                expected.Name,
                Assert.IsType<string>(
                    row[nameof(TaxCategory.Name)]));

            Assert.Equal(
                expected.Rate,
                Assert.IsType<decimal>(
                    row[nameof(TaxCategory.Rate)]));

            Assert.Equal(
                expected.Treatment,
                Assert.IsType<TaxTreatment>(
                    row[nameof(TaxCategory.Treatment)]));

            Assert.Equal(
                TaxCategoryStatus.Active,
                Assert.IsType<TaxCategoryStatus>(
                    row[nameof(TaxCategory.Status)]));

            Assert.Equal(
                new DateOnly(2010, 10, 1),
                Assert.IsType<DateOnly>(
                    row[nameof(TaxCategory.EffectiveFrom)]));

            Assert.Null(
                row[nameof(TaxCategory.EffectiveTo)]);
        }
    }

    [Fact]
    public void Product_ConfiguresRequiredTaxCategoryIdentifier()
    {
        using CatalogDbContext context = CreateContext();

        IEntityType productEntityType =
            GetEntityType<Product>(context);

        IProperty taxCategoryIdProperty =
            GetProperty(
                productEntityType,
                nameof(Product.TaxCategoryId));

        Assert.False(taxCategoryIdProperty.IsNullable);
    }

    [Fact]
    public void Product_ConfiguresTaxCategoryIdIndex()
    {
        using CatalogDbContext context = CreateContext();

        IEntityType productEntityType =
            GetEntityType<Product>(context);

        IIndex index = FindIndex(
            productEntityType,
            nameof(Product.TaxCategoryId));

        Assert.False(index.IsUnique);

        Assert.Equal(
            "IX_Products_TaxCategoryId",
            index.GetDatabaseName());
    }

    [Fact]
    public void Product_ConfiguresRestrictedTaxCategoryForeignKey()
    {
        using CatalogDbContext context = CreateContext();

        IEntityType productEntityType =
            GetEntityType<Product>(context);

        IForeignKey foreignKey =
            productEntityType.GetForeignKeys()
                .Single(key =>
                    key.PrincipalEntityType.ClrType ==
                    typeof(TaxCategory));

        IProperty foreignKeyProperty =
            Assert.Single(foreignKey.Properties);

        Assert.Equal(
            nameof(Product.TaxCategoryId),
            foreignKeyProperty.Name);

        Assert.False(foreignKeyProperty.IsNullable);

        Assert.True(foreignKey.IsRequired);

        Assert.Equal(
            DeleteBehavior.Restrict,
            foreignKey.DeleteBehavior);

        Assert.Equal(
            "FK_Products_TaxCategories_TaxCategoryId",
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
                + "was not found in the EF Core "
                + "design-time model.");
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

    private static IIndex FindIndex(
        IEntityType entityType,
        params string[] propertyNames)
    {
        return entityType.GetIndexes()
            .SingleOrDefault(index =>
                index.Properties
                    .Select(property => property.Name)
                    .SequenceEqual(propertyNames))
            ?? throw new InvalidOperationException(
                "Expected index was not configured.");
    }

    private static ICheckConstraint GetCheckConstraint(
        IEntityType entityType,
        string constraintName)
    {
        return entityType.GetCheckConstraints()
            .SingleOrDefault(constraint =>
                constraint.Name == constraintName)
            ?? throw new InvalidOperationException(
                $"Check constraint '{constraintName}' "
                + "was not configured.");
    }
}
