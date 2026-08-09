using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TradeFlow.Modules.Catalog.Application.Products.CreateProduct;
using TradeFlow.Modules.Catalog.Domain.Products;
using TradeFlow.Modules.Catalog.Infrastructure.Persistence;
using TradeFlow.BuildingBlocks.Exceptions;
using TradeFlow.Modules.Catalog.Domain.UnitsOfMeasure;
using TradeFlow.Modules.Catalog.Domain.ProductCategories;
using TradeFlow.Modules.Catalog.Domain.TaxCategories;
using TradeFlow.BuildingBlocks.Time;

namespace TradeFlow.IntegrationTests.Catalog.Products;

public sealed class CreateProductHandlerTests(ApiFactory factory)
    : IClassFixture<ApiFactory>
{
    private static readonly Guid EachUnitOfMeasureId =
        Guid.Parse("10000000-0000-0000-0000-000000000001");

    private static readonly Guid StandardGstTaxCategoryId =
        Guid.Parse("20000000-0000-0000-0000-000000000001");
    private static readonly Guid MetreUnitOfMeasureId =
        Guid.Parse("10000000-0000-0000-0000-000000000004");

    private static readonly Guid ZeroRatedTaxCategoryId =
        Guid.Parse("20000000-0000-0000-0000-000000000002");

    [Fact]
    public async Task HandleAsync_WithValidCommand_SavesProduct()
    {
        using IServiceScope scope =
            factory.Services.CreateScope();

        CatalogDbContext dbContext =
            scope.ServiceProvider
                .GetRequiredService<CatalogDbContext>();

        await dbContext.Database.MigrateAsync();

        CreateProductHandler handler =
            scope.ServiceProvider
                .GetRequiredService<CreateProductHandler>();

        Guid organisationId = Guid.NewGuid();
        Guid createdBy = Guid.NewGuid();

        string uniqueSku =
            $" chair-{Guid.NewGuid():N} ";

        CreateProductCommand command = new(
            OrganisationId: organisationId,
            Sku: uniqueSku,
            Name: " Office   Chair ",
            Description: " Ergonomic office chair ",
            UnitOfMeasureId: EachUnitOfMeasureId,
            ProductCategoryId: null,
            TaxCategoryId: StandardGstTaxCategoryId,
            CreatedBy: createdBy);

        CreateProductResult result =
            await handler.HandleAsync(
                command,
                CancellationToken.None);

        string expectedSku =
            uniqueSku.Trim().ToUpperInvariant();

        Assert.NotEqual(Guid.Empty, result.ProductId);
        Assert.Equal(expectedSku, result.Sku);

        dbContext.ChangeTracker.Clear();

        Product? savedProduct =
            await dbContext.Products
                .AsNoTracking()
                .SingleOrDefaultAsync(
                    product => product.Id == result.ProductId);

        Assert.NotNull(savedProduct);

        Assert.Equal(result.ProductId, savedProduct.Id);
        Assert.Equal(organisationId, savedProduct.OrganisationId);
        Assert.Equal(expectedSku, savedProduct.Sku);
        Assert.Equal("Office Chair", savedProduct.Name);
        Assert.Equal(
            "Ergonomic office chair",
            savedProduct.Description);

        Assert.Equal(
            EachUnitOfMeasureId,
            savedProduct.UnitOfMeasureId);

        Assert.Null(savedProduct.ProductCategoryId);

        Assert.Equal(
            StandardGstTaxCategoryId,
            savedProduct.TaxCategoryId);

        Assert.Equal(ProductStatus.Active, savedProduct.Status);
        Assert.Equal(createdBy, savedProduct.CreatedBy);
        Assert.NotEqual(default, savedProduct.CreatedAt);
        Assert.Equal(TimeSpan.Zero, savedProduct.CreatedAt.Offset);
        Assert.Null(savedProduct.LastModifiedAt);
        Assert.Null(savedProduct.LastModifiedBy);
    }
    [Fact]
    public async Task HandleAsync_WithDuplicateSkuInSameOrganisation_ThrowsBusinessRuleException()
    {
        using IServiceScope scope =
            factory.Services.CreateScope();

        CatalogDbContext dbContext =
            scope.ServiceProvider
                .GetRequiredService<CatalogDbContext>();

        await dbContext.Database.MigrateAsync();

        CreateProductHandler handler =
            scope.ServiceProvider
                .GetRequiredService<CreateProductHandler>();

        Guid organisationId = Guid.NewGuid();
        Guid createdBy = Guid.NewGuid();

        string sku =
            $"CHAIR-{Guid.NewGuid():N}";

        CreateProductCommand firstCommand = new(
            OrganisationId: organisationId,
            Sku: sku,
            Name: "Office Chair",
            Description: string.Empty,
            UnitOfMeasureId: EachUnitOfMeasureId,
            ProductCategoryId: null,
            TaxCategoryId: StandardGstTaxCategoryId,
            CreatedBy: createdBy);

        await handler.HandleAsync(
            firstCommand,
            CancellationToken.None);

        CreateProductCommand duplicateCommand = new(
            OrganisationId: organisationId,
            Sku: $" {sku.ToLowerInvariant()} ",
            Name: "Another Office Chair",
            Description: string.Empty,
            UnitOfMeasureId: EachUnitOfMeasureId,
            ProductCategoryId: null,
            TaxCategoryId: StandardGstTaxCategoryId,
            CreatedBy: createdBy);

        BusinessRuleException exception =
            await Assert.ThrowsAsync<BusinessRuleException>(
                () => handler.HandleAsync(
                    duplicateCommand,
                    CancellationToken.None));

        Assert.Equal(
            "PRODUCT_SKU_ALREADY_EXISTS",
            exception.Code);

        Assert.Equal(
            "A product with this SKU already exists in the organisation.",
            exception.Message);

        int productCount =
            await dbContext.Products
                .AsNoTracking()
                .CountAsync(
                    product =>
                        product.OrganisationId == organisationId &&
                        product.Sku == sku);

        Assert.Equal(1, productCount);
    }
    [Fact]
    public async Task HandleAsync_WithSameSkuInDifferentOrganisations_SavesBothProducts()
    {
        using IServiceScope scope =
            factory.Services.CreateScope();

        CatalogDbContext dbContext =
            scope.ServiceProvider
                .GetRequiredService<CatalogDbContext>();

        await dbContext.Database.MigrateAsync();

        CreateProductHandler handler =
            scope.ServiceProvider
                .GetRequiredService<CreateProductHandler>();

        Guid firstOrganisationId = Guid.NewGuid();
        Guid secondOrganisationId = Guid.NewGuid();
        Guid createdBy = Guid.NewGuid();

        string inputSku =
       $"desk-{Guid.NewGuid():N}";

        string expectedSku =
            inputSku.Trim().ToUpperInvariant();

        CreateProductCommand firstCommand = new(
            OrganisationId: firstOrganisationId,
            Sku: inputSku,
            Name: "Office Desk",
            Description: string.Empty,
            UnitOfMeasureId: EachUnitOfMeasureId,
            ProductCategoryId: null,
            TaxCategoryId: StandardGstTaxCategoryId,
            CreatedBy: createdBy);

        CreateProductCommand secondCommand = new(
            OrganisationId: secondOrganisationId,
            Sku: inputSku,
            Name: "Office Desk",
            Description: string.Empty,
            UnitOfMeasureId: EachUnitOfMeasureId,
            ProductCategoryId: null,
            TaxCategoryId: StandardGstTaxCategoryId,
            CreatedBy: createdBy);

        CreateProductResult firstResult =
            await handler.HandleAsync(
                firstCommand,
                CancellationToken.None);

        CreateProductResult secondResult =
            await handler.HandleAsync(
                secondCommand,
                CancellationToken.None);

        Assert.NotEqual(Guid.Empty, firstResult.ProductId);
        Assert.NotEqual(Guid.Empty, secondResult.ProductId);

        Assert.NotEqual(
            firstResult.ProductId,
            secondResult.ProductId);

        Assert.Equal(expectedSku, firstResult.Sku);
        Assert.Equal(expectedSku, secondResult.Sku);

        dbContext.ChangeTracker.Clear();

        List<Product> savedProducts =
            await dbContext.Products
                .AsNoTracking()
                .Where(product =>
                    product.Sku == expectedSku &&
                    (product.OrganisationId == firstOrganisationId ||
                     product.OrganisationId == secondOrganisationId))
                .ToListAsync();

        Assert.Equal(2, savedProducts.Count);

        Assert.Contains(
            savedProducts,
            product =>
                product.OrganisationId == firstOrganisationId);

        Assert.Contains(
            savedProducts,
            product =>
                product.OrganisationId == secondOrganisationId);
    }
    [Fact]
    public async Task HandleAsync_WithUnknownUnitOfMeasure_ThrowsBusinessRuleException()
    {
        using IServiceScope scope =
            factory.Services.CreateScope();

        CatalogDbContext dbContext =
            scope.ServiceProvider
                .GetRequiredService<CatalogDbContext>();

        await dbContext.Database.MigrateAsync();

        CreateProductHandler handler =
            scope.ServiceProvider
                .GetRequiredService<CreateProductHandler>();

        Guid organisationId = Guid.NewGuid();

        string sku =
            $"TABLE-{Guid.NewGuid():N}";

        CreateProductCommand command = new(
            OrganisationId: organisationId,
            Sku: sku,
            Name: "Meeting Table",
            Description: string.Empty,
            UnitOfMeasureId: Guid.NewGuid(),
            ProductCategoryId: null,
            TaxCategoryId: StandardGstTaxCategoryId,
            CreatedBy: Guid.NewGuid());

        BusinessRuleException exception =
            await Assert.ThrowsAsync<BusinessRuleException>(
                () => handler.HandleAsync(
                    command,
                    CancellationToken.None));

        Assert.Equal(
            "PRODUCT_UNIT_OF_MEASURE_NOT_FOUND",
            exception.Code);

        Assert.Equal(
            "The selected unit of measure does not exist.",
            exception.Message);

        bool productWasSaved =
            await dbContext.Products
                .AsNoTracking()
                .AnyAsync(
                    product =>
                        product.OrganisationId == organisationId &&
                        product.Sku == sku);

        Assert.False(productWasSaved);
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

        CreateProductHandler handler =
            scope.ServiceProvider
                .GetRequiredService<CreateProductHandler>();

        Guid organisationId = Guid.NewGuid();

        string inputSku =
            $"PIPE-{Guid.NewGuid():N}";

        string expectedSku =
            inputSku.Trim().ToUpperInvariant();

        await dbContext.UnitsOfMeasures
            .Where(unitOfMeasure =>
                unitOfMeasure.Id == MetreUnitOfMeasureId)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(
                    unitOfMeasure => unitOfMeasure.Status,
                    UnitOfMeasureStatus.Inactive));

        try
        {
            CreateProductCommand command = new(
                OrganisationId: organisationId,
                Sku: inputSku,
                Name: "Steel Pipe",
                Description: string.Empty,
                UnitOfMeasureId: MetreUnitOfMeasureId,
                ProductCategoryId: null,
                TaxCategoryId: StandardGstTaxCategoryId,
                CreatedBy: Guid.NewGuid());

            BusinessRuleException exception =
                await Assert.ThrowsAsync<BusinessRuleException>(
                    () => handler.HandleAsync(
                        command,
                        CancellationToken.None));

            Assert.Equal(
                "PRODUCT_UNIT_OF_MEASURE_INACTIVE",
                exception.Code);

            Assert.Equal(
                "The selected unit of measure is inactive.",
                exception.Message);

            bool productWasSaved =
                await dbContext.Products
                    .AsNoTracking()
                    .AnyAsync(
                        product =>
                            product.OrganisationId == organisationId &&
                            product.Sku == expectedSku);

            Assert.False(productWasSaved);
        }
        finally
        {
            await dbContext.Products
                .Where(product =>
                    product.OrganisationId == organisationId)
                .ExecuteDeleteAsync();

            await dbContext.UnitsOfMeasures
                .Where(unitOfMeasure =>
                    unitOfMeasure.Id == MetreUnitOfMeasureId)
                .ExecuteUpdateAsync(
                    setters => setters.SetProperty(
                        unitOfMeasure => unitOfMeasure.Status,
                        UnitOfMeasureStatus.Active));
        }
    }

    [Fact]
    public async Task HandleAsync_WithUnknownProductCategory_ThrowsBusinessRuleException()
    {
        using IServiceScope scope =
            factory.Services.CreateScope();

        CatalogDbContext dbContext =
            scope.ServiceProvider
                .GetRequiredService<CatalogDbContext>();

        await dbContext.Database.MigrateAsync();

        CreateProductHandler handler =
            scope.ServiceProvider
                .GetRequiredService<CreateProductHandler>();

        Guid organisationId = Guid.NewGuid();

        string inputSku =
            $"CABINET-{Guid.NewGuid():N}";

        string expectedSku =
            inputSku.Trim().ToUpperInvariant();

        CreateProductCommand command = new(
            OrganisationId: organisationId,
            Sku: inputSku,
            Name: "Storage Cabinet",
            Description: string.Empty,
            UnitOfMeasureId: EachUnitOfMeasureId,
            ProductCategoryId: Guid.NewGuid(),
            TaxCategoryId: StandardGstTaxCategoryId,
            CreatedBy: Guid.NewGuid());

        BusinessRuleException exception =
            await Assert.ThrowsAsync<BusinessRuleException>(
                () => handler.HandleAsync(
                    command,
                    CancellationToken.None));

        Assert.Equal(
            "PRODUCT_CATEGORY_NOT_FOUND",
            exception.Code);

        Assert.Equal(
            "The selected product category does not exist in the organisation.",
            exception.Message);

        bool productWasSaved =
            await dbContext.Products
                .AsNoTracking()
                .AnyAsync(
                    product =>
                        product.OrganisationId == organisationId &&
                        product.Sku == expectedSku);

        Assert.False(productWasSaved);
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

        CreateProductHandler handler =
            scope.ServiceProvider
                .GetRequiredService<CreateProductHandler>();

        Guid productOrganisationId = Guid.NewGuid();
        Guid categoryOrganisationId = Guid.NewGuid();
        Guid userId = Guid.NewGuid();

        ProductCategory category =
            ProductCategory.Create(
                organisationId: categoryOrganisationId,
                code: $"OTHER-{Guid.NewGuid():N}",
                name: "Other Organisation Category",
                description: string.Empty,
                createdAt: DateTimeOffset.UtcNow,
                createdBy: userId);

        dbContext.ProductCategories.Add(category);
        await dbContext.SaveChangesAsync();

        string inputSku =
            $"SHELF-{Guid.NewGuid():N}";

        string expectedSku =
            inputSku.Trim().ToUpperInvariant();

        try
        {
            CreateProductCommand command = new(
                OrganisationId: productOrganisationId,
                Sku: inputSku,
                Name: "Storage Shelf",
                Description: string.Empty,
                UnitOfMeasureId: EachUnitOfMeasureId,
                ProductCategoryId: category.Id,
                TaxCategoryId: StandardGstTaxCategoryId,
                CreatedBy: userId);

            BusinessRuleException exception =
                await Assert.ThrowsAsync<BusinessRuleException>(
                    () => handler.HandleAsync(
                        command,
                        CancellationToken.None));

            Assert.Equal(
                "PRODUCT_CATEGORY_NOT_FOUND",
                exception.Code);

            Assert.Equal(
                "The selected product category does not exist in the organisation.",
                exception.Message);

            bool productWasSaved =
                await dbContext.Products
                    .AsNoTracking()
                    .AnyAsync(
                        product =>
                            product.OrganisationId ==
                                productOrganisationId &&
                            product.Sku == expectedSku);

            Assert.False(productWasSaved);
        }
        finally
        {
            await dbContext.Products
                .Where(product =>
                    product.OrganisationId ==
                        productOrganisationId)
                .ExecuteDeleteAsync();

            await dbContext.ProductCategories
                .Where(existingCategory =>
                    existingCategory.Id == category.Id)
                .ExecuteDeleteAsync();
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

        CreateProductHandler handler =
            scope.ServiceProvider
                .GetRequiredService<CreateProductHandler>();

        Guid organisationId = Guid.NewGuid();
        Guid userId = Guid.NewGuid();

        DateTimeOffset createdAt =
            DateTimeOffset.UtcNow.AddMinutes(-1);

        DateTimeOffset modifiedAt =
            DateTimeOffset.UtcNow;

        ProductCategory category =
            ProductCategory.Create(
                organisationId: organisationId,
                code: $"INACTIVE-{Guid.NewGuid():N}",
                name: "Inactive Category",
                description: string.Empty,
                createdAt: createdAt,
                createdBy: userId);

        category.Deactivate(
            modifiedAt: modifiedAt,
            modifiedBy: userId);

        dbContext.ProductCategories.Add(category);
        await dbContext.SaveChangesAsync();

        string inputSku =
            $"LAMP-{Guid.NewGuid():N}";

        string expectedSku =
            inputSku.Trim().ToUpperInvariant();

        try
        {
            CreateProductCommand command = new(
                OrganisationId: organisationId,
                Sku: inputSku,
                Name: "Desk Lamp",
                Description: string.Empty,
                UnitOfMeasureId: EachUnitOfMeasureId,
                ProductCategoryId: category.Id,
                TaxCategoryId: StandardGstTaxCategoryId,
                CreatedBy: userId);

            BusinessRuleException exception =
                await Assert.ThrowsAsync<BusinessRuleException>(
                    () => handler.HandleAsync(
                        command,
                        CancellationToken.None));

            Assert.Equal(
                "PRODUCT_CATEGORY_INACTIVE",
                exception.Code);

            Assert.Equal(
                "The selected product category is inactive.",
                exception.Message);

            bool productWasSaved =
                await dbContext.Products
                    .AsNoTracking()
                    .AnyAsync(
                        product =>
                            product.OrganisationId == organisationId &&
                            product.Sku == expectedSku);

            Assert.False(productWasSaved);
        }
        finally
        {
            await dbContext.Products
                .Where(product =>
                    product.OrganisationId == organisationId)
                .ExecuteDeleteAsync();

            await dbContext.ProductCategories
                .Where(existingCategory =>
                    existingCategory.Id == category.Id)
                .ExecuteDeleteAsync();
        }
    }

    [Fact]
    public async Task HandleAsync_WithActiveCategoryInSameOrganisation_SavesProduct()
    {
        using IServiceScope scope =
            factory.Services.CreateScope();

        CatalogDbContext dbContext =
            scope.ServiceProvider
                .GetRequiredService<CatalogDbContext>();

        await dbContext.Database.MigrateAsync();

        CreateProductHandler handler =
            scope.ServiceProvider
                .GetRequiredService<CreateProductHandler>();

        Guid organisationId = Guid.NewGuid();
        Guid userId = Guid.NewGuid();

        ProductCategory category =
            ProductCategory.Create(
                organisationId: organisationId,
                code: $"FURNITURE-{Guid.NewGuid():N}",
                name: "Office Furniture",
                description: "Furniture used in an office.",
                createdAt: DateTimeOffset.UtcNow,
                createdBy: userId);

        dbContext.ProductCategories.Add(category);
        await dbContext.SaveChangesAsync();

        string inputSku =
            $"BOOKCASE-{Guid.NewGuid():N}";

        string expectedSku =
            inputSku.Trim().ToUpperInvariant();

        Guid? createdProductId = null;

        try
        {
            CreateProductCommand command = new(
                OrganisationId: organisationId,
                Sku: inputSku,
                Name: "Office Bookcase",
                Description: "Five shelf office bookcase.",
                UnitOfMeasureId: EachUnitOfMeasureId,
                ProductCategoryId: category.Id,
                TaxCategoryId: StandardGstTaxCategoryId,
                CreatedBy: userId);

            CreateProductResult result =
                await handler.HandleAsync(
                    command,
                    CancellationToken.None);

            createdProductId = result.ProductId;

            Assert.NotEqual(Guid.Empty, result.ProductId);
            Assert.Equal(expectedSku, result.Sku);

            dbContext.ChangeTracker.Clear();

            Product? savedProduct =
                await dbContext.Products
                    .AsNoTracking()
                    .SingleOrDefaultAsync(
                        product =>
                            product.Id == result.ProductId);

            Assert.NotNull(savedProduct);
            Assert.Equal(organisationId, savedProduct.OrganisationId);
            Assert.Equal(category.Id, savedProduct.ProductCategoryId);
            Assert.Equal(expectedSku, savedProduct.Sku);
            Assert.Equal(ProductStatus.Active, savedProduct.Status);
        }
        finally
        {
            if (createdProductId.HasValue)
            {
                await dbContext.Products
                    .Where(product =>
                        product.Id == createdProductId.Value)
                    .ExecuteDeleteAsync();
            }

            await dbContext.ProductCategories
                .Where(existingCategory =>
                    existingCategory.Id == category.Id)
                .ExecuteDeleteAsync();
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

        CreateProductHandler handler =
            scope.ServiceProvider
                .GetRequiredService<CreateProductHandler>();

        Guid organisationId = Guid.NewGuid();

        string inputSku =
            $"MONITOR-{Guid.NewGuid():N}";

        string expectedSku =
            inputSku.Trim().ToUpperInvariant();

        CreateProductCommand command = new(
            OrganisationId: organisationId,
            Sku: inputSku,
            Name: "Computer Monitor",
            Description: string.Empty,
            UnitOfMeasureId: EachUnitOfMeasureId,
            ProductCategoryId: null,
            TaxCategoryId: Guid.NewGuid(),
            CreatedBy: Guid.NewGuid());

        BusinessRuleException exception =
            await Assert.ThrowsAsync<BusinessRuleException>(
                () => handler.HandleAsync(
                    command,
                    CancellationToken.None));

        Assert.Equal(
            "PRODUCT_TAX_CATEGORY_NOT_FOUND",
            exception.Code);

        Assert.Equal(
            "The selected tax category does not exist.",
            exception.Message);

        bool productWasSaved =
            await dbContext.Products
                .AsNoTracking()
                .AnyAsync(
                    product =>
                        product.OrganisationId == organisationId &&
                        product.Sku == expectedSku);

        Assert.False(productWasSaved);
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

        CreateProductHandler handler =
            scope.ServiceProvider
                .GetRequiredService<CreateProductHandler>();

        Guid organisationId = Guid.NewGuid();

        string inputSku =
            $"KEYBOARD-{Guid.NewGuid():N}";

        string expectedSku =
            inputSku.Trim().ToUpperInvariant();

        await dbContext.TaxCategories
            .Where(taxCategory =>
                taxCategory.Id == ZeroRatedTaxCategoryId)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(
                    taxCategory => taxCategory.Status,
                    TaxCategoryStatus.Inactive));

        try
        {
            CreateProductCommand command = new(
                OrganisationId: organisationId,
                Sku: inputSku,
                Name: "Computer Keyboard",
                Description: string.Empty,
                UnitOfMeasureId: EachUnitOfMeasureId,
                ProductCategoryId: null,
                TaxCategoryId: ZeroRatedTaxCategoryId,
                CreatedBy: Guid.NewGuid());

            BusinessRuleException exception =
                await Assert.ThrowsAsync<BusinessRuleException>(
                    () => handler.HandleAsync(
                        command,
                        CancellationToken.None));

            Assert.Equal(
                "PRODUCT_TAX_CATEGORY_INACTIVE",
                exception.Code);

            Assert.Equal(
                "The selected tax category is inactive.",
                exception.Message);

            bool productWasSaved =
                await dbContext.Products
                    .AsNoTracking()
                    .AnyAsync(
                        product =>
                            product.OrganisationId == organisationId &&
                            product.Sku == expectedSku);

            Assert.False(productWasSaved);
        }
        finally
        {
            await dbContext.Products
                .Where(product =>
                    product.OrganisationId == organisationId)
                .ExecuteDeleteAsync();

            await dbContext.TaxCategories
                .Where(taxCategory =>
                    taxCategory.Id == ZeroRatedTaxCategoryId)
                .ExecuteUpdateAsync(
                    setters => setters.SetProperty(
                        taxCategory => taxCategory.Status,
                        TaxCategoryStatus.Active));
        }
    }
    private sealed class TestClock(
    DateTimeOffset utcNow) : IClock
    {
        public DateTimeOffset UtcNow { get; } = utcNow;
    }
    [Fact]
    public async Task HandleAsync_WithTaxCategoryNotYetEffective_ThrowsBusinessRuleException()
    {
        using IServiceScope scope =
            factory.Services.CreateScope();

        CatalogDbContext dbContext =
            scope.ServiceProvider
                .GetRequiredService<CatalogDbContext>();

        await dbContext.Database.MigrateAsync();

        DateTimeOffset fixedUtcNow =
            new(
                year: 2026,
                month: 8,
                day: 5,
                hour: 12,
                minute: 0,
                second: 0,
                offset: TimeSpan.Zero);

        TestClock clock = new(fixedUtcNow);

        CreateProductHandler handler =
            new(dbContext, clock);

        string taxCategoryCode =
            $"FUTURE-{Guid.NewGuid().ToString("N")[..20]}";

        TaxCategory futureTaxCategory =
            TaxCategory.Create(
                code: taxCategoryCode,
                name: "Future GST Category",
                description: string.Empty,
                rate: 0.15m,
                treatment: TaxTreatment.StandardRated,
                effectiveFrom: new DateOnly(2026, 8, 6),
                effectiveTo: null);

        dbContext.TaxCategories.Add(futureTaxCategory);
        await dbContext.SaveChangesAsync();

        Guid organisationId = Guid.NewGuid();

        string inputSku =
            $"MOUSE-{Guid.NewGuid():N}";

        string expectedSku =
            inputSku.Trim().ToUpperInvariant();

        try
        {
            CreateProductCommand command = new(
                OrganisationId: organisationId,
                Sku: inputSku,
                Name: "Computer Mouse",
                Description: string.Empty,
                UnitOfMeasureId: EachUnitOfMeasureId,
                ProductCategoryId: null,
                TaxCategoryId: futureTaxCategory.Id,
                CreatedBy: Guid.NewGuid());

            BusinessRuleException exception =
                await Assert.ThrowsAsync<BusinessRuleException>(
                    () => handler.HandleAsync(
                        command,
                        CancellationToken.None));

            Assert.Equal(
                "PRODUCT_TAX_CATEGORY_NOT_EFFECTIVE",
                exception.Code);

            Assert.Equal(
                "The selected tax category is not effective on the product creation date.",
                exception.Message);

            bool productWasSaved =
                await dbContext.Products
                    .AsNoTracking()
                    .AnyAsync(
                        product =>
                            product.OrganisationId == organisationId &&
                            product.Sku == expectedSku);

            Assert.False(productWasSaved);
        }
        finally
        {
            await dbContext.Products
                .Where(product =>
                    product.OrganisationId == organisationId)
                .ExecuteDeleteAsync();

            await dbContext.TaxCategories
                .Where(taxCategory =>
                    taxCategory.Id == futureTaxCategory.Id)
                .ExecuteDeleteAsync();
        }
    }

    [Fact]
    public async Task HandleAsync_WithExpiredTaxCategory_ThrowsBusinessRuleException()
    {
        using IServiceScope scope =
            factory.Services.CreateScope();

        CatalogDbContext dbContext =
            scope.ServiceProvider
                .GetRequiredService<CatalogDbContext>();

        await dbContext.Database.MigrateAsync();

        DateTimeOffset fixedUtcNow =
            new(
                year: 2026,
                month: 8,
                day: 5,
                hour: 12,
                minute: 0,
                second: 0,
                offset: TimeSpan.Zero);

        TestClock clock = new(fixedUtcNow);

        CreateProductHandler handler =
            new(dbContext, clock);

        string taxCategoryCode =
            $"EXPIRED-{Guid.NewGuid().ToString("N")[..20]}";

        TaxCategory expiredTaxCategory =
            TaxCategory.Create(
                code: taxCategoryCode,
                name: "Expired GST Category",
                description: string.Empty,
                rate: 0.15m,
                treatment: TaxTreatment.StandardRated,
                effectiveFrom: new DateOnly(2026, 1, 1),
                effectiveTo: new DateOnly(2026, 8, 4));

        dbContext.TaxCategories.Add(expiredTaxCategory);
        await dbContext.SaveChangesAsync();

        Guid organisationId = Guid.NewGuid();

        string inputSku =
            $"PRINTER-{Guid.NewGuid():N}";

        string expectedSku =
            inputSku.Trim().ToUpperInvariant();

        try
        {
            CreateProductCommand command = new(
                OrganisationId: organisationId,
                Sku: inputSku,
                Name: "Office Printer",
                Description: string.Empty,
                UnitOfMeasureId: EachUnitOfMeasureId,
                ProductCategoryId: null,
                TaxCategoryId: expiredTaxCategory.Id,
                CreatedBy: Guid.NewGuid());

            BusinessRuleException exception =
                await Assert.ThrowsAsync<BusinessRuleException>(
                    () => handler.HandleAsync(
                        command,
                        CancellationToken.None));

            Assert.Equal(
                "PRODUCT_TAX_CATEGORY_NOT_EFFECTIVE",
                exception.Code);

            Assert.Equal(
                "The selected tax category is not effective on the product creation date.",
                exception.Message);

            bool productWasSaved =
                await dbContext.Products
                    .AsNoTracking()
                    .AnyAsync(
                        product =>
                            product.OrganisationId == organisationId &&
                            product.Sku == expectedSku);

            Assert.False(productWasSaved);
        }
        finally
        {
            await dbContext.Products
                .Where(product =>
                    product.OrganisationId == organisationId)
                .ExecuteDeleteAsync();

            await dbContext.TaxCategories
                .Where(taxCategory =>
                    taxCategory.Id == expiredTaxCategory.Id)
                .ExecuteDeleteAsync();
        }
    }

    [Theory]
    [InlineData(0, 5)]
    [InlineData(-5, 0)]
    public async Task HandleAsync_WithTaxCategoryOnEffectiveBoundary_SavesProduct(
    int effectiveFromOffsetDays,
    int effectiveToOffsetDays)
    {
        using IServiceScope scope =
            factory.Services.CreateScope();

        CatalogDbContext dbContext =
            scope.ServiceProvider
                .GetRequiredService<CatalogDbContext>();

        await dbContext.Database.MigrateAsync();

        DateTimeOffset fixedUtcNow =
            new(
                year: 2026,
                month: 8,
                day: 5,
                hour: 12,
                minute: 0,
                second: 0,
                offset: TimeSpan.Zero);

        DateOnly creationDate =
            DateOnly.FromDateTime(fixedUtcNow.UtcDateTime);

        DateOnly effectiveFrom =
            creationDate.AddDays(effectiveFromOffsetDays);

        DateOnly effectiveTo =
            creationDate.AddDays(effectiveToOffsetDays);

        TestClock clock = new(fixedUtcNow);

        CreateProductHandler handler =
            new(dbContext, clock);

        string taxCategoryCode =
            $"BOUND-{Guid.NewGuid().ToString("N")[..20]}";

        TaxCategory boundaryTaxCategory =
            TaxCategory.Create(
                code: taxCategoryCode,
                name: "Boundary GST Category",
                description: string.Empty,
                rate: 0.15m,
                treatment: TaxTreatment.StandardRated,
                effectiveFrom: effectiveFrom,
                effectiveTo: effectiveTo);

        dbContext.TaxCategories.Add(boundaryTaxCategory);
        await dbContext.SaveChangesAsync();

        Guid organisationId = Guid.NewGuid();

        string inputSku =
            $"SCANNER-{Guid.NewGuid():N}";

        string expectedSku =
            inputSku.Trim().ToUpperInvariant();

        try
        {
            CreateProductCommand command = new(
                OrganisationId: organisationId,
                Sku: inputSku,
                Name: "Document Scanner",
                Description: string.Empty,
                UnitOfMeasureId: EachUnitOfMeasureId,
                ProductCategoryId: null,
                TaxCategoryId: boundaryTaxCategory.Id,
                CreatedBy: Guid.NewGuid());

            CreateProductResult result =
                await handler.HandleAsync(
                    command,
                    CancellationToken.None);

            Assert.NotEqual(Guid.Empty, result.ProductId);
            Assert.Equal(expectedSku, result.Sku);

            dbContext.ChangeTracker.Clear();

            Product? savedProduct =
                await dbContext.Products
                    .AsNoTracking()
                    .SingleOrDefaultAsync(
                        product =>
                            product.Id == result.ProductId);

            Assert.NotNull(savedProduct);

            Assert.Equal(
                boundaryTaxCategory.Id,
                savedProduct.TaxCategoryId);

            Assert.Equal(
                creationDate,
                DateOnly.FromDateTime(
                    savedProduct.CreatedAt.UtcDateTime));
        }
        finally
        {
            await dbContext.Products
                .Where(product =>
                    product.OrganisationId == organisationId)
                .ExecuteDeleteAsync();

            await dbContext.TaxCategories
                .Where(taxCategory =>
                    taxCategory.Id == boundaryTaxCategory.Id)
                .ExecuteDeleteAsync();
        }
    }

    [Fact]
    public async Task HandleAsync_WithTaxCategoryWithoutEndDate_SavesProduct()
    {
        using IServiceScope scope =
            factory.Services.CreateScope();

        CatalogDbContext dbContext =
            scope.ServiceProvider
                .GetRequiredService<CatalogDbContext>();

        await dbContext.Database.MigrateAsync();

        DateTimeOffset fixedUtcNow =
            new(
                year: 2026,
                month: 8,
                day: 5,
                hour: 12,
                minute: 0,
                second: 0,
                offset: TimeSpan.Zero);

        TestClock clock = new(fixedUtcNow);

        CreateProductHandler handler =
            new(dbContext, clock);

        TaxCategory? taxCategory =
            await dbContext.TaxCategories
                .AsNoTracking()
                .SingleOrDefaultAsync(
                    category =>
                        category.Id ==
                            StandardGstTaxCategoryId);

        Assert.NotNull(taxCategory);
        Assert.Null(taxCategory.EffectiveTo);

        Guid organisationId = Guid.NewGuid();

        string inputSku =
            $"HEADSET-{Guid.NewGuid():N}";

        string expectedSku =
            inputSku.Trim().ToUpperInvariant();

        try
        {
            CreateProductCommand command = new(
                OrganisationId: organisationId,
                Sku: inputSku,
                Name: "Office Headset",
                Description: string.Empty,
                UnitOfMeasureId: EachUnitOfMeasureId,
                ProductCategoryId: null,
                TaxCategoryId: StandardGstTaxCategoryId,
                CreatedBy: Guid.NewGuid());

            CreateProductResult result =
                await handler.HandleAsync(
                    command,
                    CancellationToken.None);

            Assert.NotEqual(Guid.Empty, result.ProductId);
            Assert.Equal(expectedSku, result.Sku);

            dbContext.ChangeTracker.Clear();

            Product? savedProduct =
                await dbContext.Products
                    .AsNoTracking()
                    .SingleOrDefaultAsync(
                        product =>
                            product.Id == result.ProductId);

            Assert.NotNull(savedProduct);

            Assert.Equal(
                StandardGstTaxCategoryId,
                savedProduct.TaxCategoryId);

            Assert.Equal(
                fixedUtcNow,
                savedProduct.CreatedAt);
        }
        finally
        {
            await dbContext.Products
                .Where(product =>
                    product.OrganisationId == organisationId)
                .ExecuteDeleteAsync();
        }
    }
    [Fact]
    public async Task HandleAsync_WithNullCommand_ThrowsArgumentNullException()
    {
        using IServiceScope scope =
            factory.Services.CreateScope();

        CreateProductHandler handler =
            scope.ServiceProvider
                .GetRequiredService<CreateProductHandler>();

        ArgumentNullException exception =
            await Assert.ThrowsAsync<ArgumentNullException>(
                () => handler.HandleAsync(
                    null!,
                    CancellationToken.None));

        Assert.Equal("command", exception.ParamName);
    }

}
