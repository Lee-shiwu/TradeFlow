using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TradeFlow.BuildingBlocks.Exceptions;
using TradeFlow.BuildingBlocks.Time;
using TradeFlow.Modules.Catalog.Application.Products.UpdateProductDetails;
using TradeFlow.Modules.Catalog.Domain.ProductCategories;
using TradeFlow.Modules.Catalog.Domain.Products;
using TradeFlow.Modules.Catalog.Domain.TaxCategories;
using TradeFlow.Modules.Catalog.Infrastructure.Persistence;

namespace TradeFlow.IntegrationTests.Catalog.Products;

public sealed class UpdateProductDetailsHandlerTests(
    ApiFactory factory)
    : IClassFixture<ApiFactory>
{
    private static readonly Guid EachUnitOfMeasureId =
        Guid.Parse(
            "10000000-0000-0000-0000-000000000001");

    private static readonly Guid StandardGstTaxCategoryId =
        Guid.Parse(
            "20000000-0000-0000-0000-000000000001");

    private static readonly DateTimeOffset FixedUtcNow =
        new(
            year: 2026,
            month: 8,
            day: 18,
            hour: 10,
            minute: 30,
            second: 0,
            offset: TimeSpan.Zero);

    [Fact]
    public async Task HandleAsync_WithValidCommand_UpdatesProduct()
    {
        using IServiceScope scope =
            factory.Services.CreateScope();

        CatalogDbContext dbContext =
            scope.ServiceProvider
                .GetRequiredService<CatalogDbContext>();

        await dbContext.Database.MigrateAsync();

        Guid organisationId = Guid.NewGuid();
        Guid createdBy = Guid.NewGuid();
        Guid modifiedBy = Guid.NewGuid();

        ProductCategory category =
            CreateProductCategory(
                organisationId,
                createdBy);

        TaxCategory taxCategory =
            CreateTaxCategory(
                effectiveFrom:
                    DateOnly.FromDateTime(
                        FixedUtcNow.UtcDateTime).AddDays(-10),
                effectiveTo: null);

        Product product =
            CreateProduct(
                organisationId: organisationId,
                productCategoryId: null,
                taxCategoryId: StandardGstTaxCategoryId,
                createdBy: createdBy);

        dbContext.ProductCategories.Add(category);
        dbContext.TaxCategories.Add(taxCategory);
        dbContext.Products.Add(product);

        await dbContext.SaveChangesAsync();

        byte[] originalRowVersion =
            product.RowVersion.ToArray();

        try
        {
            dbContext.ChangeTracker.Clear();

            UpdateProductDetailsHandler handler =
                new(
                    dbContext,
                    new TestClock(FixedUtcNow));

            UpdateProductDetailsCommand command =
                new(
                    OrganisationId: organisationId,
                    ProductId: product.Id,
                    Name: "  Updated Office Chair  ",
                    Description: "  Updated description.  ",
                    ProductCategoryId: category.Id,
                    TaxCategoryId: taxCategory.Id,
                    ModifiedBy: modifiedBy,
                    RowVersion: originalRowVersion);

            UpdateProductDetailsResult result =
                await handler.HandleAsync(
                    command,
                    CancellationToken.None);

            Assert.Equal(product.Id, result.ProductId);
            Assert.Equal("Updated Office Chair", result.Name);
            Assert.Equal(
                "Updated description.",
                result.Description);

            Assert.Equal(
                category.Id,
                result.ProductCategoryId);

            Assert.Equal(
                taxCategory.Id,
                result.TaxCategoryId);

            Assert.Equal(
                FixedUtcNow,
                result.LastModifiedAt);

            Assert.Equal(
                modifiedBy,
                result.LastModifiedBy);

            Assert.NotEmpty(result.RowVersion);

            Assert.False(
                originalRowVersion.SequenceEqual(
                    result.RowVersion));

            dbContext.ChangeTracker.Clear();

            Product? savedProduct =
                await dbContext.Products
                    .AsNoTracking()
                    .SingleOrDefaultAsync(
                        existingProduct =>
                            existingProduct.Id == product.Id);

            Assert.NotNull(savedProduct);

            Assert.Equal(
                "Updated Office Chair",
                savedProduct.Name);

            Assert.Equal(
                "Updated description.",
                savedProduct.Description);

            Assert.Equal(
                category.Id,
                savedProduct.ProductCategoryId);

            Assert.Equal(
                taxCategory.Id,
                savedProduct.TaxCategoryId);

            Assert.Equal(
                FixedUtcNow,
                savedProduct.LastModifiedAt);

            Assert.Equal(
                modifiedBy,
                savedProduct.LastModifiedBy);

            Assert.Equal(
                result.RowVersion,
                savedProduct.RowVersion);
        }
        finally
        {
            await DeleteTestDataAsync(
                dbContext,
                productId: product.Id,
                productCategoryId: category.Id,
                taxCategoryId: taxCategory.Id);
        }
    }

    [Fact]
    public async Task HandleAsync_WithNullProductCategory_ClearsCategory()
    {
        using IServiceScope scope =
            factory.Services.CreateScope();

        CatalogDbContext dbContext =
            scope.ServiceProvider
                .GetRequiredService<CatalogDbContext>();

        await dbContext.Database.MigrateAsync();

        Guid organisationId = Guid.NewGuid();
        Guid userId = Guid.NewGuid();

        ProductCategory category =
            CreateProductCategory(
                organisationId,
                userId);

        Product product =
            CreateProduct(
                organisationId: organisationId,
                productCategoryId: category.Id,
                taxCategoryId: StandardGstTaxCategoryId,
                createdBy: userId);

        dbContext.ProductCategories.Add(category);
        dbContext.Products.Add(product);

        await dbContext.SaveChangesAsync();

        byte[] rowVersion =
            product.RowVersion.ToArray();

        try
        {
            dbContext.ChangeTracker.Clear();

            UpdateProductDetailsHandler handler =
                new(
                    dbContext,
                    new TestClock(FixedUtcNow));

            UpdateProductDetailsCommand command =
                new(
                    OrganisationId: organisationId,
                    ProductId: product.Id,
                    Name: product.Name,
                    Description: product.Description,
                    ProductCategoryId: null,
                    TaxCategoryId:
                        StandardGstTaxCategoryId,
                    ModifiedBy: userId,
                    RowVersion: rowVersion);

            UpdateProductDetailsResult result =
                await handler.HandleAsync(
                    command,
                    CancellationToken.None);

            Assert.Null(result.ProductCategoryId);

            dbContext.ChangeTracker.Clear();

            Product? savedProduct =
                await dbContext.Products
                    .AsNoTracking()
                    .SingleOrDefaultAsync(
                        existingProduct =>
                            existingProduct.Id == product.Id);

            Assert.NotNull(savedProduct);
            Assert.Null(savedProduct.ProductCategoryId);
        }
        finally
        {
            await DeleteTestDataAsync(
                dbContext,
                productId: product.Id,
                productCategoryId: category.Id);
        }
    }

    [Fact]
    public async Task HandleAsync_WithProductFromAnotherOrganisation_ThrowsNotFoundException()
    {
        using IServiceScope scope =
            factory.Services.CreateScope();

        CatalogDbContext dbContext =
            scope.ServiceProvider
                .GetRequiredService<CatalogDbContext>();

        await dbContext.Database.MigrateAsync();

        Guid owningOrganisationId = Guid.NewGuid();
        Guid otherOrganisationId = Guid.NewGuid();

        Product product =
            CreateProduct(
                organisationId: owningOrganisationId,
                productCategoryId: null,
                taxCategoryId: StandardGstTaxCategoryId,
                createdBy: Guid.NewGuid());

        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync();

        byte[] rowVersion =
            product.RowVersion.ToArray();

        try
        {
            dbContext.ChangeTracker.Clear();

            UpdateProductDetailsHandler handler =
                new(
                    dbContext,
                    new TestClock(FixedUtcNow));

            UpdateProductDetailsCommand command =
                new(
                    OrganisationId: otherOrganisationId,
                    ProductId: product.Id,
                    Name: "Updated Name",
                    Description: string.Empty,
                    ProductCategoryId: null,
                    TaxCategoryId:
                        StandardGstTaxCategoryId,
                    ModifiedBy: Guid.NewGuid(),
                    RowVersion: rowVersion);

            NotFoundException exception =
                await Assert.ThrowsAsync<NotFoundException>(
                    () => handler.HandleAsync(
                        command,
                        CancellationToken.None));

            Assert.Equal(
                "UPDATE_PRODUCT_NOT_FOUND",
                exception.Code);

            Assert.Equal(
                "The selected product does not exist.",
                exception.Message);
        }
        finally
        {
            await DeleteTestDataAsync(
                dbContext,
                productId: product.Id);
        }
    }

    [Fact]
    public async Task HandleAsync_WithCategoryFromAnotherOrganisation_ThrowsBusinessRuleException()
    {
        using IServiceScope scope =
            factory.Services.CreateScope();

        CatalogDbContext dbContext =
            scope.ServiceProvider
                .GetRequiredService<CatalogDbContext>();

        await dbContext.Database.MigrateAsync();

        Guid productOrganisationId = Guid.NewGuid();
        Guid categoryOrganisationId = Guid.NewGuid();
        Guid userId = Guid.NewGuid();

        ProductCategory category =
            CreateProductCategory(
                categoryOrganisationId,
                userId);

        Product product =
            CreateProduct(
                organisationId: productOrganisationId,
                productCategoryId: null,
                taxCategoryId: StandardGstTaxCategoryId,
                createdBy: userId);

        dbContext.ProductCategories.Add(category);
        dbContext.Products.Add(product);

        await dbContext.SaveChangesAsync();

        byte[] rowVersion =
            product.RowVersion.ToArray();

        try
        {
            dbContext.ChangeTracker.Clear();

            UpdateProductDetailsHandler handler =
                new(
                    dbContext,
                    new TestClock(FixedUtcNow));

            UpdateProductDetailsCommand command =
                new(
                    OrganisationId: productOrganisationId,
                    ProductId: product.Id,
                    Name: "Updated Name",
                    Description: string.Empty,
                    ProductCategoryId: category.Id,
                    TaxCategoryId:
                        StandardGstTaxCategoryId,
                    ModifiedBy: userId,
                    RowVersion: rowVersion);

            BusinessRuleException exception =
                await Assert.ThrowsAsync<BusinessRuleException>(
                    () => handler.HandleAsync(
                        command,
                        CancellationToken.None));

            Assert.Equal(
                "UPDATE_PRODUCT_CATEGORY_NOT_FOUND",
                exception.Code);

            Assert.Equal(
                "The selected product category does not exist.",
                exception.Message);
        }
        finally
        {
            await DeleteTestDataAsync(
                dbContext,
                productId: product.Id,
                productCategoryId: category.Id);
        }
    }

    [Fact]
    public async Task HandleAsync_WithInactiveProductCategory_ThrowsBusinessRuleException()
    {
        using IServiceScope scope =
            factory.Services.CreateScope();

        CatalogDbContext dbContext =
            scope.ServiceProvider
                .GetRequiredService<CatalogDbContext>();

        await dbContext.Database.MigrateAsync();

        Guid organisationId = Guid.NewGuid();
        Guid userId = Guid.NewGuid();

        ProductCategory category =
            CreateProductCategory(
                organisationId,
                userId);

        category.Deactivate(
            modifiedAt: FixedUtcNow.AddMinutes(-1),
            modifiedBy: userId);

        Product product =
            CreateProduct(
                organisationId: organisationId,
                productCategoryId: null,
                taxCategoryId: StandardGstTaxCategoryId,
                createdBy: userId);

        dbContext.ProductCategories.Add(category);
        dbContext.Products.Add(product);

        await dbContext.SaveChangesAsync();

        byte[] rowVersion =
            product.RowVersion.ToArray();

        try
        {
            dbContext.ChangeTracker.Clear();

            UpdateProductDetailsHandler handler =
                new(
                    dbContext,
                    new TestClock(FixedUtcNow));

            UpdateProductDetailsCommand command =
                new(
                    OrganisationId: organisationId,
                    ProductId: product.Id,
                    Name: "Updated Name",
                    Description: string.Empty,
                    ProductCategoryId: category.Id,
                    TaxCategoryId:
                        StandardGstTaxCategoryId,
                    ModifiedBy: userId,
                    RowVersion: rowVersion);

            BusinessRuleException exception =
                await Assert.ThrowsAsync<BusinessRuleException>(
                    () => handler.HandleAsync(
                        command,
                        CancellationToken.None));

            Assert.Equal(
                "UPDATE_PRODUCT_CATEGORY_INACTIVE",
                exception.Code);

            Assert.Equal(
                "The selected product category is inactive.",
                exception.Message);
        }
        finally
        {
            await DeleteTestDataAsync(
                dbContext,
                productId: product.Id,
                productCategoryId: category.Id);
        }
    }

    [Fact]
    public async Task HandleAsync_WithUnknownTaxCategory_ThrowsBusinessRuleException()
    {
        using IServiceScope scope =
            factory.Services.CreateScope();

        CatalogDbContext dbContext =
            scope.ServiceProvider
                .GetRequiredService<CatalogDbContext>();

        await dbContext.Database.MigrateAsync();

        Guid organisationId = Guid.NewGuid();

        Product product =
            CreateProduct(
                organisationId: organisationId,
                productCategoryId: null,
                taxCategoryId: StandardGstTaxCategoryId,
                createdBy: Guid.NewGuid());

        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync();

        byte[] rowVersion =
            product.RowVersion.ToArray();

        try
        {
            dbContext.ChangeTracker.Clear();

            UpdateProductDetailsHandler handler =
                new(
                    dbContext,
                    new TestClock(FixedUtcNow));

            UpdateProductDetailsCommand command =
                new(
                    OrganisationId: organisationId,
                    ProductId: product.Id,
                    Name: "Updated Name",
                    Description: string.Empty,
                    ProductCategoryId: null,
                    TaxCategoryId: Guid.NewGuid(),
                    ModifiedBy: Guid.NewGuid(),
                    RowVersion: rowVersion);

            BusinessRuleException exception =
                await Assert.ThrowsAsync<BusinessRuleException>(
                    () => handler.HandleAsync(
                        command,
                        CancellationToken.None));

            Assert.Equal(
                "UPDATE_PRODUCT_TAX_CATEGORY_NOT_FOUND",
                exception.Code);

            Assert.Equal(
                "The selected tax category does not exist.",
                exception.Message);
        }
        finally
        {
            await DeleteTestDataAsync(
                dbContext,
                productId: product.Id);
        }
    }

    [Fact]
    public async Task HandleAsync_WithInactiveTaxCategory_ThrowsBusinessRuleException()
    {
        using IServiceScope scope =
            factory.Services.CreateScope();

        CatalogDbContext dbContext =
            scope.ServiceProvider
                .GetRequiredService<CatalogDbContext>();

        await dbContext.Database.MigrateAsync();

        Guid organisationId = Guid.NewGuid();

        TaxCategory taxCategory =
            CreateTaxCategory(
                effectiveFrom:
                    DateOnly.FromDateTime(
                        FixedUtcNow.UtcDateTime).AddDays(-1),
                effectiveTo: null);

        Product product =
            CreateProduct(
                organisationId: organisationId,
                productCategoryId: null,
                taxCategoryId: StandardGstTaxCategoryId,
                createdBy: Guid.NewGuid());

        dbContext.TaxCategories.Add(taxCategory);
        dbContext.Products.Add(product);

        await dbContext.SaveChangesAsync();

        await dbContext.TaxCategories
            .Where(existingTaxCategory =>
                existingTaxCategory.Id == taxCategory.Id)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(
                    existingTaxCategory =>
                        existingTaxCategory.Status,
                    TaxCategoryStatus.Inactive));

        byte[] rowVersion =
            product.RowVersion.ToArray();

        try
        {
            dbContext.ChangeTracker.Clear();

            UpdateProductDetailsHandler handler =
                new(
                    dbContext,
                    new TestClock(FixedUtcNow));

            UpdateProductDetailsCommand command =
                new(
                    OrganisationId: organisationId,
                    ProductId: product.Id,
                    Name: "Updated Name",
                    Description: string.Empty,
                    ProductCategoryId: null,
                    TaxCategoryId: taxCategory.Id,
                    ModifiedBy: Guid.NewGuid(),
                    RowVersion: rowVersion);

            BusinessRuleException exception =
                await Assert.ThrowsAsync<BusinessRuleException>(
                    () => handler.HandleAsync(
                        command,
                        CancellationToken.None));

            Assert.Equal(
                "UPDATE_PRODUCT_TAX_CATEGORY_INACTIVE",
                exception.Code);

            Assert.Equal(
                "The selected tax category is inactive.",
                exception.Message);
        }
        finally
        {
            await DeleteTestDataAsync(
                dbContext,
                productId: product.Id,
                taxCategoryId: taxCategory.Id);
        }
    }

    [Theory]
    [InlineData(1, 10)]
    [InlineData(-10, -1)]
    public async Task HandleAsync_WithTaxCategoryOutsideEffectivePeriod_ThrowsBusinessRuleException(
        int effectiveFromOffsetDays,
        int effectiveToOffsetDays)
    {
        using IServiceScope scope =
            factory.Services.CreateScope();

        CatalogDbContext dbContext =
            scope.ServiceProvider
                .GetRequiredService<CatalogDbContext>();

        await dbContext.Database.MigrateAsync();

        DateOnly currentDate =
            DateOnly.FromDateTime(
                FixedUtcNow.UtcDateTime);

        TaxCategory taxCategory =
            CreateTaxCategory(
                effectiveFrom:
                    currentDate.AddDays(
                        effectiveFromOffsetDays),
                effectiveTo:
                    currentDate.AddDays(
                        effectiveToOffsetDays));

        Guid organisationId = Guid.NewGuid();

        Product product =
            CreateProduct(
                organisationId: organisationId,
                productCategoryId: null,
                taxCategoryId: StandardGstTaxCategoryId,
                createdBy: Guid.NewGuid());

        dbContext.TaxCategories.Add(taxCategory);
        dbContext.Products.Add(product);

        await dbContext.SaveChangesAsync();

        byte[] rowVersion =
            product.RowVersion.ToArray();

        try
        {
            dbContext.ChangeTracker.Clear();

            UpdateProductDetailsHandler handler =
                new(
                    dbContext,
                    new TestClock(FixedUtcNow));

            UpdateProductDetailsCommand command =
                new(
                    OrganisationId: organisationId,
                    ProductId: product.Id,
                    Name: "Updated Name",
                    Description: string.Empty,
                    ProductCategoryId: null,
                    TaxCategoryId: taxCategory.Id,
                    ModifiedBy: Guid.NewGuid(),
                    RowVersion: rowVersion);

            BusinessRuleException exception =
                await Assert.ThrowsAsync<BusinessRuleException>(
                    () => handler.HandleAsync(
                        command,
                        CancellationToken.None));

            Assert.Equal(
                "UPDATE_PRODUCT_TAX_CATEGORY_NOT_EFFECTIVE",
                exception.Code);

            Assert.Equal(
                "The selected tax category is not effective on the current date.",
                exception.Message);
        }
        finally
        {
            await DeleteTestDataAsync(
                dbContext,
                productId: product.Id,
                taxCategoryId: taxCategory.Id);
        }
    }

    [Fact]
    public async Task HandleAsync_WithStaleRowVersion_ThrowsConcurrencyConflictAndDoesNotOverwriteProduct()
    {
        using IServiceScope scope =
            factory.Services.CreateScope();

        CatalogDbContext dbContext =
            scope.ServiceProvider
                .GetRequiredService<CatalogDbContext>();

        await dbContext.Database.MigrateAsync();

        Guid organisationId = Guid.NewGuid();

        Product product =
            CreateProduct(
                organisationId: organisationId,
                productCategoryId: null,
                taxCategoryId: StandardGstTaxCategoryId,
                createdBy: Guid.NewGuid());

        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync();

        byte[] staleRowVersion =
            product.RowVersion.ToArray();

        try
        {
            dbContext.ChangeTracker.Clear();

            await dbContext.Products
                .Where(existingProduct =>
                    existingProduct.Id == product.Id)
                .ExecuteUpdateAsync(
                    setters => setters.SetProperty(
                        existingProduct =>
                            existingProduct.Name,
                        "Changed By Another User"));

            dbContext.ChangeTracker.Clear();

            UpdateProductDetailsHandler handler =
                new(
                    dbContext,
                    new TestClock(FixedUtcNow));

            UpdateProductDetailsCommand command =
                new(
                    OrganisationId: organisationId,
                    ProductId: product.Id,
                    Name: "My Stale Update",
                    Description: "Should not be saved.",
                    ProductCategoryId: null,
                    TaxCategoryId:
                        StandardGstTaxCategoryId,
                    ModifiedBy: Guid.NewGuid(),
                    RowVersion: staleRowVersion);

            BusinessRuleException exception =
                await Assert.ThrowsAsync<BusinessRuleException>(
                    () => handler.HandleAsync(
                        command,
                        CancellationToken.None));

            Assert.Equal(
                "UPDATE_PRODUCT_CONCURRENCY_CONFLICT",
                exception.Code);

            Assert.Equal(
                "The product was modified by another user. Reload the product and try again.",
                exception.Message);

            dbContext.ChangeTracker.Clear();

            Product? savedProduct =
                await dbContext.Products
                    .AsNoTracking()
                    .SingleOrDefaultAsync(
                        existingProduct =>
                            existingProduct.Id == product.Id);

            Assert.NotNull(savedProduct);

            Assert.Equal(
                "Changed By Another User",
                savedProduct.Name);

            Assert.NotEqual(
                "My Stale Update",
                savedProduct.Name);
        }
        finally
        {
            await DeleteTestDataAsync(
                dbContext,
                productId: product.Id);
        }
    }

    [Fact]
    public async Task HandleAsync_WithNoActualChanges_DoesNotChangeAuditOrRowVersion()
    {
        using IServiceScope scope =
            factory.Services.CreateScope();

        CatalogDbContext dbContext =
            scope.ServiceProvider
                .GetRequiredService<CatalogDbContext>();

        await dbContext.Database.MigrateAsync();

        Guid organisationId = Guid.NewGuid();

        Product product =
            CreateProduct(
                organisationId: organisationId,
                productCategoryId: null,
                taxCategoryId: StandardGstTaxCategoryId,
                createdBy: Guid.NewGuid());

        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync();

        byte[] originalRowVersion =
            product.RowVersion.ToArray();

        try
        {
            dbContext.ChangeTracker.Clear();

            UpdateProductDetailsHandler handler =
                new(
                    dbContext,
                    new TestClock(FixedUtcNow));

            UpdateProductDetailsCommand command =
                new(
                    OrganisationId: organisationId,
                    ProductId: product.Id,
                    Name: product.Name,
                    Description: product.Description,
                    ProductCategoryId:
                        product.ProductCategoryId,
                    TaxCategoryId:
                        product.TaxCategoryId,
                    ModifiedBy: Guid.NewGuid(),
                    RowVersion: originalRowVersion);

            UpdateProductDetailsResult result =
                await handler.HandleAsync(
                    command,
                    CancellationToken.None);

            Assert.Null(result.LastModifiedAt);
            Assert.Null(result.LastModifiedBy);

            Assert.True(
                originalRowVersion.SequenceEqual(
                    result.RowVersion));

            dbContext.ChangeTracker.Clear();

            Product? savedProduct =
                await dbContext.Products
                    .AsNoTracking()
                    .SingleOrDefaultAsync(
                        existingProduct =>
                            existingProduct.Id == product.Id);

            Assert.NotNull(savedProduct);
            Assert.Null(savedProduct.LastModifiedAt);
            Assert.Null(savedProduct.LastModifiedBy);

            Assert.True(
                originalRowVersion.SequenceEqual(
                    savedProduct.RowVersion));
        }
        finally
        {
            await DeleteTestDataAsync(
                dbContext,
                productId: product.Id);
        }
    }

    private static Product CreateProduct(
        Guid organisationId,
        Guid? productCategoryId,
        Guid taxCategoryId,
        Guid createdBy)
    {
        return Product.Create(
            organisationId: organisationId,
            sku: $"PRODUCT-{Guid.NewGuid():N}",
            name: "Office Chair",
            description: "Ergonomic office chair.",
            unitOfMeasureId: EachUnitOfMeasureId,
            productCategoryId: productCategoryId,
            taxCategoryId: taxCategoryId,
            createdAt: FixedUtcNow.AddDays(-1),
            createdBy: createdBy);
    }

    private static ProductCategory CreateProductCategory(
        Guid organisationId,
        Guid createdBy)
    {
        return ProductCategory.Create(
            organisationId: organisationId,
            code: $"CATEGORY-{Guid.NewGuid():N}",
            name: "Office Furniture",
            description: "Office furniture category.",
            createdAt: FixedUtcNow.AddDays(-1),
            createdBy: createdBy);
    }

    private static TaxCategory CreateTaxCategory(
        DateOnly effectiveFrom,
        DateOnly? effectiveTo)
    {
        string code =
            $"GST-{Guid.NewGuid().ToString("N")[..20]}";

        return TaxCategory.Create(
            code: code,
            name: "Test GST Category",
            description: "Tax category used by integration tests.",
            rate: 0.15m,
            treatment: TaxTreatment.StandardRated,
            effectiveFrom: effectiveFrom,
            effectiveTo: effectiveTo);
    }

    private static async Task DeleteTestDataAsync(
        CatalogDbContext dbContext,
        Guid productId,
        Guid? productCategoryId = null,
        Guid? taxCategoryId = null)
    {
        dbContext.ChangeTracker.Clear();

        await dbContext.Products
            .Where(product =>
                product.Id == productId)
            .ExecuteDeleteAsync();

        if (productCategoryId.HasValue)
        {
            await dbContext.ProductCategories
                .Where(category =>
                    category.Id ==
                        productCategoryId.Value)
                .ExecuteDeleteAsync();
        }

        if (taxCategoryId.HasValue)
        {
            await dbContext.TaxCategories
                .Where(taxCategory =>
                    taxCategory.Id ==
                        taxCategoryId.Value)
                .ExecuteDeleteAsync();
        }
    }

    private sealed class TestClock(
        DateTimeOffset utcNow)
        : IClock
    {
        public DateTimeOffset UtcNow { get; } =
            utcNow;
    }
}
