using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TradeFlow.BuildingBlocks.Exceptions;
using TradeFlow.Modules.Catalog.Application.Products.ListProducts;
using TradeFlow.Modules.Catalog.Domain.Products;
using TradeFlow.Modules.Catalog.Infrastructure.Persistence;

namespace TradeFlow.IntegrationTests.Catalog.Products;

public sealed class ListProductsHandlerTests(
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
    public async Task HandleAsync_ReturnsOnlyProductsFromRequestedOrganisation()
    {
        using IServiceScope scope =
            factory.Services.CreateScope();

        CatalogDbContext dbContext =
            scope.ServiceProvider
                .GetRequiredService<CatalogDbContext>();

        await dbContext.Database.MigrateAsync();

        ListProductsHandler handler =
            scope.ServiceProvider
                .GetRequiredService<ListProductsHandler>();

        Guid organisationId = Guid.NewGuid();
        Guid otherOrganisationId = Guid.NewGuid();
        string suffix = CreateSuffix();

        Product expectedProduct =
            CreateProduct(
                organisationId,
                $"CHAIR-{suffix}",
                "Office Chair");

        Product otherProduct =
            CreateProduct(
                otherOrganisationId,
                $"DESK-{suffix}",
                "Office Desk");

        dbContext.Products.AddRange(
            expectedProduct,
            otherProduct);

        await dbContext.SaveChangesAsync();

        try
        {
            dbContext.ChangeTracker.Clear();

            ListProductsQuery query =
                new(
                    OrganisationId: organisationId,
                    Search: null,
                    Status: null,
                    PageNumber: 1,
                    PageSize: 20);

            ListProductsResult result =
                await handler.HandleAsync(
                    query,
                    CancellationToken.None);

            Assert.Equal(1, result.TotalCount);
            Assert.Equal(1, result.TotalPages);
            Assert.Equal(1, result.PageNumber);
            Assert.Equal(20, result.PageSize);

            ListProductItem item =
                Assert.Single(result.Items);

            Assert.Equal(
                expectedProduct.Id,
                item.ProductId);

            Assert.Equal(
                expectedProduct.Sku,
                item.Sku);

            Assert.Equal(
                expectedProduct.Name,
                item.Name);

            Assert.Equal(
                expectedProduct.UnitOfMeasureId,
                item.UnitOfMeasureId);

            Assert.Equal(
                expectedProduct.ProductCategoryId,
                item.ProductCategoryId);

            Assert.Equal(
                expectedProduct.TaxCategoryId,
                item.TaxCategoryId);

            Assert.Equal(
                expectedProduct.Status,
                item.Status);

            Assert.Equal(
                expectedProduct.CreatedAt,
                item.CreatedAt);

            Assert.Equal(
                expectedProduct.LastModifiedAt,
                item.LastModifiedAt);

            Assert.Empty(
                dbContext.ChangeTracker.Entries<Product>());
        }
        finally
        {
            await DeleteProductsAsync(
                dbContext,
                organisationId,
                otherOrganisationId);
        }
    }

    [Fact]
    public async Task HandleAsync_WithSearch_MatchesSkuOrName()
    {
        using IServiceScope scope =
            factory.Services.CreateScope();

        CatalogDbContext dbContext =
            scope.ServiceProvider
                .GetRequiredService<CatalogDbContext>();

        await dbContext.Database.MigrateAsync();

        ListProductsHandler handler =
            scope.ServiceProvider
                .GetRequiredService<ListProductsHandler>();

        Guid organisationId = Guid.NewGuid();
        Guid otherOrganisationId = Guid.NewGuid();
        string suffix = CreateSuffix();

        Product skuMatch =
            CreateProduct(
                organisationId,
                $"CHAIR-{suffix}",
                "Conference Furniture");

        Product nameMatch =
            CreateProduct(
                organisationId,
                $"DESK-{suffix}",
                "Office Chair");

        Product noMatch =
            CreateProduct(
                organisationId,
                $"MONITOR-{suffix}",
                "Computer Monitor");

        Product otherOrganisationMatch =
            CreateProduct(
                otherOrganisationId,
                $"OTHER-CHAIR-{suffix}",
                "Office Chair");

        dbContext.Products.AddRange(
            skuMatch,
            nameMatch,
            noMatch,
            otherOrganisationMatch);

        await dbContext.SaveChangesAsync();

        try
        {
            dbContext.ChangeTracker.Clear();

            ListProductsQuery query =
                new(
                    OrganisationId: organisationId,
                    Search: " chair ",
                    Status: null,
                    PageNumber: 1,
                    PageSize: 20);

            ListProductsResult result =
                await handler.HandleAsync(
                    query,
                    CancellationToken.None);

            Assert.Equal(2, result.TotalCount);
            Assert.Equal(2, result.Items.Count);

            Assert.Contains(
                result.Items,
                item =>
                    item.ProductId == skuMatch.Id);

            Assert.Contains(
                result.Items,
                item =>
                    item.ProductId == nameMatch.Id);

            Assert.DoesNotContain(
                result.Items,
                item =>
                    item.ProductId == noMatch.Id);

            Assert.DoesNotContain(
                result.Items,
                item =>
                    item.ProductId ==
                    otherOrganisationMatch.Id);
        }
        finally
        {
            await DeleteProductsAsync(
                dbContext,
                organisationId,
                otherOrganisationId);
        }
    }

    [Fact]
    public async Task HandleAsync_WithInactiveStatus_ReturnsOnlyInactiveProducts()
    {
        using IServiceScope scope =
            factory.Services.CreateScope();

        CatalogDbContext dbContext =
            scope.ServiceProvider
                .GetRequiredService<CatalogDbContext>();

        await dbContext.Database.MigrateAsync();

        ListProductsHandler handler =
            scope.ServiceProvider
                .GetRequiredService<ListProductsHandler>();

        Guid organisationId = Guid.NewGuid();
        string suffix = CreateSuffix();

        Product activeProduct =
            CreateProduct(
                organisationId,
                $"ACTIVE-{suffix}",
                "Active Product");

        Product inactiveProduct =
            CreateProduct(
                organisationId,
                $"INACTIVE-{suffix}",
                "Inactive Product");

        inactiveProduct.Deactivate(
            DateTimeOffset.UtcNow,
            Guid.NewGuid());

        dbContext.Products.AddRange(
            activeProduct,
            inactiveProduct);

        await dbContext.SaveChangesAsync();

        try
        {
            dbContext.ChangeTracker.Clear();

            ListProductsQuery query =
                new(
                    OrganisationId: organisationId,
                    Search: null,
                    Status: ProductStatus.Inactive,
                    PageNumber: 1,
                    PageSize: 20);

            ListProductsResult result =
                await handler.HandleAsync(
                    query,
                    CancellationToken.None);

            Assert.Equal(1, result.TotalCount);

            ListProductItem item =
                Assert.Single(result.Items);

            Assert.Equal(
                inactiveProduct.Id,
                item.ProductId);

            Assert.Equal(
                ProductStatus.Inactive,
                item.Status);
        }
        finally
        {
            await DeleteProductsAsync(
                dbContext,
                organisationId);
        }
    }

    [Fact]
    public async Task HandleAsync_WithSecondPage_ReturnsStableSortedPage()
    {
        using IServiceScope scope =
            factory.Services.CreateScope();

        CatalogDbContext dbContext =
            scope.ServiceProvider
                .GetRequiredService<CatalogDbContext>();

        await dbContext.Database.MigrateAsync();

        ListProductsHandler handler =
            scope.ServiceProvider
                .GetRequiredService<ListProductsHandler>();

        Guid organisationId = Guid.NewGuid();
        string suffix = CreateSuffix();

        Product productD =
            CreateProduct(
                organisationId,
                $"PAGE-{suffix}-D",
                "Product D");

        Product productA =
            CreateProduct(
                organisationId,
                $"PAGE-{suffix}-A",
                "Product A");

        Product productE =
            CreateProduct(
                organisationId,
                $"PAGE-{suffix}-E",
                "Product E");

        Product productC =
            CreateProduct(
                organisationId,
                $"PAGE-{suffix}-C",
                "Product C");

        Product productB =
            CreateProduct(
                organisationId,
                $"PAGE-{suffix}-B",
                "Product B");

        dbContext.Products.AddRange(
            productD,
            productA,
            productE,
            productC,
            productB);

        await dbContext.SaveChangesAsync();

        try
        {
            dbContext.ChangeTracker.Clear();

            ListProductsQuery query =
                new(
                    OrganisationId: organisationId,
                    Search: null,
                    Status: null,
                    PageNumber: 2,
                    PageSize: 2);

            ListProductsResult result =
                await handler.HandleAsync(
                    query,
                    CancellationToken.None);

            Assert.Equal(5, result.TotalCount);
            Assert.Equal(3, result.TotalPages);
            Assert.Equal(2, result.PageNumber);
            Assert.Equal(2, result.PageSize);
            Assert.Equal(2, result.Items.Count);

            Assert.Equal(
                productC.Sku,
                result.Items[0].Sku);

            Assert.Equal(
                productD.Sku,
                result.Items[1].Sku);
        }
        finally
        {
            await DeleteProductsAsync(
                dbContext,
                organisationId);
        }
    }

    [Fact]
    public async Task HandleAsync_WithNoMatchingProducts_ReturnsEmptyPage()
    {
        using IServiceScope scope =
            factory.Services.CreateScope();

        CatalogDbContext dbContext =
            scope.ServiceProvider
                .GetRequiredService<CatalogDbContext>();

        await dbContext.Database.MigrateAsync();

        ListProductsHandler handler =
            scope.ServiceProvider
                .GetRequiredService<ListProductsHandler>();

        ListProductsQuery query =
            new(
                OrganisationId: Guid.NewGuid(),
                Search: null,
                Status: null,
                PageNumber: 1,
                PageSize: 20);

        ListProductsResult result =
            await handler.HandleAsync(
                query,
                CancellationToken.None);

        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalCount);
        Assert.Equal(0, result.TotalPages);
        Assert.Equal(1, result.PageNumber);
        Assert.Equal(20, result.PageSize);
    }

    [Fact]
    public async Task HandleAsync_WithEmptyOrganisationId_ThrowsRequestValidationException()
    {
        using IServiceScope scope =
            factory.Services.CreateScope();

        ListProductsHandler handler =
            scope.ServiceProvider
                .GetRequiredService<ListProductsHandler>();

        ListProductsQuery query =
            new(
                OrganisationId: Guid.Empty,
                Search: null,
                Status: null,
                PageNumber: 1,
                PageSize: 20);

        RequestValidationException exception =
            await Assert.ThrowsAsync<RequestValidationException>(
                () => handler.HandleAsync(
                    query,
                    CancellationToken.None));

        Assert.Equal(
            "LIST_PRODUCTS_ORGANISATION_REQUIRED",
            exception.Code);

        Assert.Equal(
            "Organisation is required.",
            exception.Message);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task HandleAsync_WithInvalidPageNumber_ThrowsRequestValidationException(
        int pageNumber)
    {
        using IServiceScope scope =
            factory.Services.CreateScope();

        ListProductsHandler handler =
            scope.ServiceProvider
                .GetRequiredService<ListProductsHandler>();

        ListProductsQuery query =
            new(
                OrganisationId: Guid.NewGuid(),
                Search: null,
                Status: null,
                PageNumber: pageNumber,
                PageSize: 20);

        RequestValidationException exception =
            await Assert.ThrowsAsync<RequestValidationException>(
                () => handler.HandleAsync(
                    query,
                    CancellationToken.None));

        Assert.Equal(
            "LIST_PRODUCTS_PAGE_NUMBER_INVALID",
            exception.Code);

        Assert.Equal(
            "Page number must be greater than or equal to 1.",
            exception.Message);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(101)]
    public async Task HandleAsync_WithInvalidPageSize_ThrowsRequestValidationException(
        int pageSize)
    {
        using IServiceScope scope =
            factory.Services.CreateScope();

        ListProductsHandler handler =
            scope.ServiceProvider
                .GetRequiredService<ListProductsHandler>();

        ListProductsQuery query =
            new(
                OrganisationId: Guid.NewGuid(),
                Search: null,
                Status: null,
                PageNumber: 1,
                PageSize: pageSize);

        RequestValidationException exception =
            await Assert.ThrowsAsync<RequestValidationException>(
                () => handler.HandleAsync(
                    query,
                    CancellationToken.None));

        Assert.Equal(
            "LIST_PRODUCTS_PAGE_SIZE_INVALID",
            exception.Code);

        Assert.Equal(
            "Page size must be between 1 and 100.",
            exception.Message);
    }

    [Fact]
    public async Task HandleAsync_WithSearchOverMaximumLength_ThrowsRequestValidationException()
    {
        using IServiceScope scope =
            factory.Services.CreateScope();

        ListProductsHandler handler =
            scope.ServiceProvider
                .GetRequiredService<ListProductsHandler>();

        ListProductsQuery query =
            new(
                OrganisationId: Guid.NewGuid(),
                Search: new string('A', 101),
                Status: null,
                PageNumber: 1,
                PageSize: 20);

        RequestValidationException exception =
            await Assert.ThrowsAsync<RequestValidationException>(
                () => handler.HandleAsync(
                    query,
                    CancellationToken.None));

        Assert.Equal(
            "LIST_PRODUCTS_SEARCH_INVALID",
            exception.Code);

        Assert.Equal(
            "Search must not exceed 100 characters.",
            exception.Message);
    }

    [Fact]
    public async Task HandleAsync_WithNullQuery_ThrowsArgumentNullException()
    {
        using IServiceScope scope =
            factory.Services.CreateScope();

        ListProductsHandler handler =
            scope.ServiceProvider
                .GetRequiredService<ListProductsHandler>();

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
        string sku,
        string name)
    {
        return Product.Create(
            organisationId: organisationId,
            sku: sku,
            name: name,
            description: string.Empty,
            unitOfMeasureId: EachUnitOfMeasureId,
            productCategoryId: null,
            taxCategoryId: StandardGstTaxCategoryId,
            createdAt:
                DateTimeOffset.UtcNow.AddMinutes(-5),
            createdBy: Guid.NewGuid());
    }

    private static string CreateSuffix()
    {
        return Guid.NewGuid()
            .ToString("N")[..8]
            .ToUpperInvariant();
    }

    private static async Task DeleteProductsAsync(
        CatalogDbContext dbContext,
        params Guid[] organisationIds)
    {
        await dbContext.Products
            .Where(product =>
                organisationIds.Contains(
                    product.OrganisationId))
            .ExecuteDeleteAsync();
    }
}
