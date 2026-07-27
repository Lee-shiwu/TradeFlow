using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using TradeFlow.Modules.Catalog.Domain.Products;
using TradeFlow.Modules.Catalog.Infrastructure.Persistence;

namespace TradeFlow.UnitTests.Catalog.Persistence;

public sealed class ProductConfigurationTests
{
    [Fact]
    public void Product_MapsToExpectedTableAndSchema()
    {
        using CatalogDbContext context = CreateContext();

        IEntityType entityType =
            GetProductEntityType(context);

        Assert.Equal("Products", entityType.GetTableName());
        Assert.Equal("catalog", entityType.GetSchema());

        IKey primaryKey =
            entityType.FindPrimaryKey()
            ?? throw new InvalidOperationException(
                "Product primary key was not configured.");

        IProperty idProperty =
            Assert.Single(primaryKey.Properties);

        Assert.Equal(
            nameof(Product.Id),
            idProperty.Name);

        Assert.Equal(
            ValueGenerated.Never,
            idProperty.ValueGenerated);
    }

    [Fact]
    public void Product_ConfiguresSkuCorrectly()
    {
        using CatalogDbContext context = CreateContext();

        IEntityType entityType =
            GetProductEntityType(context);

        IProperty skuProperty =
            GetProperty(entityType, nameof(Product.Sku));

        StoreObjectIdentifier table =
            StoreObjectIdentifier.Table(
                "Products",
                "catalog");

        Assert.Equal(
            "SKU",
            skuProperty.GetColumnName(table));

        Assert.Equal(50, skuProperty.GetMaxLength());
        Assert.Equal(false, skuProperty.IsUnicode());
        Assert.False(skuProperty.IsNullable);
    }

    [Fact]
    public void Product_ConfiguresNameCorrectly()
    {
        using CatalogDbContext context = CreateContext();

        IEntityType entityType =
            GetProductEntityType(context);

        IProperty nameProperty =
            GetProperty(entityType, nameof(Product.Name));

        Assert.Equal(100, nameProperty.GetMaxLength());
        Assert.Equal(true, nameProperty.IsUnicode());
        Assert.False(nameProperty.IsNullable);
    }

    [Fact]
    public void Product_ConfiguresDescriptionCorrectly()
    {
        using CatalogDbContext context = CreateContext();

        IEntityType entityType =
            GetProductEntityType(context);

        IProperty descriptionProperty =
            GetProperty(
                entityType,
                nameof(Product.Description));

        Assert.Equal(
            1000,
            descriptionProperty.GetMaxLength());

        Assert.Equal(
            true,
            descriptionProperty.IsUnicode());

        Assert.False(descriptionProperty.IsNullable);

        Assert.Equal(
            string.Empty,
            descriptionProperty.GetDefaultValue());
    }

    [Fact]
    public void Product_ConfiguresStatusAsNonUnicodeString()
    {
        using CatalogDbContext context = CreateContext();

        IEntityType entityType =
            GetProductEntityType(context);

        IProperty statusProperty =
            GetProperty(
                entityType,
                nameof(Product.Status));

        Assert.Equal(20, statusProperty.GetMaxLength());
        Assert.Equal(false, statusProperty.IsUnicode());
        Assert.False(statusProperty.IsNullable);

        
        Assert.Equal(
            typeof(string),
            statusProperty.GetProviderClrType());
    }

    [Fact]
    public void Product_ConfiguresRequiredAndOptionalIdentifiers()
    {
        using CatalogDbContext context = CreateContext();

        IEntityType entityType =
            GetProductEntityType(context);

        Assert.False(
            GetProperty(
                entityType,
                nameof(Product.OrganisationId))
            .IsNullable);

        Assert.False(
            GetProperty(
                entityType,
                nameof(Product.UnitOfMeasureId))
            .IsNullable);

        Assert.True(
            GetProperty(
                entityType,
                nameof(Product.ProductCategoryId))
            .IsNullable);

        Assert.False(
            GetProperty(
                entityType,
                nameof(Product.TaxCategoryId))
            .IsNullable);

        Assert.False(
            GetProperty(
                entityType,
                nameof(Product.CreatedBy))
            .IsNullable);

        Assert.True(
            GetProperty(
                entityType,
                nameof(Product.LastModifiedBy))
            .IsNullable);
    }

    [Fact]
    public void Product_ConfiguresAuditTimestampsCorrectly()
    {
        using CatalogDbContext context = CreateContext();

        IEntityType entityType =
            GetProductEntityType(context);

        IProperty createdAtProperty =
            GetProperty(
                entityType,
                nameof(Product.CreatedAt));

        IProperty lastModifiedAtProperty =
            GetProperty(
                entityType,
                nameof(Product.LastModifiedAt));

        Assert.False(createdAtProperty.IsNullable);
        Assert.Equal(7, createdAtProperty.GetPrecision());

        Assert.True(lastModifiedAtProperty.IsNullable);
        Assert.Equal(
            7,
            lastModifiedAtProperty.GetPrecision());
    }

    [Fact]
    public void Product_ConfiguresRowVersionForConcurrency()
    {
        using CatalogDbContext context = CreateContext();

        IEntityType entityType =
            GetProductEntityType(context);

        IProperty rowVersionProperty =
            GetProperty(
                entityType,
                nameof(Product.RowVersion));

        Assert.True(rowVersionProperty.IsConcurrencyToken);

        Assert.Equal(
            ValueGenerated.OnAddOrUpdate,
            rowVersionProperty.ValueGenerated);
    }

    [Fact]
    public void Product_ConfiguresOrganisationAndSkuAsUniqueIndex()
    {
        using CatalogDbContext context = CreateContext();

        IEntityType entityType =
            GetProductEntityType(context);

        IIndex index = FindIndex(
            entityType,
            nameof(Product.OrganisationId),
            nameof(Product.Sku));

        Assert.True(index.IsUnique);

        Assert.Equal(
            "UX_Products_OrganisationId_Sku",
            index.GetDatabaseName());
    }

    [Fact]
    public void Product_ConfiguresOrganisationAndStatusAsNonUniqueIndex()
    {
        using CatalogDbContext context = CreateContext();

        IEntityType entityType =
            GetProductEntityType(context);

        IIndex index = FindIndex(
            entityType,
            nameof(Product.OrganisationId),
            nameof(Product.Status));

        Assert.False(index.IsUnique);

        Assert.Equal(
            "IX_Products_OrganisationId_Status",
            index.GetDatabaseName());
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

    private static IEntityType GetProductEntityType(
        CatalogDbContext context)
    {
        return context.Model.FindEntityType(typeof(Product))
            ?? throw new InvalidOperationException(
                "Product was not found in the EF Core model.");
    }

    private static IProperty GetProperty(
        IEntityType entityType,
        string propertyName)
    {
        return entityType.FindProperty(propertyName)
            ?? throw new InvalidOperationException(
                $"Property '{propertyName}' was not configured.");
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
                "Expected Product index was not configured.");
    }
}
