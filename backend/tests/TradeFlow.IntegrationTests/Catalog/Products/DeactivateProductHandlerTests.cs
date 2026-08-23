using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TradeFlow.BuildingBlocks.Exceptions;
using TradeFlow.BuildingBlocks.Time;
using TradeFlow.Modules.Catalog.Application.Products.DeactivateProduct;
using TradeFlow.Modules.Catalog.Domain.Products;
using TradeFlow.Modules.Catalog.Infrastructure.Persistence;

namespace TradeFlow.IntegrationTests.Catalog.Products;

public sealed class DeactivateProductHandlerTests(
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
            day: 22,
            hour: 10,
            minute: 30,
            second: 0,
            offset: TimeSpan.Zero);

    [Fact]
    public async Task HandleAsync_WithValidCommand_DeactivatesProduct()
    {
        using IServiceScope scope =
            factory.Services.CreateScope();

        CatalogDbContext dbContext =
            scope.ServiceProvider
                .GetRequiredService<CatalogDbContext>();

        await dbContext.Database.MigrateAsync();

        Guid organisationId = Guid.NewGuid();
        Guid modifiedBy = Guid.NewGuid();

        Product product =
            CreateProduct(
                organisationId,
                Guid.NewGuid());

        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync();

        byte[] originalRowVersion =
            product.RowVersion.ToArray();

        try
        {
            dbContext.ChangeTracker.Clear();

            DeactivateProductHandler handler =
                new(
                    dbContext,
                    new TestClock(FixedUtcNow));

            DeactivateProductCommand command =
                new(
                    OrganisationId: organisationId,
                    ProductId: product.Id,
                    ModifiedBy: modifiedBy,
                    RowVersion: originalRowVersion);

            DeactivateProductResult result =
                await handler.HandleAsync(
                    command,
                    CancellationToken.None);

            Assert.Equal(product.Id, result.ProductId);
            Assert.Equal(
                ProductStatus.Inactive,
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
                            existingProduct.Id == product.Id);

            Assert.NotNull(savedProduct);

            Assert.Equal(
                ProductStatus.Inactive,
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
            await DeleteProductAsync(
                dbContext,
                product.Id);
        }
    }

    [Fact]
    public async Task HandleAsync_WithAlreadyInactiveProduct_DoesNotChangeAuditOrRowVersion()
    {
        using IServiceScope scope =
            factory.Services.CreateScope();

        CatalogDbContext dbContext =
            scope.ServiceProvider
                .GetRequiredService<CatalogDbContext>();

        await dbContext.Database.MigrateAsync();

        Guid organisationId = Guid.NewGuid();
        Guid originalModifiedBy = Guid.NewGuid();

        DateTimeOffset originalModifiedAt =
            FixedUtcNow.AddHours(-1);

        Product product =
            CreateProduct(
                organisationId,
                Guid.NewGuid());

        product.Deactivate(
            modifiedAt: originalModifiedAt,
            modifiedBy: originalModifiedBy);

        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync();

        byte[] originalRowVersion =
            product.RowVersion.ToArray();

        try
        {
            dbContext.ChangeTracker.Clear();

            DeactivateProductHandler handler =
                new(
                    dbContext,
                    new TestClock(FixedUtcNow));

            DeactivateProductCommand command =
                new(
                    OrganisationId: organisationId,
                    ProductId: product.Id,
                    ModifiedBy: Guid.NewGuid(),
                    RowVersion: originalRowVersion);

            DeactivateProductResult result =
                await handler.HandleAsync(
                    command,
                    CancellationToken.None);

            Assert.Equal(
                ProductStatus.Inactive,
                result.Status);

            Assert.Equal(
                originalModifiedAt,
                result.LastModifiedAt);

            Assert.Equal(
                originalModifiedBy,
                result.LastModifiedBy);

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

            Assert.Equal(
                ProductStatus.Inactive,
                savedProduct.Status);

            Assert.Equal(
                originalModifiedAt,
                savedProduct.LastModifiedAt);

            Assert.Equal(
                originalModifiedBy,
                savedProduct.LastModifiedBy);

            Assert.True(
                originalRowVersion.SequenceEqual(
                    savedProduct.RowVersion));
        }
        finally
        {
            await DeleteProductAsync(
                dbContext,
                product.Id);
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

        DeactivateProductHandler handler =
            new(
                dbContext,
                new TestClock(FixedUtcNow));

        DeactivateProductCommand command =
            new(
                OrganisationId: Guid.NewGuid(),
                ProductId: Guid.NewGuid(),
                ModifiedBy: Guid.NewGuid(),
                RowVersion: [1]);

        NotFoundException exception =
            await Assert.ThrowsAsync<NotFoundException>(
                () => handler.HandleAsync(
                    command,
                    CancellationToken.None));

        Assert.Equal(
            "DEACTIVATE_PRODUCT_NOT_FOUND",
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

        Guid owningOrganisationId = Guid.NewGuid();
        Guid otherOrganisationId = Guid.NewGuid();

        Product product =
            CreateProduct(
                owningOrganisationId,
                Guid.NewGuid());

        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync();

        byte[] rowVersion =
            product.RowVersion.ToArray();

        try
        {
            dbContext.ChangeTracker.Clear();

            DeactivateProductHandler handler =
                new(
                    dbContext,
                    new TestClock(FixedUtcNow));

            DeactivateProductCommand command =
                new(
                    OrganisationId: otherOrganisationId,
                    ProductId: product.Id,
                    ModifiedBy: Guid.NewGuid(),
                    RowVersion: rowVersion);

            NotFoundException exception =
                await Assert.ThrowsAsync<NotFoundException>(
                    () => handler.HandleAsync(
                        command,
                        CancellationToken.None));

            Assert.Equal(
                "DEACTIVATE_PRODUCT_NOT_FOUND",
                exception.Code);

            dbContext.ChangeTracker.Clear();

            Product? savedProduct =
                await dbContext.Products
                    .AsNoTracking()
                    .SingleOrDefaultAsync(
                        existingProduct =>
                            existingProduct.Id == product.Id);

            Assert.NotNull(savedProduct);

            Assert.Equal(
                ProductStatus.Active,
                savedProduct.Status);

            Assert.Null(savedProduct.LastModifiedAt);
            Assert.Null(savedProduct.LastModifiedBy);
        }
        finally
        {
            await DeleteProductAsync(
                dbContext,
                product.Id);
        }
    }

    [Fact]
    public async Task HandleAsync_WithStaleRowVersion_ThrowsConcurrencyConflictAndDoesNotDeactivateProduct()
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

            DeactivateProductHandler handler =
                new(
                    dbContext,
                    new TestClock(FixedUtcNow));

            DeactivateProductCommand command =
                new(
                    OrganisationId: organisationId,
                    ProductId: product.Id,
                    ModifiedBy: Guid.NewGuid(),
                    RowVersion: staleRowVersion);

            BusinessRuleException exception =
                await Assert.ThrowsAsync<BusinessRuleException>(
                    () => handler.HandleAsync(
                        command,
                        CancellationToken.None));

            Assert.Equal(
                "DEACTIVATE_PRODUCT_CONCURRENCY_CONFLICT",
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
                ProductStatus.Active,
                savedProduct.Status);

            Assert.Equal(
                "Changed By Another User",
                savedProduct.Name);

            Assert.Null(savedProduct.LastModifiedAt);
            Assert.Null(savedProduct.LastModifiedBy);
        }
        finally
        {
            await DeleteProductAsync(
                dbContext,
                product.Id);
        }
    }

    [Fact]
    public async Task HandleAsync_WithEmptyOrganisationId_ThrowsRequestValidationException()
    {
        using IServiceScope scope =
            factory.Services.CreateScope();

        CatalogDbContext dbContext =
            scope.ServiceProvider
                .GetRequiredService<CatalogDbContext>();

        DeactivateProductHandler handler =
            new(
                dbContext,
                new TestClock(FixedUtcNow));

        DeactivateProductCommand command =
            new(
                OrganisationId: Guid.Empty,
                ProductId: Guid.NewGuid(),
                ModifiedBy: Guid.NewGuid(),
                RowVersion: [1]);

        RequestValidationException exception =
            await Assert.ThrowsAsync<RequestValidationException>(
                () => handler.HandleAsync(
                    command,
                    CancellationToken.None));

        Assert.Equal(
            "DEACTIVATE_PRODUCT_ORGANISATION_REQUIRED",
            exception.Code);

        Assert.Equal(
            "Organisation is required.",
            exception.Message);
    }

    [Fact]
    public async Task HandleAsync_WithEmptyProductId_ThrowsRequestValidationException()
    {
        using IServiceScope scope =
            factory.Services.CreateScope();

        CatalogDbContext dbContext =
            scope.ServiceProvider
                .GetRequiredService<CatalogDbContext>();

        DeactivateProductHandler handler =
            new(
                dbContext,
                new TestClock(FixedUtcNow));

        DeactivateProductCommand command =
            new(
                OrganisationId: Guid.NewGuid(),
                ProductId: Guid.Empty,
                ModifiedBy: Guid.NewGuid(),
                RowVersion: [1]);

        RequestValidationException exception =
            await Assert.ThrowsAsync<RequestValidationException>(
                () => handler.HandleAsync(
                    command,
                    CancellationToken.None));

        Assert.Equal(
            "DEACTIVATE_PRODUCT_ID_REQUIRED",
            exception.Code);

        Assert.Equal(
            "Product ID is required.",
            exception.Message);
    }

    [Fact]
    public async Task HandleAsync_WithEmptyModifiedBy_ThrowsRequestValidationException()
    {
        using IServiceScope scope =
            factory.Services.CreateScope();

        CatalogDbContext dbContext =
            scope.ServiceProvider
                .GetRequiredService<CatalogDbContext>();

        DeactivateProductHandler handler =
            new(
                dbContext,
                new TestClock(FixedUtcNow));

        DeactivateProductCommand command =
            new(
                OrganisationId: Guid.NewGuid(),
                ProductId: Guid.NewGuid(),
                ModifiedBy: Guid.Empty,
                RowVersion: [1]);

        RequestValidationException exception =
            await Assert.ThrowsAsync<RequestValidationException>(
                () => handler.HandleAsync(
                    command,
                    CancellationToken.None));

        Assert.Equal(
            "DEACTIVATE_PRODUCT_MODIFIED_BY_REQUIRED",
            exception.Code);

        Assert.Equal(
            "Modified by is required.",
            exception.Message);
    }

    [Fact]
    public async Task HandleAsync_WithNullRowVersion_ThrowsRequestValidationException()
    {
        using IServiceScope scope =
            factory.Services.CreateScope();

        CatalogDbContext dbContext =
            scope.ServiceProvider
                .GetRequiredService<CatalogDbContext>();

        DeactivateProductHandler handler =
            new(
                dbContext,
                new TestClock(FixedUtcNow));

        DeactivateProductCommand command =
            new(
                OrganisationId: Guid.NewGuid(),
                ProductId: Guid.NewGuid(),
                ModifiedBy: Guid.NewGuid(),
                RowVersion: null!);

        RequestValidationException exception =
            await Assert.ThrowsAsync<RequestValidationException>(
                () => handler.HandleAsync(
                    command,
                    CancellationToken.None));

        Assert.Equal(
            "DEACTIVATE_PRODUCT_ROW_VERSION_REQUIRED",
            exception.Code);

        Assert.Equal(
            "Product row version is required.",
            exception.Message);
    }

    [Fact]
    public async Task HandleAsync_WithEmptyRowVersion_ThrowsRequestValidationException()
    {
        using IServiceScope scope =
            factory.Services.CreateScope();

        CatalogDbContext dbContext =
            scope.ServiceProvider
                .GetRequiredService<CatalogDbContext>();

        DeactivateProductHandler handler =
            new(
                dbContext,
                new TestClock(FixedUtcNow));

        DeactivateProductCommand command =
            new(
                OrganisationId: Guid.NewGuid(),
                ProductId: Guid.NewGuid(),
                ModifiedBy: Guid.NewGuid(),
                RowVersion: Array.Empty<byte>());

        RequestValidationException exception =
            await Assert.ThrowsAsync<RequestValidationException>(
                () => handler.HandleAsync(
                    command,
                    CancellationToken.None));

        Assert.Equal(
            "DEACTIVATE_PRODUCT_ROW_VERSION_REQUIRED",
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

        DeactivateProductHandler handler =
            new(
                dbContext,
                new TestClock(FixedUtcNow));

        ArgumentNullException exception =
            await Assert.ThrowsAsync<ArgumentNullException>(
                () => handler.HandleAsync(
                    null!,
                    CancellationToken.None));

        Assert.Equal(
            "command",
            exception.ParamName);
    }

    private static Product CreateProduct(
        Guid organisationId,
        Guid createdBy)
    {
        return Product.Create(
            organisationId: organisationId,
            sku: $"PRODUCT-{Guid.NewGuid():N}",
            name: "Office Chair",
            description: "Ergonomic office chair.",
            unitOfMeasureId: EachUnitOfMeasureId,
            productCategoryId: null,
            taxCategoryId: StandardGstTaxCategoryId,
            createdAt: FixedUtcNow.AddDays(-1),
            createdBy: createdBy);
    }

    private static async Task DeleteProductAsync(
        CatalogDbContext dbContext,
        Guid productId)
    {
        dbContext.ChangeTracker.Clear();

        await dbContext.Products
            .Where(product =>
                product.Id == productId)
            .ExecuteDeleteAsync();
    }

    private sealed class TestClock(
        DateTimeOffset utcNow)
        : IClock
    {
        public DateTimeOffset UtcNow { get; } =
            utcNow;
    }
}
