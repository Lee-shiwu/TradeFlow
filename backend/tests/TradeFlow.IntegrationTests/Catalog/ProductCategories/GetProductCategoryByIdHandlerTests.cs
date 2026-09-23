using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TradeFlow.BuildingBlocks.Exceptions;
using TradeFlow.Modules.Catalog.Application.ProductCategories.GetProductCategoryById;
using TradeFlow.Modules.Catalog.Domain.ProductCategories;
using TradeFlow.Modules.Catalog.Infrastructure.Persistence;

namespace TradeFlow.IntegrationTests.Catalog.ProductCategories;

public sealed class GetProductCategoryByIdHandlerTests(ApiFactory factory)
    : IClassFixture<ApiFactory>
{
    [Fact]
    public async Task HandleAsync_WithExistingCategory_ReturnsCategoryDetails()
    {
        using IServiceScope scope = factory.Services.CreateScope();
        CatalogDbContext dbContext =
            scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        GetProductCategoryByIdHandler handler =
            scope.ServiceProvider
                .GetRequiredService<GetProductCategoryByIdHandler>();

        await dbContext.Database.MigrateAsync();

        Guid organisationId = Guid.NewGuid();
        DateTimeOffset createdAt =
            new(2026, 9, 20, 10, 0, 0, TimeSpan.Zero);
        Guid createdBy = Guid.NewGuid();
        DateTimeOffset modifiedAt = createdAt.AddHours(1);
        Guid modifiedBy = Guid.NewGuid();

        ProductCategory category =
            ProductCategory.Create(
                organisationId,
                $"DETAIL-{CreateSuffix()}",
                "Office products",
                "Products used in the office.",
                createdAt,
                createdBy);

        category.Deactivate(modifiedAt, modifiedBy);

        dbContext.ProductCategories.Add(category);
        await dbContext.SaveChangesAsync();

        try
        {
            dbContext.ChangeTracker.Clear();

            GetProductCategoryByIdResult result =
                await handler.HandleAsync(
                    new GetProductCategoryByIdQuery(
                        OrganisationId: organisationId,
                        ProductCategoryId: category.Id),
                    CancellationToken.None);

            Assert.Equal(category.Id, result.ProductCategoryId);
            Assert.Equal(category.Code, result.Code);
            Assert.Equal(category.Name, result.Name);
            Assert.Equal(category.Description, result.Description);
            Assert.Equal(ProductCategoryStatus.Inactive, result.Status);
            Assert.Equal(createdAt, result.CreatedAt);
            Assert.Equal(createdBy, result.CreatedBy);
            Assert.Equal(modifiedAt, result.LastModifiedAt);
            Assert.Equal(modifiedBy, result.LastModifiedBy);
            Assert.NotEmpty(result.RowVersion);
            Assert.Empty(dbContext.ChangeTracker.Entries<ProductCategory>());
        }
        finally
        {
            await DeleteCategoryAsync(dbContext, category.Id);
        }
    }

    [Fact]
    public async Task HandleAsync_WithUnknownCategory_ThrowsNotFoundException()
    {
        using IServiceScope scope = factory.Services.CreateScope();
        GetProductCategoryByIdHandler handler =
            scope.ServiceProvider
                .GetRequiredService<GetProductCategoryByIdHandler>();

        NotFoundException exception =
            await Assert.ThrowsAsync<NotFoundException>(
                () => handler.HandleAsync(
                    new GetProductCategoryByIdQuery(
                        OrganisationId: Guid.NewGuid(),
                        ProductCategoryId: Guid.NewGuid()),
                    CancellationToken.None));

        Assert.Equal("PRODUCT_CATEGORY_NOT_FOUND", exception.Code);
        Assert.Equal(
            "The requested product category was not found.",
            exception.Message);
    }

    [Fact]
    public async Task HandleAsync_WithCategoryFromAnotherOrganisation_ThrowsNotFoundException()
    {
        using IServiceScope scope = factory.Services.CreateScope();
        CatalogDbContext dbContext =
            scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        GetProductCategoryByIdHandler handler =
            scope.ServiceProvider
                .GetRequiredService<GetProductCategoryByIdHandler>();

        await dbContext.Database.MigrateAsync();

        ProductCategory category =
            CreateCategory(Guid.NewGuid());

        dbContext.ProductCategories.Add(category);
        await dbContext.SaveChangesAsync();

        try
        {
            dbContext.ChangeTracker.Clear();

            NotFoundException exception =
                await Assert.ThrowsAsync<NotFoundException>(
                    () => handler.HandleAsync(
                        new GetProductCategoryByIdQuery(
                            OrganisationId: Guid.NewGuid(),
                            ProductCategoryId: category.Id),
                        CancellationToken.None));

            Assert.Equal("PRODUCT_CATEGORY_NOT_FOUND", exception.Code);
        }
        finally
        {
            await DeleteCategoryAsync(dbContext, category.Id);
        }
    }

    [Fact]
    public async Task HandleAsync_WithNullQuery_ThrowsArgumentNullException()
    {
        using IServiceScope scope = factory.Services.CreateScope();
        GetProductCategoryByIdHandler handler =
            scope.ServiceProvider
                .GetRequiredService<GetProductCategoryByIdHandler>();

        ArgumentNullException exception =
            await Assert.ThrowsAsync<ArgumentNullException>(
                () => handler.HandleAsync(null!, CancellationToken.None));

        Assert.Equal("query", exception.ParamName);
    }

    private static ProductCategory CreateCategory(Guid organisationId)
    {
        return ProductCategory.Create(
            organisationId,
            $"DETAIL-{CreateSuffix()}",
            "Office products",
            "Products used in the office.",
            DateTimeOffset.UtcNow.AddMinutes(-5),
            Guid.NewGuid());
    }

    private static string CreateSuffix()
    {
        return Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();
    }

    private static async Task DeleteCategoryAsync(
        CatalogDbContext dbContext,
        Guid productCategoryId)
    {
        await dbContext.ProductCategories
            .Where(category => category.Id == productCategoryId)
            .ExecuteDeleteAsync();
    }
}
