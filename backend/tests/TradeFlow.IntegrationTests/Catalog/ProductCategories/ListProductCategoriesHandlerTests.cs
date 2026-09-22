using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TradeFlow.BuildingBlocks.Exceptions;
using TradeFlow.Modules.Catalog.Application.ProductCategories.ListProductCategories;
using TradeFlow.Modules.Catalog.Domain.ProductCategories;
using TradeFlow.Modules.Catalog.Infrastructure.Persistence;

namespace TradeFlow.IntegrationTests.Catalog.ProductCategories;

public sealed class ListProductCategoriesHandlerTests(ApiFactory factory)
    : IClassFixture<ApiFactory>
{
    [Fact]
    public async Task HandleAsync_AppliesOrganisationSearchStatusAndPagination()
    {
        using IServiceScope scope = factory.Services.CreateScope();
        CatalogDbContext dbContext =
            scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        ListProductCategoriesHandler handler =
            scope.ServiceProvider
                .GetRequiredService<ListProductCategoriesHandler>();

        await dbContext.Database.MigrateAsync();

        Guid organisationId = Guid.NewGuid();
        Guid otherOrganisationId = Guid.NewGuid();
        string suffix = CreateSuffix();

        ProductCategory matchA =
            CreateCategory(organisationId, $"CHAIR-{suffix}-A", "Furniture");
        ProductCategory matchB =
            CreateCategory(organisationId, $"OFFICE-{suffix}", "Chair parts");
        ProductCategory matchC =
            CreateCategory(organisationId, $"CHAIR-{suffix}-C", "Seating");
        ProductCategory inactive =
            CreateCategory(organisationId, $"CHAIR-{suffix}-OLD", "Old chairs");
        ProductCategory otherOrganisation =
            CreateCategory(otherOrganisationId, $"CHAIR-{suffix}-OTHER", "Other");

        inactive.Deactivate(DateTimeOffset.UtcNow, Guid.NewGuid());

        dbContext.ProductCategories.AddRange(
            matchC,
            inactive,
            matchA,
            otherOrganisation,
            matchB);
        await dbContext.SaveChangesAsync();

        try
        {
            dbContext.ChangeTracker.Clear();

            ListProductCategoriesResult result =
                await handler.HandleAsync(
                    new ListProductCategoriesQuery(
                        OrganisationId: organisationId,
                        Search: " chair ",
                        Status: ProductCategoryStatus.Active,
                        PageNumber: 2,
                        PageSize: 2),
                    CancellationToken.None);

            Assert.Equal(3, result.TotalCount);
            Assert.Equal(2, result.TotalPages);
            Assert.Equal(2, result.PageNumber);
            Assert.Equal(2, result.PageSize);

            ListProductCategoryItem item = Assert.Single(result.Items);
            Assert.Equal(matchB.Id, item.ProductCategoryId);
            Assert.Equal(matchB.Code, item.Code);
            Assert.Equal(matchB.Name, item.Name);
            Assert.Equal(matchB.Description, item.Description);
            Assert.Equal(ProductCategoryStatus.Active, item.Status);
            Assert.Equal(matchB.CreatedAt, item.CreatedAt);
            Assert.Null(item.LastModifiedAt);

            Assert.Empty(dbContext.ChangeTracker.Entries<ProductCategory>());
        }
        finally
        {
            await DeleteCategoriesAsync(
                dbContext,
                organisationId,
                otherOrganisationId);
        }
    }

    [Fact]
    public async Task HandleAsync_WithNoMatches_ReturnsEmptyPage()
    {
        using IServiceScope scope = factory.Services.CreateScope();
        ListProductCategoriesHandler handler =
            scope.ServiceProvider
                .GetRequiredService<ListProductCategoriesHandler>();

        ListProductCategoriesResult result =
            await handler.HandleAsync(
                ValidQuery() with { OrganisationId = Guid.NewGuid() },
                CancellationToken.None);

        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalCount);
        Assert.Equal(0, result.TotalPages);
    }

    [Fact]
    public async Task HandleAsync_WithNullQuery_ThrowsArgumentNullException()
    {
        using IServiceScope scope = factory.Services.CreateScope();
        ListProductCategoriesHandler handler =
            scope.ServiceProvider
                .GetRequiredService<ListProductCategoriesHandler>();

        ArgumentNullException exception =
            await Assert.ThrowsAsync<ArgumentNullException>(
                () => handler.HandleAsync(null!, CancellationToken.None));

        Assert.Equal("query", exception.ParamName);
    }

    [Fact]
    public async Task HandleAsync_WithEmptyOrganisation_ThrowsValidationException()
    {
        RequestValidationException exception =
            await AssertValidationExceptionAsync(
                ValidQuery() with { OrganisationId = Guid.Empty });

        Assert.Equal(
            "LIST_PRODUCT_CATEGORIES_ORGANISATION_REQUIRED",
            exception.Code);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task HandleAsync_WithInvalidPageNumber_ThrowsValidationException(
        int pageNumber)
    {
        RequestValidationException exception =
            await AssertValidationExceptionAsync(
                ValidQuery() with { PageNumber = pageNumber });

        Assert.Equal(
            "LIST_PRODUCT_CATEGORIES_PAGE_NUMBER_INVALID",
            exception.Code);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(101)]
    public async Task HandleAsync_WithInvalidPageSize_ThrowsValidationException(
        int pageSize)
    {
        RequestValidationException exception =
            await AssertValidationExceptionAsync(
                ValidQuery() with { PageSize = pageSize });

        Assert.Equal(
            "LIST_PRODUCT_CATEGORIES_PAGE_SIZE_INVALID",
            exception.Code);
    }

    [Fact]
    public async Task HandleAsync_WithLongSearch_ThrowsValidationException()
    {
        RequestValidationException exception =
            await AssertValidationExceptionAsync(
                ValidQuery() with { Search = new string('A', 101) });

        Assert.Equal(
            "LIST_PRODUCT_CATEGORIES_SEARCH_INVALID",
            exception.Code);
    }

    private async Task<RequestValidationException>
        AssertValidationExceptionAsync(ListProductCategoriesQuery query)
    {
        using IServiceScope scope = factory.Services.CreateScope();
        ListProductCategoriesHandler handler =
            scope.ServiceProvider
                .GetRequiredService<ListProductCategoriesHandler>();

        return await Assert.ThrowsAsync<RequestValidationException>(
            () => handler.HandleAsync(query, CancellationToken.None));
    }

    private static ListProductCategoriesQuery ValidQuery()
    {
        return new ListProductCategoriesQuery(
            OrganisationId: Guid.NewGuid(),
            Search: null,
            Status: null,
            PageNumber: 1,
            PageSize: 20);
    }

    private static ProductCategory CreateCategory(
        Guid organisationId,
        string code,
        string name)
    {
        return ProductCategory.Create(
            organisationId: organisationId,
            code: code,
            name: name,
            description: $"Description for {name}",
            createdAt: DateTimeOffset.UtcNow.AddMinutes(-5),
            createdBy: Guid.NewGuid());
    }

    private static string CreateSuffix()
    {
        return Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();
    }

    private static async Task DeleteCategoriesAsync(
        CatalogDbContext dbContext,
        params Guid[] organisationIds)
    {
        await dbContext.ProductCategories
            .Where(category =>
                organisationIds.Contains(category.OrganisationId))
            .ExecuteDeleteAsync();
    }
}
