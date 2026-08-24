using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TradeFlow.BuildingBlocks.Exceptions;
using TradeFlow.BuildingBlocks.Time;
using TradeFlow.Modules.Catalog.Application.Products.ActivateProduct;
using TradeFlow.Modules.Catalog.Domain.ProductCategories;
using TradeFlow.Modules.Catalog.Domain.Products;
using TradeFlow.Modules.Catalog.Domain.TaxCategories;
using TradeFlow.Modules.Catalog.Domain.UnitsOfMeasure;
using TradeFlow.Modules.Catalog.Infrastructure.Persistence;

namespace TradeFlow.IntegrationTests.Catalog.Products;

[CollectionDefinition(
    CollectionName,
    DisableParallelization = true)]
public sealed class ActivateProductDatabaseCollection
{
    public const string CollectionName =
        "Activate Product database tests";
}

[Collection(
    ActivateProductDatabaseCollection.CollectionName)]
public sealed class ActivateProductHandlerTests(
    ApiFactory factory)
    : IClassFixture<ApiFactory>
{
    private static readonly Guid EachUnitOfMeasureId =
        Guid.Parse(
            "10000000-0000-0000-0000-000000000001");

    private static readonly Guid LitreUnitOfMeasureId =
        Guid.Parse(
            "10000000-0000-0000-0000-000000000003");

    private static readonly Guid StandardGstTaxCategoryId =
        Guid.Parse(
            "20000000-0000-0000-0000-000000000001");

    private static readonly DateTimeOffset FixedUtcNow =
        new(
            year: 2026,
            month: 8,
            day: 25,
            hour: 10,
            minute: 30,
            second: 0,
            offset: TimeSpan.Zero);

    [Fact]
    public async Task HandleAsync_WithValidCommand_ActivatesProduct()
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

        Product product =
            CreateInactiveProduct(
                organisationId: organisationId,
                createdBy: createdBy,
                productCategoryId: category.Id);

        dbContext.ProductCategories.Add(category);
        dbContext.Products.Add(product);

        await dbContext.SaveChangesAsync();

        byte[] originalRowVersion =
            product.RowVersion.ToArray();

        try
        {
            dbContext.ChangeTracker.Clear();

            ActivateProductHandler handler =
                CreateHandler(dbContext);

            ActivateProductCommand command =
                CreateCommand(
                    organisationId,
                    product.Id,
                    modifiedBy,
                    originalRowVersion);

            ActivateProductResult result =
                await handler.HandleAsync(
                    command,
                    CancellationToken.None);

            Assert.Equal(product.Id, result.ProductId);
            Assert.Equal(
                ProductStatus.Active,
                result.Status);

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
                            existingProduct.Id ==
                                product.Id);

            Assert.NotNull(savedProduct);

            Assert.Equal(
                ProductStatus.Active,
                savedProduct.Status);

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
                productCategoryId: category.Id);
        }
    }

    [Fact]
    public async Task HandleAsync_WithNullProductCategory_ActivatesProduct()
    {
        using IServiceScope scope =
            factory.Services.CreateScope();

        CatalogDbContext dbContext =
            scope.ServiceProvider
                .GetRequiredService<CatalogDbContext>();

        await dbContext.Database.MigrateAsync();

        Guid organisationId = Guid.NewGuid();

        Product product =
            CreateInactiveProduct(
                organisationId,
                Guid.NewGuid(),
                productCategoryId: null);

        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync();

        byte[] rowVersion =
            product.RowVersion.ToArray();

        try
        {
            dbContext.ChangeTracker.Clear();

            ActivateProductHandler handler =
                CreateHandler(dbContext);

            ActivateProductCommand command =
                CreateCommand(
                    organisationId,
                    product.Id,
                    Guid.NewGuid(),
                    rowVersion);

            ActivateProductResult result =
                await handler.HandleAsync(
                    command,
                    CancellationToken.None);

            Assert.Equal(
                ProductStatus.Active,
                result.Status);

            dbContext.ChangeTracker.Clear();

            Product? savedProduct =
                await dbContext.Products
                    .AsNoTracking()
                    .SingleOrDefaultAsync(
                        existingProduct =>
                            existingProduct.Id ==
                                product.Id);

            Assert.NotNull(savedProduct);
            Assert.Null(savedProduct.ProductCategoryId);

            Assert.Equal(
                ProductStatus.Active,
                savedProduct.Status);
        }
        finally
        {
            await DeleteTestDataAsync(
                dbContext,
                product.Id);
        }
    }

    [Fact]
    public async Task HandleAsync_WithAlreadyActiveProduct_DoesNotChangeAuditOrRowVersion()
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
                    CurrentDate.AddDays(-10),
                effectiveTo: null);

        Product product =
            CreateActiveProduct(
                organisationId: organisationId,
                createdBy: Guid.NewGuid(),
                productCategoryId: null,
                taxCategoryId: taxCategory.Id);

        dbContext.TaxCategories.Add(taxCategory);
        dbContext.Products.Add(product);

        await dbContext.SaveChangesAsync();

        byte[] originalRowVersion =
            product.RowVersion.ToArray();

        await dbContext.TaxCategories
            .Where(existingTaxCategory =>
                existingTaxCategory.Id ==
                    taxCategory.Id)
            .ExecuteUpdateAsync(
                setters =>
                    setters.SetProperty(
                        existingTaxCategory =>
                            existingTaxCategory.Status,
                        TaxCategoryStatus.Inactive));

        try
        {
            dbContext.ChangeTracker.Clear();

            ActivateProductHandler handler =
                CreateHandler(dbContext);

            ActivateProductCommand command =
                CreateCommand(
                    organisationId,
                    product.Id,
                    Guid.NewGuid(),
                    originalRowVersion);

            ActivateProductResult result =
                await handler.HandleAsync(
                    command,
                    CancellationToken.None);

            Assert.Equal(
                ProductStatus.Active,
                result.Status);

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
                            existingProduct.Id ==
                                product.Id);

            Assert.NotNull(savedProduct);

            Assert.Equal(
                ProductStatus.Active,
                savedProduct.Status);

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
                productId: product.Id,
                taxCategoryId: taxCategory.Id);
        }
    }

    [Fact]
    public async Task HandleAsync_WithUnknownProduct_ThrowsNotFoundException()
    {
        using IServiceScope scope =
            factory.Services.CreateScope();

        CatalogDbContext dbContext =
            scope.ServiceProvider
                .GetRequiredService<CatalogDbContext>();

        await dbContext.Database.MigrateAsync();

        ActivateProductHandler handler =
            CreateHandler(dbContext);

        ActivateProductCommand command =
            CreateCommand(
                organisationId: Guid.NewGuid(),
                productId: Guid.NewGuid(),
                modifiedBy: Guid.NewGuid(),
                rowVersion: [1]);

        NotFoundException exception =
            await Assert.ThrowsAsync<NotFoundException>(
                () => handler.HandleAsync(
                    command,
                    CancellationToken.None));

        Assert.Equal(
            "ACTIVATE_PRODUCT_NOT_FOUND",
            exception.Code);

        Assert.Equal(
            "The selected product does not exist.",
            exception.Message);
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

        Guid owningOrganisationId =
            Guid.NewGuid();

        Guid requestingOrganisationId =
            Guid.NewGuid();

        Product product =
            CreateInactiveProduct(
                owningOrganisationId,
                Guid.NewGuid());

        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync();

        byte[] rowVersion =
            product.RowVersion.ToArray();

        try
        {
            dbContext.ChangeTracker.Clear();

            ActivateProductHandler handler =
                CreateHandler(dbContext);

            ActivateProductCommand command =
                CreateCommand(
                    requestingOrganisationId,
                    product.Id,
                    Guid.NewGuid(),
                    rowVersion);

            NotFoundException exception =
                await Assert.ThrowsAsync<NotFoundException>(
                    () => handler.HandleAsync(
                        command,
                        CancellationToken.None));

            Assert.Equal(
                "ACTIVATE_PRODUCT_NOT_FOUND",
                exception.Code);

            dbContext.ChangeTracker.Clear();

            Product? savedProduct =
                await dbContext.Products
                    .AsNoTracking()
                    .SingleOrDefaultAsync(
                        existingProduct =>
                            existingProduct.Id ==
                                product.Id);

            Assert.NotNull(savedProduct);

            Assert.Equal(
                ProductStatus.Inactive,
                savedProduct.Status);
        }
        finally
        {
            await DeleteTestDataAsync(
                dbContext,
                product.Id);
        }
    }

    [Fact]
    public async Task HandleAsync_WithInactiveUnitOfMeasure_ThrowsBusinessRuleException()
    {
        using IServiceScope scope =
            factory.Services.CreateScope();

        CatalogDbContext dbContext =
            scope.ServiceProvider
                .GetRequiredService<CatalogDbContext>();

        await dbContext.Database.MigrateAsync();

        Guid organisationId = Guid.NewGuid();

        Product product =
            CreateInactiveProduct(
                organisationId: organisationId,
                createdBy: Guid.NewGuid(),
                unitOfMeasureId:
                    LitreUnitOfMeasureId);

        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync();

        byte[] rowVersion =
            product.RowVersion.ToArray();

        try
        {
            await dbContext.UnitsOfMeasures
                .Where(unitOfMeasure =>
                    unitOfMeasure.Id ==
                        LitreUnitOfMeasureId)
                .ExecuteUpdateAsync(
                    setters =>
                        setters.SetProperty(
                            unitOfMeasure =>
                                unitOfMeasure.Status,
                            UnitOfMeasureStatus.Inactive));

            dbContext.ChangeTracker.Clear();

            ActivateProductHandler handler =
                CreateHandler(dbContext);

            ActivateProductCommand command =
                CreateCommand(
                    organisationId,
                    product.Id,
                    Guid.NewGuid(),
                    rowVersion);

            BusinessRuleException exception =
                await Assert.ThrowsAsync<
                    BusinessRuleException>(
                    () => handler.HandleAsync(
                        command,
                        CancellationToken.None));

            Assert.Equal(
                "ACTIVATE_PRODUCT_UNIT_OF_MEASURE_INACTIVE",
                exception.Code);

            Assert.Equal(
                "The selected unit of measure is inactive.",
                exception.Message);

            await AssertProductRemainsInactiveAsync(
                dbContext,
                product.Id);
        }
        finally
        {
            dbContext.ChangeTracker.Clear();

            await dbContext.Products
                .Where(existingProduct =>
                    existingProduct.Id ==
                        product.Id)
                .ExecuteDeleteAsync();

            await dbContext.UnitsOfMeasures
                .Where(unitOfMeasure =>
                    unitOfMeasure.Id ==
                        LitreUnitOfMeasureId)
                .ExecuteUpdateAsync(
                    setters =>
                        setters.SetProperty(
                            unitOfMeasure =>
                                unitOfMeasure.Status,
                            UnitOfMeasureStatus.Active));
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

        Guid productOrganisationId =
            Guid.NewGuid();

        Guid categoryOrganisationId =
            Guid.NewGuid();

        ProductCategory category =
            CreateProductCategory(
                categoryOrganisationId,
                Guid.NewGuid());

        Product product =
            CreateInactiveProduct(
                organisationId:
                    productOrganisationId,
                createdBy: Guid.NewGuid(),
                productCategoryId: category.Id);

        dbContext.ProductCategories.Add(category);
        dbContext.Products.Add(product);

        await dbContext.SaveChangesAsync();

        byte[] rowVersion =
            product.RowVersion.ToArray();

        try
        {
            dbContext.ChangeTracker.Clear();

            ActivateProductHandler handler =
                CreateHandler(dbContext);

            ActivateProductCommand command =
                CreateCommand(
                    productOrganisationId,
                    product.Id,
                    Guid.NewGuid(),
                    rowVersion);

            BusinessRuleException exception =
                await Assert.ThrowsAsync<
                    BusinessRuleException>(
                    () => handler.HandleAsync(
                        command,
                        CancellationToken.None));

            Assert.Equal(
                "ACTIVATE_PRODUCT_CATEGORY_NOT_FOUND",
                exception.Code);

            Assert.Equal(
                "The selected product category does not exist.",
                exception.Message);

            await AssertProductRemainsInactiveAsync(
                dbContext,
                product.Id);
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
            modifiedAt:
                FixedUtcNow.AddMinutes(-10),
            modifiedBy: userId);

        Product product =
            CreateInactiveProduct(
                organisationId: organisationId,
                createdBy: userId,
                productCategoryId: category.Id);

        dbContext.ProductCategories.Add(category);
        dbContext.Products.Add(product);

        await dbContext.SaveChangesAsync();

        byte[] rowVersion =
            product.RowVersion.ToArray();

        try
        {
            dbContext.ChangeTracker.Clear();

            ActivateProductHandler handler =
                CreateHandler(dbContext);

            ActivateProductCommand command =
                CreateCommand(
                    organisationId,
                    product.Id,
                    Guid.NewGuid(),
                    rowVersion);

            BusinessRuleException exception =
                await Assert.ThrowsAsync<
                    BusinessRuleException>(
                    () => handler.HandleAsync(
                        command,
                        CancellationToken.None));

            Assert.Equal(
                "ACTIVATE_PRODUCT_CATEGORY_INACTIVE",
                exception.Code);

            Assert.Equal(
                "The selected product category is inactive.",
                exception.Message);

            await AssertProductRemainsInactiveAsync(
                dbContext,
                product.Id);
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
                    CurrentDate.AddDays(-10),
                effectiveTo: null);

        Product product =
            CreateInactiveProduct(
                organisationId: organisationId,
                createdBy: Guid.NewGuid(),
                taxCategoryId: taxCategory.Id);

        dbContext.TaxCategories.Add(taxCategory);
        dbContext.Products.Add(product);

        await dbContext.SaveChangesAsync();

        byte[] rowVersion =
            product.RowVersion.ToArray();

        try
        {
            await dbContext.TaxCategories
                .Where(existingTaxCategory =>
                    existingTaxCategory.Id ==
                        taxCategory.Id)
                .ExecuteUpdateAsync(
                    setters =>
                        setters.SetProperty(
                            existingTaxCategory =>
                                existingTaxCategory.Status,
                            TaxCategoryStatus.Inactive));

            dbContext.ChangeTracker.Clear();

            ActivateProductHandler handler =
                CreateHandler(dbContext);

            ActivateProductCommand command =
                CreateCommand(
                    organisationId,
                    product.Id,
                    Guid.NewGuid(),
                    rowVersion);

            BusinessRuleException exception =
                await Assert.ThrowsAsync<
                    BusinessRuleException>(
                    () => handler.HandleAsync(
                        command,
                        CancellationToken.None));

            Assert.Equal(
                "ACTIVATE_PRODUCT_TAX_CATEGORY_INACTIVE",
                exception.Code);

            Assert.Equal(
                "The selected tax category is inactive.",
                exception.Message);

            await AssertProductRemainsInactiveAsync(
                dbContext,
                product.Id);
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

        Guid organisationId = Guid.NewGuid();

        TaxCategory taxCategory =
            CreateTaxCategory(
                effectiveFrom:
                    CurrentDate.AddDays(
                        effectiveFromOffsetDays),
                effectiveTo:
                    CurrentDate.AddDays(
                        effectiveToOffsetDays));

        Product product =
            CreateInactiveProduct(
                organisationId: organisationId,
                createdBy: Guid.NewGuid(),
                taxCategoryId: taxCategory.Id);

        dbContext.TaxCategories.Add(taxCategory);
        dbContext.Products.Add(product);

        await dbContext.SaveChangesAsync();

        byte[] rowVersion =
            product.RowVersion.ToArray();

        try
        {
            dbContext.ChangeTracker.Clear();

            ActivateProductHandler handler =
                CreateHandler(dbContext);

            ActivateProductCommand command =
                CreateCommand(
                    organisationId,
                    product.Id,
                    Guid.NewGuid(),
                    rowVersion);

            BusinessRuleException exception =
                await Assert.ThrowsAsync<
                    BusinessRuleException>(
                    () => handler.HandleAsync(
                        command,
                        CancellationToken.None));

            Assert.Equal(
                "ACTIVATE_PRODUCT_TAX_CATEGORY_NOT_EFFECTIVE",
                exception.Code);

            Assert.Equal(
                "The selected tax category is not effective on the current date.",
                exception.Message);

            await AssertProductRemainsInactiveAsync(
                dbContext,
                product.Id);
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
    [InlineData(0, 10)]
    [InlineData(-10, 0)]
    public async Task HandleAsync_WithTaxCategoryOnEffectiveBoundary_ActivatesProduct(
        int effectiveFromOffsetDays,
        int effectiveToOffsetDays)
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
                    CurrentDate.AddDays(
                        effectiveFromOffsetDays),
                effectiveTo:
                    CurrentDate.AddDays(
                        effectiveToOffsetDays));

        Product product =
            CreateInactiveProduct(
                organisationId: organisationId,
                createdBy: Guid.NewGuid(),
                taxCategoryId: taxCategory.Id);

        dbContext.TaxCategories.Add(taxCategory);
        dbContext.Products.Add(product);

        await dbContext.SaveChangesAsync();

        byte[] originalRowVersion =
            product.RowVersion.ToArray();

        try
        {
            dbContext.ChangeTracker.Clear();

            ActivateProductHandler handler =
                CreateHandler(dbContext);

            ActivateProductCommand command =
                CreateCommand(
                    organisationId,
                    product.Id,
                    Guid.NewGuid(),
                    originalRowVersion);

            ActivateProductResult result =
                await handler.HandleAsync(
                    command,
                    CancellationToken.None);

            Assert.Equal(
                ProductStatus.Active,
                result.Status);

            Assert.False(
                originalRowVersion.SequenceEqual(
                    result.RowVersion));
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
    public async Task HandleAsync_WithStaleRowVersion_ThrowsConcurrencyConflictAndDoesNotActivateProduct()
    {
        using IServiceScope scope =
            factory.Services.CreateScope();

        CatalogDbContext dbContext =
            scope.ServiceProvider
                .GetRequiredService<CatalogDbContext>();

        await dbContext.Database.MigrateAsync();

        Guid organisationId = Guid.NewGuid();

        Product product =
            CreateInactiveProduct(
                organisationId,
                Guid.NewGuid());

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
                    setters =>
                        setters.SetProperty(
                            existingProduct =>
                                existingProduct.Name,
                            "Changed By Another User"));

            dbContext.ChangeTracker.Clear();

            ActivateProductHandler handler =
                CreateHandler(dbContext);

            ActivateProductCommand command =
                CreateCommand(
                    organisationId,
                    product.Id,
                    Guid.NewGuid(),
                    staleRowVersion);

            BusinessRuleException exception =
                await Assert.ThrowsAsync<
                    BusinessRuleException>(
                    () => handler.HandleAsync(
                        command,
                        CancellationToken.None));

            Assert.Equal(
                "ACTIVATE_PRODUCT_CONCURRENCY_CONFLICT",
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
                            existingProduct.Id ==
                                product.Id);

            Assert.NotNull(savedProduct);

            Assert.Equal(
                ProductStatus.Inactive,
                savedProduct.Status);

            Assert.Equal(
                "Changed By Another User",
                savedProduct.Name);
        }
        finally
        {
            await DeleteTestDataAsync(
                dbContext,
                product.Id);
        }
    }

    [Theory]
    [InlineData(InvalidCommandField.OrganisationId)]
    [InlineData(InvalidCommandField.ProductId)]
    [InlineData(InvalidCommandField.ModifiedBy)]
    [InlineData(InvalidCommandField.NullRowVersion)]
    [InlineData(InvalidCommandField.EmptyRowVersion)]
    public async Task HandleAsync_WithInvalidRequiredField_ThrowsRequestValidationException(
        InvalidCommandField invalidField)
    {
        using IServiceScope scope =
            factory.Services.CreateScope();

        CatalogDbContext dbContext =
            scope.ServiceProvider
                .GetRequiredService<CatalogDbContext>();

        ActivateProductHandler handler =
            CreateHandler(dbContext);

        Guid organisationId = Guid.NewGuid();
        Guid productId = Guid.NewGuid();
        Guid modifiedBy = Guid.NewGuid();
        byte[]? rowVersion = [1];

        string expectedCode =
            invalidField switch
            {
                InvalidCommandField.OrganisationId =>
                    "ACTIVATE_PRODUCT_ORGANISATION_REQUIRED",

                InvalidCommandField.ProductId =>
                    "ACTIVATE_PRODUCT_ID_REQUIRED",

                InvalidCommandField.ModifiedBy =>
                    "ACTIVATE_PRODUCT_MODIFIED_BY_REQUIRED",

                InvalidCommandField.NullRowVersion =>
                    "ACTIVATE_PRODUCT_ROW_VERSION_REQUIRED",

                InvalidCommandField.EmptyRowVersion =>
                    "ACTIVATE_PRODUCT_ROW_VERSION_REQUIRED",

                _ => throw new ArgumentOutOfRangeException(
                    nameof(invalidField))
            };

        switch (invalidField)
        {
            case InvalidCommandField.OrganisationId:
                organisationId = Guid.Empty;
                break;

            case InvalidCommandField.ProductId:
                productId = Guid.Empty;
                break;

            case InvalidCommandField.ModifiedBy:
                modifiedBy = Guid.Empty;
                break;

            case InvalidCommandField.NullRowVersion:
                rowVersion = null;
                break;

            case InvalidCommandField.EmptyRowVersion:
                rowVersion = [];
                break;

            default:
                throw new ArgumentOutOfRangeException(
                    nameof(invalidField));
        }

        ActivateProductCommand command =
            new(
                ProductId: productId,
                OrganisationId: organisationId,
                ModifiedBy: modifiedBy,
                RowVersion: rowVersion!);

        RequestValidationException exception =
            await Assert.ThrowsAsync<
                RequestValidationException>(
                () => handler.HandleAsync(
                    command,
                    CancellationToken.None));

        Assert.Equal(
            expectedCode,
            exception.Code);
    }

    [Fact]
    public async Task HandleAsync_WithNullCommand_ThrowsArgumentNullException()
    {
        using IServiceScope scope =
            factory.Services.CreateScope();

        CatalogDbContext dbContext =
            scope.ServiceProvider
                .GetRequiredService<CatalogDbContext>();

        ActivateProductHandler handler =
            CreateHandler(dbContext);

        ArgumentNullException exception =
            await Assert.ThrowsAsync<
                ArgumentNullException>(
                () => handler.HandleAsync(
                    null!,
                    CancellationToken.None));

        Assert.Equal(
            "command",
            exception.ParamName);
    }

    private static DateOnly CurrentDate =>
        DateOnly.FromDateTime(
            FixedUtcNow.UtcDateTime);

    private static ActivateProductHandler CreateHandler(
        CatalogDbContext dbContext)
    {
        return new ActivateProductHandler(
            dbContext,
            new TestClock(FixedUtcNow));
    }

    private static ActivateProductCommand CreateCommand(
        Guid organisationId,
        Guid productId,
        Guid modifiedBy,
        byte[] rowVersion)
    {
        return new ActivateProductCommand(
            ProductId: productId,
            OrganisationId: organisationId,
            ModifiedBy: modifiedBy,
            RowVersion: rowVersion);
    }

    private static Product CreateActiveProduct(
        Guid organisationId,
        Guid createdBy,
        Guid? productCategoryId = null,
        Guid? unitOfMeasureId = null,
        Guid? taxCategoryId = null)
    {
        return Product.Create(
            organisationId: organisationId,
            sku:
                $"ACTIVATE-{Guid.NewGuid():N}",
            name: "Office Chair",
            description:
                "Ergonomic office chair.",
            unitOfMeasureId:
                unitOfMeasureId ??
                    EachUnitOfMeasureId,
            productCategoryId:
                productCategoryId,
            taxCategoryId:
                taxCategoryId ??
                    StandardGstTaxCategoryId,
            createdAt:
                FixedUtcNow.AddDays(-2),
            createdBy: createdBy);
    }

    private static Product CreateInactiveProduct(
        Guid organisationId,
        Guid createdBy,
        Guid? productCategoryId = null,
        Guid? unitOfMeasureId = null,
        Guid? taxCategoryId = null)
    {
        Product product =
            CreateActiveProduct(
                organisationId,
                createdBy,
                productCategoryId,
                unitOfMeasureId,
                taxCategoryId);

        product.Deactivate(
            modifiedAt:
                FixedUtcNow.AddDays(-1),
            modifiedBy: createdBy);

        return product;
    }

    private static ProductCategory CreateProductCategory(
        Guid organisationId,
        Guid createdBy)
    {
        return ProductCategory.Create(
            organisationId: organisationId,
            code:
                $"CATEGORY-{Guid.NewGuid():N}",
            name: "Office Furniture",
            description:
                "Office furniture category.",
            createdAt:
                FixedUtcNow.AddDays(-2),
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
            description:
                "Tax category used by integration tests.",
            rate: 0.15m,
            treatment:
                TaxTreatment.StandardRated,
            effectiveFrom: effectiveFrom,
            effectiveTo: effectiveTo);
    }

    private static async Task AssertProductRemainsInactiveAsync(
        CatalogDbContext dbContext,
        Guid productId)
    {
        dbContext.ChangeTracker.Clear();

        Product? savedProduct =
            await dbContext.Products
                .AsNoTracking()
                .SingleOrDefaultAsync(
                    product =>
                        product.Id == productId);

        Assert.NotNull(savedProduct);

        Assert.Equal(
            ProductStatus.Inactive,
            savedProduct.Status);
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

    public enum InvalidCommandField
    {
        OrganisationId,
        ProductId,
        ModifiedBy,
        NullRowVersion,
        EmptyRowVersion
    }

    private sealed class TestClock(
        DateTimeOffset utcNow)
        : IClock
    {
        public DateTimeOffset UtcNow { get; } =
            utcNow;
    }
}
