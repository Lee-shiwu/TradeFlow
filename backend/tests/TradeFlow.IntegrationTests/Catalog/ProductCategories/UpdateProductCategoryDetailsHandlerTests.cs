using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TradeFlow.BuildingBlocks.Exceptions;
using TradeFlow.BuildingBlocks.Time;
using TradeFlow.Modules.Catalog.Application.ProductCategories.UpdateProductCategoryDetails;
using TradeFlow.Modules.Catalog.Domain.ProductCategories;
using TradeFlow.Modules.Catalog.Infrastructure.Persistence;

namespace TradeFlow.IntegrationTests.Catalog.ProductCategories;

public sealed class UpdateProductCategoryDetailsHandlerTests(ApiFactory factory)
    : IClassFixture<ApiFactory>
{
    private static readonly DateTimeOffset FixedUtcNow =
        new(2026, 9, 23, 10, 30, 0, TimeSpan.Zero);

    [Fact]
    public async Task HandleAsync_WithValidCommand_UpdatesDetailsAndAudit()
    {
        using IServiceScope scope = factory.Services.CreateScope();
        CatalogDbContext dbContext =
            scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        await dbContext.Database.MigrateAsync();

        Guid organisationId = Guid.NewGuid();
        Guid modifiedBy = Guid.NewGuid();
        ProductCategory category = CreateCategory(organisationId);
        dbContext.ProductCategories.Add(category);
        await dbContext.SaveChangesAsync();
        byte[] originalRowVersion = category.RowVersion.ToArray();

        try
        {
            dbContext.ChangeTracker.Clear();
            UpdateProductCategoryDetailsHandler handler =
                new(dbContext, new TestClock(FixedUtcNow));

            UpdateProductCategoryDetailsResult result =
                await handler.HandleAsync(
                    CreateCommand(category, organisationId, modifiedBy, originalRowVersion)
                        with
                        {
                            Name = "  Updated   category  ",
                            Description = "  Updated description.  "
                        },
                    CancellationToken.None);

            Assert.Equal(category.Id, result.ProductCategoryId);
            Assert.Equal(category.Code, result.Code);
            Assert.Equal("Updated category", result.Name);
            Assert.Equal("Updated description.", result.Description);
            Assert.Equal(FixedUtcNow, result.LastModifiedAt);
            Assert.Equal(modifiedBy, result.LastModifiedBy);
            Assert.False(originalRowVersion.SequenceEqual(result.RowVersion));

            dbContext.ChangeTracker.Clear();
            ProductCategory saved = await dbContext.ProductCategories
                .AsNoTracking()
                .SingleAsync(existing => existing.Id == category.Id);
            Assert.Equal(result.Name, saved.Name);
            Assert.Equal(result.Description, saved.Description);
            Assert.Equal(result.RowVersion, saved.RowVersion);
        }
        finally
        {
            await DeleteCategoriesAsync(dbContext, organisationId);
        }
    }

    [Fact]
    public async Task HandleAsync_WithNoDetailChanges_PreservesAuditAndRowVersion()
    {
        using IServiceScope scope = factory.Services.CreateScope();
        CatalogDbContext dbContext =
            scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        await dbContext.Database.MigrateAsync();

        Guid organisationId = Guid.NewGuid();
        ProductCategory category = CreateCategory(organisationId);
        dbContext.ProductCategories.Add(category);
        await dbContext.SaveChangesAsync();
        byte[] originalRowVersion = category.RowVersion.ToArray();

        try
        {
            dbContext.ChangeTracker.Clear();
            UpdateProductCategoryDetailsHandler handler =
                new(dbContext, new TestClock(FixedUtcNow));

            UpdateProductCategoryDetailsResult result =
                await handler.HandleAsync(
                    CreateCommand(
                        category,
                        organisationId,
                        Guid.NewGuid(),
                        originalRowVersion),
                    CancellationToken.None);

            Assert.Null(result.LastModifiedAt);
            Assert.Null(result.LastModifiedBy);
            Assert.Equal(originalRowVersion, result.RowVersion);
        }
        finally
        {
            await DeleteCategoriesAsync(dbContext, organisationId);
        }
    }

    [Fact]
    public async Task HandleAsync_WithStaleRowVersion_ThrowsConcurrencyConflict()
    {
        using IServiceScope scope = factory.Services.CreateScope();
        CatalogDbContext dbContext =
            scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        await dbContext.Database.MigrateAsync();

        Guid organisationId = Guid.NewGuid();
        ProductCategory category = CreateCategory(organisationId);
        dbContext.ProductCategories.Add(category);
        await dbContext.SaveChangesAsync();

        try
        {
            dbContext.ChangeTracker.Clear();
            UpdateProductCategoryDetailsHandler handler =
                new(dbContext, new TestClock(FixedUtcNow));

            BusinessRuleException exception =
                await Assert.ThrowsAsync<BusinessRuleException>(() =>
                    handler.HandleAsync(
                        CreateCommand(
                            category,
                            organisationId,
                            Guid.NewGuid(),
                            new byte[8]),
                        CancellationToken.None));

            Assert.Equal(
                "UPDATE_PRODUCT_CATEGORY_CONCURRENCY_CONFLICT",
                exception.Code);
        }
        finally
        {
            await DeleteCategoriesAsync(dbContext, organisationId);
        }
    }

    [Fact]
    public async Task HandleAsync_WithCategoryFromAnotherOrganisation_ThrowsNotFound()
    {
        using IServiceScope scope = factory.Services.CreateScope();
        CatalogDbContext dbContext =
            scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        await dbContext.Database.MigrateAsync();

        Guid ownerOrganisationId = Guid.NewGuid();
        ProductCategory category = CreateCategory(ownerOrganisationId);
        dbContext.ProductCategories.Add(category);
        await dbContext.SaveChangesAsync();

        try
        {
            dbContext.ChangeTracker.Clear();
            UpdateProductCategoryDetailsHandler handler =
                new(dbContext, new TestClock(FixedUtcNow));

            NotFoundException exception =
                await Assert.ThrowsAsync<NotFoundException>(() =>
                    handler.HandleAsync(
                        CreateCommand(
                            category,
                            Guid.NewGuid(),
                            Guid.NewGuid(),
                            category.RowVersion),
                        CancellationToken.None));

            Assert.Equal("UPDATE_PRODUCT_CATEGORY_NOT_FOUND", exception.Code);
        }
        finally
        {
            await DeleteCategoriesAsync(dbContext, ownerOrganisationId);
        }
    }

    [Fact]
    public async Task HandleAsync_WithUnknownCategory_ThrowsNotFound()
    {
        using IServiceScope scope = factory.Services.CreateScope();
        CatalogDbContext dbContext =
            scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        UpdateProductCategoryDetailsHandler handler =
            new(dbContext, new TestClock(FixedUtcNow));

        NotFoundException exception =
            await Assert.ThrowsAsync<NotFoundException>(() =>
                handler.HandleAsync(
                    new UpdateProductCategoryDetailsCommand(
                        Guid.NewGuid(),
                        Guid.NewGuid(),
                        "Unknown",
                        null,
                        Guid.NewGuid(),
                        new byte[8]),
                    CancellationToken.None));

        Assert.Equal("UPDATE_PRODUCT_CATEGORY_NOT_FOUND", exception.Code);
    }

    [Fact]
    public async Task HandleAsync_WithEmptyRowVersion_ThrowsRequestValidation()
    {
        using IServiceScope scope = factory.Services.CreateScope();
        CatalogDbContext dbContext =
            scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        UpdateProductCategoryDetailsHandler handler =
            new(dbContext, new TestClock(FixedUtcNow));

        RequestValidationException exception =
            await Assert.ThrowsAsync<RequestValidationException>(() =>
                handler.HandleAsync(
                    new UpdateProductCategoryDetailsCommand(
                        Guid.NewGuid(),
                        Guid.NewGuid(),
                        "Category",
                        null,
                        Guid.NewGuid(),
                        []),
                    CancellationToken.None));

        Assert.Equal("UPDATE_PRODUCT_CATEGORY_ROW_VERSION_REQUIRED", exception.Code);
    }

    private static ProductCategory CreateCategory(Guid organisationId)
    {
        return ProductCategory.Create(
            organisationId,
            $"EDIT-{Guid.NewGuid():N}"[..13].ToUpperInvariant(),
            "Existing category",
            "Existing description.",
            FixedUtcNow.AddDays(-1),
            Guid.NewGuid());
    }

    private static UpdateProductCategoryDetailsCommand CreateCommand(
        ProductCategory category,
        Guid organisationId,
        Guid modifiedBy,
        byte[] rowVersion)
    {
        return new UpdateProductCategoryDetailsCommand(
            organisationId,
            category.Id,
            category.Name,
            category.Description,
            modifiedBy,
            rowVersion);
    }

    private static async Task DeleteCategoriesAsync(
        CatalogDbContext dbContext,
        params Guid[] organisationIds)
    {
        dbContext.ChangeTracker.Clear();
        await dbContext.ProductCategories
            .Where(category => organisationIds.Contains(category.OrganisationId))
            .ExecuteDeleteAsync();
    }

    private sealed class TestClock(DateTimeOffset utcNow) : IClock
    {
        public DateTimeOffset UtcNow { get; } = utcNow;
    }
}
