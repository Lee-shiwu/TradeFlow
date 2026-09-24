using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TradeFlow.BuildingBlocks.Exceptions;
using TradeFlow.BuildingBlocks.Time;
using TradeFlow.Modules.Catalog.Application.ProductCategories.ActivateProductCategory;
using TradeFlow.Modules.Catalog.Application.ProductCategories.DeactivateProductCategory;
using TradeFlow.Modules.Catalog.Domain.ProductCategories;
using TradeFlow.Modules.Catalog.Infrastructure.Persistence;

namespace TradeFlow.IntegrationTests.Catalog.ProductCategories;

public sealed class ProductCategoryStatusHandlerTests(ApiFactory factory)
    : IClassFixture<ApiFactory>
{
    private static readonly DateTimeOffset FixedUtcNow =
        new(2026, 9, 23, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Deactivate_WithActiveCategory_UpdatesStatusAndAudit()
    {
        using IServiceScope scope = factory.Services.CreateScope();
        CatalogDbContext dbContext = GetDbContext(scope);
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
            DeactivateProductCategoryHandler handler =
                new(dbContext, new TestClock(FixedUtcNow));

            DeactivateProductCategoryResult result =
                await handler.HandleAsync(
                    new DeactivateProductCategoryCommand(
                        organisationId,
                        category.Id,
                        modifiedBy,
                        originalRowVersion),
                    CancellationToken.None);

            Assert.Equal(ProductCategoryStatus.Inactive, result.Status);
            Assert.Equal(FixedUtcNow, result.LastModifiedAt);
            Assert.Equal(modifiedBy, result.LastModifiedBy);
            Assert.False(originalRowVersion.SequenceEqual(result.RowVersion));
        }
        finally
        {
            await DeleteCategoriesAsync(dbContext, organisationId);
        }
    }

    [Fact]
    public async Task Activate_WithInactiveCategory_UpdatesStatusAndAudit()
    {
        using IServiceScope scope = factory.Services.CreateScope();
        CatalogDbContext dbContext = GetDbContext(scope);
        await dbContext.Database.MigrateAsync();
        Guid organisationId = Guid.NewGuid();
        Guid modifiedBy = Guid.NewGuid();
        ProductCategory category = CreateCategory(organisationId, inactive: true);
        dbContext.ProductCategories.Add(category);
        await dbContext.SaveChangesAsync();
        byte[] originalRowVersion = category.RowVersion.ToArray();

        try
        {
            dbContext.ChangeTracker.Clear();
            ActivateProductCategoryHandler handler =
                new(dbContext, new TestClock(FixedUtcNow));

            ActivateProductCategoryResult result =
                await handler.HandleAsync(
                    new ActivateProductCategoryCommand(
                        organisationId,
                        category.Id,
                        modifiedBy,
                        originalRowVersion),
                    CancellationToken.None);

            Assert.Equal(ProductCategoryStatus.Active, result.Status);
            Assert.Equal(FixedUtcNow, result.LastModifiedAt);
            Assert.Equal(modifiedBy, result.LastModifiedBy);
            Assert.False(originalRowVersion.SequenceEqual(result.RowVersion));
        }
        finally
        {
            await DeleteCategoriesAsync(dbContext, organisationId);
        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task SameTargetStatus_IsIdempotent(bool activate)
    {
        using IServiceScope scope = factory.Services.CreateScope();
        CatalogDbContext dbContext = GetDbContext(scope);
        await dbContext.Database.MigrateAsync();
        Guid organisationId = Guid.NewGuid();
        ProductCategory category =
            CreateCategory(organisationId, inactive: !activate);
        dbContext.ProductCategories.Add(category);
        await dbContext.SaveChangesAsync();
        byte[] originalRowVersion = category.RowVersion.ToArray();
        DateTimeOffset? originalModifiedAt = category.LastModifiedAt;
        Guid? originalModifiedBy = category.LastModifiedBy;

        try
        {
            dbContext.ChangeTracker.Clear();

            if (activate)
            {
                ActivateProductCategoryResult result =
                    await new ActivateProductCategoryHandler(
                            dbContext,
                            new TestClock(FixedUtcNow))
                        .HandleAsync(
                            new ActivateProductCategoryCommand(
                                organisationId,
                                category.Id,
                                Guid.NewGuid(),
                                originalRowVersion),
                            CancellationToken.None);
                Assert.Equal(originalRowVersion, result.RowVersion);
                Assert.Equal(originalModifiedAt, result.LastModifiedAt);
                Assert.Equal(originalModifiedBy, result.LastModifiedBy);
            }
            else
            {
                DeactivateProductCategoryResult result =
                    await new DeactivateProductCategoryHandler(
                            dbContext,
                            new TestClock(FixedUtcNow))
                        .HandleAsync(
                            new DeactivateProductCategoryCommand(
                                organisationId,
                                category.Id,
                                Guid.NewGuid(),
                                originalRowVersion),
                            CancellationToken.None);
                Assert.Equal(originalRowVersion, result.RowVersion);
                Assert.Equal(originalModifiedAt, result.LastModifiedAt);
                Assert.Equal(originalModifiedBy, result.LastModifiedBy);
            }
        }
        finally
        {
            await DeleteCategoriesAsync(dbContext, organisationId);
        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task StaleRowVersion_ThrowsConcurrencyConflict(bool activate)
    {
        using IServiceScope scope = factory.Services.CreateScope();
        CatalogDbContext dbContext = GetDbContext(scope);
        await dbContext.Database.MigrateAsync();
        Guid organisationId = Guid.NewGuid();
        ProductCategory category =
            CreateCategory(organisationId, inactive: !activate);
        dbContext.ProductCategories.Add(category);
        await dbContext.SaveChangesAsync();

        try
        {
            dbContext.ChangeTracker.Clear();

            BusinessRuleException exception;
            if (activate)
            {
                exception = await Assert.ThrowsAsync<BusinessRuleException>(() =>
                    new ActivateProductCategoryHandler(
                            dbContext,
                            new TestClock(FixedUtcNow))
                        .HandleAsync(
                            new ActivateProductCategoryCommand(
                                organisationId,
                                category.Id,
                                Guid.NewGuid(),
                                new byte[8]),
                            CancellationToken.None));
                Assert.Equal(
                    "ACTIVATE_PRODUCT_CATEGORY_CONCURRENCY_CONFLICT",
                    exception.Code);
            }
            else
            {
                exception = await Assert.ThrowsAsync<BusinessRuleException>(() =>
                    new DeactivateProductCategoryHandler(
                            dbContext,
                            new TestClock(FixedUtcNow))
                        .HandleAsync(
                            new DeactivateProductCategoryCommand(
                                organisationId,
                                category.Id,
                                Guid.NewGuid(),
                                new byte[8]),
                            CancellationToken.None));
                Assert.Equal(
                    "DEACTIVATE_PRODUCT_CATEGORY_CONCURRENCY_CONFLICT",
                    exception.Code);
            }
        }
        finally
        {
            await DeleteCategoriesAsync(dbContext, organisationId);
        }
    }

    [Fact]
    public async Task Deactivate_WithCategoryFromAnotherOrganisation_ThrowsNotFound()
    {
        using IServiceScope scope = factory.Services.CreateScope();
        CatalogDbContext dbContext = GetDbContext(scope);
        await dbContext.Database.MigrateAsync();
        Guid ownerOrganisationId = Guid.NewGuid();
        ProductCategory category = CreateCategory(ownerOrganisationId);
        dbContext.ProductCategories.Add(category);
        await dbContext.SaveChangesAsync();

        try
        {
            dbContext.ChangeTracker.Clear();
            NotFoundException exception =
                await Assert.ThrowsAsync<NotFoundException>(() =>
                    new DeactivateProductCategoryHandler(
                            dbContext,
                            new TestClock(FixedUtcNow))
                        .HandleAsync(
                            new DeactivateProductCategoryCommand(
                                Guid.NewGuid(),
                                category.Id,
                                Guid.NewGuid(),
                                category.RowVersion),
                            CancellationToken.None));

            Assert.Equal("DEACTIVATE_PRODUCT_CATEGORY_NOT_FOUND", exception.Code);
        }
        finally
        {
            await DeleteCategoriesAsync(dbContext, ownerOrganisationId);
        }
    }

    private static CatalogDbContext GetDbContext(IServiceScope scope)
    {
        return scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
    }

    private static ProductCategory CreateCategory(
        Guid organisationId,
        bool inactive = false)
    {
        ProductCategory category = ProductCategory.Create(
            organisationId,
            $"STATUS-{Guid.NewGuid():N}"[..15].ToUpperInvariant(),
            "Status category",
            null,
            FixedUtcNow.AddDays(-1),
            Guid.NewGuid());

        if (inactive)
        {
            category.Deactivate(FixedUtcNow.AddHours(-1), Guid.NewGuid());
        }

        return category;
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
