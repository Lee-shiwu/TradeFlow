using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using TradeFlow.Modules.Catalog.Domain.ProductCategories;
using TradeFlow.Modules.Catalog.Domain.Products;
using TradeFlow.Modules.Catalog.Infrastructure.Persistence;

namespace TradeFlow.UnitTests.Catalog.Persistence;

public sealed class ProductCategoryConfigurationTests
{
    [Fact]
    public void ProductCategory_MapsToExpectedTableAndSchema()
    {
        using CatalogDbContext context = CreateContext();

        IEntityType entityType =
            GetEntityType<ProductCategory>(context);

        Assert.Equal(
            "ProductCategories",
            entityType.GetTableName());

        Assert.Equal(
            "catalog",
            entityType.GetSchema());

        IKey primaryKey =
            entityType.FindPrimaryKey()
            ?? throw new InvalidOperationException(
                "ProductCategory primary key was not configured.");

        IProperty idProperty =
            Assert.Single(primaryKey.Properties);

        Assert.Equal(
            nameof(ProductCategory.Id),
            idProperty.Name);

        Assert.Equal(
            ValueGenerated.Never,
            idProperty.ValueGenerated);
    }

    [Fact]
    public void ProductCategory_ConfiguresOrganisationCorrectly()
    {
        using CatalogDbContext context = CreateContext();

        IEntityType entityType =
            GetEntityType<ProductCategory>(context);

        IProperty organisationProperty =
            GetProperty(
                entityType,
                nameof(ProductCategory.OrganisationId));

        Assert.False(organisationProperty.IsNullable);
        Assert.Equal(typeof(Guid), organisationProperty.ClrType);
    }

    [Fact]
    public void ProductCategory_ConfiguresCodeCorrectly()
    {
        using CatalogDbContext context = CreateContext();

        IEntityType entityType =
            GetEntityType<ProductCategory>(context);

        IProperty codeProperty =
            GetProperty(
                entityType,
                nameof(ProductCategory.Code));

        Assert.False(codeProperty.IsNullable);
        Assert.Equal(50, codeProperty.GetMaxLength());
        Assert.Equal(false, codeProperty.IsUnicode());
    }

    [Fact]
    public void ProductCategory_ConfiguresNameCorrectly()
    {
        using CatalogDbContext context = CreateContext();

        IEntityType entityType =
            GetEntityType<ProductCategory>(context);

        IProperty nameProperty =
            GetProperty(
                entityType,
                nameof(ProductCategory.Name));

        Assert.False(nameProperty.IsNullable);
        Assert.Equal(100, nameProperty.GetMaxLength());
        Assert.Equal(true, nameProperty.IsUnicode());
    }

    [Fact]
    public void ProductCategory_ConfiguresDescriptionCorrectly()
    {
        using CatalogDbContext context = CreateContext();

        IEntityType entityType =
            GetEntityType<ProductCategory>(context);

        IProperty descriptionProperty =
            GetProperty(
                entityType,
                nameof(ProductCategory.Description));

        Assert.False(descriptionProperty.IsNullable);

        Assert.Equal(
            1000,
            descriptionProperty.GetMaxLength());

        Assert.Equal(
            true,
            descriptionProperty.IsUnicode());

        Assert.Equal(
            string.Empty,
            descriptionProperty.GetDefaultValue());
    }

    [Fact]
    public void ProductCategory_ConfiguresStatusAsNonUnicodeString()
    {
        using CatalogDbContext context = CreateContext();

        IEntityType entityType =
            GetEntityType<ProductCategory>(context);

        IProperty statusProperty =
            GetProperty(
                entityType,
                nameof(ProductCategory.Status));

        Assert.False(statusProperty.IsNullable);
        Assert.Equal(20, statusProperty.GetMaxLength());
        Assert.Equal(false, statusProperty.IsUnicode());

        Assert.Equal(
            typeof(string),
            statusProperty.GetProviderClrType());
    }

    [Fact]
    public void ProductCategory_ConfiguresAuditFieldsCorrectly()
    {
        using CatalogDbContext context = CreateContext();

        IEntityType entityType =
            GetEntityType<ProductCategory>(context);

        IProperty createdAtProperty =
            GetProperty(
                entityType,
                nameof(ProductCategory.CreatedAt));

        IProperty createdByProperty =
            GetProperty(
                entityType,
                nameof(ProductCategory.CreatedBy));

        IProperty lastModifiedAtProperty =
            GetProperty(
                entityType,
                nameof(ProductCategory.LastModifiedAt));

        IProperty lastModifiedByProperty =
            GetProperty(
                entityType,
                nameof(ProductCategory.LastModifiedBy));

        Assert.False(createdAtProperty.IsNullable);
        Assert.Equal(7, createdAtProperty.GetPrecision());

        Assert.False(createdByProperty.IsNullable);

        Assert.True(lastModifiedAtProperty.IsNullable);
        Assert.Equal(
            7,
            lastModifiedAtProperty.GetPrecision());

        Assert.True(lastModifiedByProperty.IsNullable);
    }

    [Fact]
    public void ProductCategory_ConfiguresRowVersionForConcurrency()
    {
        using CatalogDbContext context = CreateContext();

        IEntityType entityType =
            GetEntityType<ProductCategory>(context);

        IProperty rowVersionProperty =
            GetProperty(
                entityType,
                nameof(ProductCategory.RowVersion));

        Assert.True(rowVersionProperty.IsConcurrencyToken);

        Assert.Equal(
            ValueGenerated.OnAddOrUpdate,
            rowVersionProperty.ValueGenerated);
    }

    [Fact]
    public void ProductCategory_ConfiguresOrganisationAndCodeAsUniqueIndex()
    {
        using CatalogDbContext context = CreateContext();

        IEntityType entityType =
            GetEntityType<ProductCategory>(context);

        IIndex index = FindIndex(
            entityType,
            nameof(ProductCategory.OrganisationId),
            nameof(ProductCategory.Code));

        Assert.True(index.IsUnique);

        Assert.Equal(
            "UX_ProductCategories_OrganisationId_Code",
            index.GetDatabaseName());
    }

    [Fact]
    public void ProductCategory_ConfiguresOrganisationAndStatusAsNonUniqueIndex()
    {
        using CatalogDbContext context = CreateContext();

        IEntityType entityType =
            GetEntityType<ProductCategory>(context);

        IIndex index = FindIndex(
            entityType,
            nameof(ProductCategory.OrganisationId),
            nameof(ProductCategory.Status));

        Assert.False(index.IsUnique);

        Assert.Equal(
            "IX_ProductCategories_OrganisationId_Status",
            index.GetDatabaseName());
    }

    [Fact]
    public void ProductCategory_DoesNotConfigureSeedData()
    {
        using CatalogDbContext context = CreateContext();

        IEntityType entityType =
            GetEntityType<ProductCategory>(context);

        Assert.Empty(entityType.GetSeedData());
    }

    [Fact]
    public void Product_ConfiguresOptionalRestrictedProductCategoryForeignKey()
    {
        using CatalogDbContext context = CreateContext();

        IEntityType productEntityType =
            GetEntityType<Product>(context);

        IForeignKey foreignKey =
            productEntityType.GetForeignKeys()
                .Single(key =>
                    key.PrincipalEntityType.ClrType ==
                    typeof(ProductCategory));

        IProperty foreignKeyProperty =
            Assert.Single(foreignKey.Properties);

        Assert.Equal(
            nameof(Product.ProductCategoryId),
            foreignKeyProperty.Name);

        Assert.True(foreignKeyProperty.IsNullable);

        Assert.Equal(
            DeleteBehavior.Restrict,
            foreignKey.DeleteBehavior);

        Assert.Equal(
            "FK_Products_ProductCategories_ProductCategoryId",
            foreignKey.GetConstraintName());
    }

    [Fact]
    public void Product_ConfiguresUnitOfMeasureIdIndex()
    {
        using CatalogDbContext context = CreateContext();

        IEntityType productEntityType =
            GetEntityType<Product>(context);

        IIndex index = FindIndex(
            productEntityType,
            nameof(Product.UnitOfMeasureId));

        Assert.False(index.IsUnique);

        Assert.Equal(
            "IX_Products_UnitOfMeasureId",
            index.GetDatabaseName());
    }

    [Fact]
    public void Product_ConfiguresProductCategoryIdIndex()
    {
        using CatalogDbContext context = CreateContext();

        IEntityType productEntityType =
            GetEntityType<Product>(context);

        IIndex index = FindIndex(
            productEntityType,
            nameof(Product.ProductCategoryId));

        Assert.False(index.IsUnique);

        Assert.Equal(
            "IX_Products_ProductCategoryId",
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
}
