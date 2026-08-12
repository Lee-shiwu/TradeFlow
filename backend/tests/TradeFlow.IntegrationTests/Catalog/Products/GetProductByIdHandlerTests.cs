using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TradeFlow.BuildingBlocks.Exceptions;
using TradeFlow.Modules.Catalog.Application.Products.GetProductById;
using TradeFlow.Modules.Catalog.Domain.Products;
using TradeFlow.Modules.Catalog.Infrastructure.Persistence;

namespace TradeFlow.IntegrationTests.Catalog.Products;

public sealed class GetProductByIdHandlerTests(
    ApiFactory factory)
    : IClassFixture<ApiFactory>
{
    private static readonly Guid EachUnitOfMeasureId =
        Guid.Parse(
            "10000000-0000-0000-0000-000000000001");

    private static readonly Guid StandardGstTaxCategoryId =
        Guid.Parse(
            "20000000-0000-0000-0000-000000000001");

    [Fact]
    public async Task HandleAsync_WithExistingProduct_ReturnsProduct()
    {
        using IServiceScope scope =
            factory.Services.CreateScope();

        CatalogDbContext dbContext =
            scope.ServiceProvider
                .GetRequiredService<CatalogDbContext>();

        await dbContext.Database.MigrateAsync();

        GetProductByIdHandler handler =
            scope.ServiceProvider
                .GetRequiredService<GetProductByIdHandler>();

        Guid organisationId = Guid.NewGuid();
        Guid createdBy = Guid.NewGuid();

        DateTimeOffset createdAt =
            new(
                year: 2026,
                month: 8,
                day: 11,
                hour: 10,
                minute: 0,
                second: 0,
                offset: TimeSpan.Zero);

        Product product =
            CreateProduct(
                organisationId,
                createdAt,
                createdBy);

        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync();

        try
        {
            dbContext.ChangeTracker.Clear();

            GetProductByIdQuery query =
                new(
                    ProductId: product.Id,
                    OrganisationId: organisationId);

            GetProductByIdResult result =
                await handler.HandleAsync(
                    query,
                    CancellationToken.None);

            Assert.Equal(product.Id, result.ProductId);
            Assert.Equal(product.Sku, result.Sku);
            Assert.Equal(product.Name, result.Name);
            Assert.Equal(
                product.Description,
                result.Description);

            Assert.Equal(
                product.UnitOfMeasureId,
                result.UnitOfMeasureId);

            Assert.Equal(
                product.ProductCategoryId,
                result.ProductCategoryId);

            Assert.Equal(
                product.TaxCategoryId,
                result.TaxCategoryId);

            Assert.Equal(product.Status, result.Status);
            Assert.Equal(product.CreatedAt, result.CreatedAt);
            Assert.Equal(product.CreatedBy, result.CreatedBy);

            Assert.Equal(
                product.LastModifiedAt,
                result.LastModifiedAt);

            Assert.Equal(
                product.LastModifiedBy,
                result.LastModifiedBy);

            Assert.NotEmpty(result.RowVersion);
            Assert.Equal(product.RowVersion, result.RowVersion);
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

        GetProductByIdHandler handler =
            scope.ServiceProvider
                .GetRequiredService<GetProductByIdHandler>();

        GetProductByIdQuery query =
            new(
                ProductId: Guid.NewGuid(),
                OrganisationId: Guid.NewGuid());

        NotFoundException exception =
            await Assert.ThrowsAsync<NotFoundException>(
                () => handler.HandleAsync(
                    query,
                    CancellationToken.None));

        Assert.Equal(
            "PRODUCT_NOT_FOUND",
            exception.Code);

        Assert.Equal(
            "The requested product was not found.",
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

        GetProductByIdHandler handler =
            scope.ServiceProvider
                .GetRequiredService<GetProductByIdHandler>();

        Guid owningOrganisationId = Guid.NewGuid();
        Guid otherOrganisationId = Guid.NewGuid();

        Product product =
            CreateProduct(
                owningOrganisationId,
                DateTimeOffset.UtcNow,
                Guid.NewGuid());

        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync();

        try
        {
            dbContext.ChangeTracker.Clear();

            GetProductByIdQuery query =
                new(
                    ProductId: product.Id,
                    OrganisationId: otherOrganisationId);

            NotFoundException exception =
                await Assert.ThrowsAsync<NotFoundException>(
                    () => handler.HandleAsync(
                        query,
                        CancellationToken.None));

            Assert.Equal(
                "PRODUCT_NOT_FOUND",
                exception.Code);

            Assert.Equal(
                "The requested product was not found.",
                exception.Message);
        }
        finally
        {
            await DeleteProductAsync(
                dbContext,
                product.Id);
        }
    }

    [Fact]
    public async Task HandleAsync_WithInactiveProduct_ReturnsProduct()
    {
        using IServiceScope scope =
            factory.Services.CreateScope();

        CatalogDbContext dbContext =
            scope.ServiceProvider
                .GetRequiredService<CatalogDbContext>();

        await dbContext.Database.MigrateAsync();

        GetProductByIdHandler handler =
            scope.ServiceProvider
                .GetRequiredService<GetProductByIdHandler>();

        Guid organisationId = Guid.NewGuid();
        Guid createdBy = Guid.NewGuid();
        Guid modifiedBy = Guid.NewGuid();

        DateTimeOffset createdAt =
            DateTimeOffset.UtcNow.AddMinutes(-2);

        DateTimeOffset modifiedAt =
            DateTimeOffset.UtcNow.AddMinutes(-1);

        Product product =
            CreateProduct(
                organisationId,
                createdAt,
                createdBy);

        product.Deactivate(
            modifiedAt,
            modifiedBy);

        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync();

        try
        {
            dbContext.ChangeTracker.Clear();

            GetProductByIdQuery query =
                new(
                    ProductId: product.Id,
                    OrganisationId: organisationId);

            GetProductByIdResult result =
                await handler.HandleAsync(
                    query,
                    CancellationToken.None);

            Assert.Equal(product.Id, result.ProductId);
            Assert.Equal(
                ProductStatus.Inactive,
                result.Status);

            Assert.Equal(
                modifiedAt.ToUniversalTime(),
                result.LastModifiedAt);

            Assert.Equal(
                modifiedBy,
                result.LastModifiedBy);
        }
        finally
        {
            await DeleteProductAsync(
                dbContext,
                product.Id);
        }
    }

    [Fact]
    public async Task HandleAsync_WithExistingProduct_DoesNotTrackProduct()
    {
        using IServiceScope scope =
            factory.Services.CreateScope();

        CatalogDbContext dbContext =
            scope.ServiceProvider
                .GetRequiredService<CatalogDbContext>();

        await dbContext.Database.MigrateAsync();

        GetProductByIdHandler handler =
            scope.ServiceProvider
                .GetRequiredService<GetProductByIdHandler>();

        Guid organisationId = Guid.NewGuid();

        Product product =
            CreateProduct(
                organisationId,
                DateTimeOffset.UtcNow,
                Guid.NewGuid());

        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync();

        try
        {
            dbContext.ChangeTracker.Clear();

            GetProductByIdQuery query =
                new(
                    ProductId: product.Id,
                    OrganisationId: organisationId);

            await handler.HandleAsync(
                query,
                CancellationToken.None);

            Assert.Empty(
                dbContext.ChangeTracker.Entries<Product>());
        }
        finally
        {
            await DeleteProductAsync(
                dbContext,
                product.Id);
        }
    }

    [Fact]
    public async Task HandleAsync_WithNullQuery_ThrowsArgumentNullException()
    {
        using IServiceScope scope =
            factory.Services.CreateScope();

        GetProductByIdHandler handler =
            scope.ServiceProvider
                .GetRequiredService<GetProductByIdHandler>();

        ArgumentNullException exception =
            await Assert.ThrowsAsync<ArgumentNullException>(
                () => handler.HandleAsync(
                    null!,
                    CancellationToken.None));

        Assert.Equal(
            "query",
            exception.ParamName);
    }

    private static Product CreateProduct(
        Guid organisationId,
        DateTimeOffset createdAt,
        Guid createdBy)
    {
        return Product.Create(
            organisationId: organisationId,
            sku: $"PRODUCT-{Guid.NewGuid():N}",
            name: "Office Chair",
            description: "Ergonomic office chair",
            unitOfMeasureId: EachUnitOfMeasureId,
            productCategoryId: null,
            taxCategoryId: StandardGstTaxCategoryId,
            createdAt: createdAt,
            createdBy: createdBy);
    }

    private static async Task DeleteProductAsync(
        CatalogDbContext dbContext,
        Guid productId)
    {
        await dbContext.Products
            .Where(product =>
                product.Id == productId)
            .ExecuteDeleteAsync();
    }
}
