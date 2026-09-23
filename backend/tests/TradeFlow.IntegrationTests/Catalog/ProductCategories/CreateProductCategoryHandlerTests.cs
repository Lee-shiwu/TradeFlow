using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TradeFlow.BuildingBlocks.Exceptions;
using TradeFlow.BuildingBlocks.Time;
using TradeFlow.Modules.Catalog.Application.ProductCategories.CreateProductCategory;
using TradeFlow.Modules.Catalog.Domain.ProductCategories;
using TradeFlow.Modules.Catalog.Infrastructure.Persistence;

namespace TradeFlow.IntegrationTests.Catalog.ProductCategories;

public sealed class CreateProductCategoryHandlerTests(ApiFactory factory)
    : IClassFixture<ApiFactory>
{
    private static readonly DateTimeOffset FixedUtcNow =
        new(2026, 9, 23, 8, 30, 0, TimeSpan.Zero);

    [Fact]
    public async Task HandleAsync_WithValidCommand_CreatesNormalizedCategory()
    {
        using IServiceScope scope = factory.Services.CreateScope();
        CatalogDbContext dbContext =
            scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        CreateProductCategoryHandler handler =
            new(dbContext, new TestClock(FixedUtcNow));

        await dbContext.Database.MigrateAsync();

        Guid organisationId = Guid.NewGuid();
        Guid createdBy = Guid.NewGuid();
        string code = $" category-{CreateSuffix()} ";

        CreateProductCategoryResult result =
            await handler.HandleAsync(
                new CreateProductCategoryCommand(
                    OrganisationId: organisationId,
                    Code: code,
                    Name: "  Office   products  ",
                    Description: "  Products used in the office.  ",
                    CreatedBy: createdBy),
                CancellationToken.None);

        try
        {
            Assert.NotEqual(Guid.Empty, result.ProductCategoryId);
            Assert.Equal(code.Trim().ToUpperInvariant(), result.Code);
            Assert.Equal("Office products", result.Name);
            Assert.Equal("Products used in the office.", result.Description);
            Assert.Equal(ProductCategoryStatus.Active, result.Status);
            Assert.Equal(FixedUtcNow, result.CreatedAt);
            Assert.Equal(createdBy, result.CreatedBy);
            Assert.Null(result.LastModifiedAt);
            Assert.Null(result.LastModifiedBy);
            Assert.NotEmpty(result.RowVersion);

            ProductCategory? persisted =
                await dbContext.ProductCategories
                    .AsNoTracking()
                    .SingleOrDefaultAsync(category =>
                        category.Id == result.ProductCategoryId);

            Assert.NotNull(persisted);
            Assert.Equal(organisationId, persisted.OrganisationId);
            Assert.Equal(result.Code, persisted.Code);
            Assert.Equal(result.RowVersion, persisted.RowVersion);
        }
        finally
        {
            await DeleteCategoriesAsync(dbContext, organisationId);
        }
    }

    [Fact]
    public async Task HandleAsync_WithDuplicateCodeInOrganisation_ThrowsConflict()
    {
        using IServiceScope scope = factory.Services.CreateScope();
        CatalogDbContext dbContext =
            scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        CreateProductCategoryHandler handler =
            new(dbContext, new TestClock(FixedUtcNow));

        await dbContext.Database.MigrateAsync();

        Guid organisationId = Guid.NewGuid();
        string code = $"DUP-{CreateSuffix()}";
        ProductCategory existing = CreateCategory(organisationId, code);

        dbContext.ProductCategories.Add(existing);
        await dbContext.SaveChangesAsync();

        try
        {
            BusinessRuleException exception =
                await Assert.ThrowsAsync<BusinessRuleException>(
                    () => handler.HandleAsync(
                        ValidCommand(organisationId) with
                        {
                            Code = $" {code.ToLowerInvariant()} "
                        },
                        CancellationToken.None));

            Assert.Equal(
                "PRODUCT_CATEGORY_CODE_ALREADY_EXISTS",
                exception.Code);

            Assert.Equal(
                1,
                await dbContext.ProductCategories.CountAsync(category =>
                    category.OrganisationId == organisationId));
        }
        finally
        {
            await DeleteCategoriesAsync(dbContext, organisationId);
        }
    }

    [Fact]
    public async Task HandleAsync_WithSameCodeInDifferentOrganisation_CreatesCategory()
    {
        using IServiceScope scope = factory.Services.CreateScope();
        CatalogDbContext dbContext =
            scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        CreateProductCategoryHandler handler =
            new(dbContext, new TestClock(FixedUtcNow));

        await dbContext.Database.MigrateAsync();

        Guid firstOrganisationId = Guid.NewGuid();
        Guid secondOrganisationId = Guid.NewGuid();
        string code = $"SHARED-{CreateSuffix()}";

        dbContext.ProductCategories.Add(
            CreateCategory(firstOrganisationId, code));
        await dbContext.SaveChangesAsync();

        try
        {
            CreateProductCategoryResult result =
                await handler.HandleAsync(
                    ValidCommand(secondOrganisationId) with { Code = code },
                    CancellationToken.None);

            Assert.Equal(code, result.Code);
            Assert.Equal(
                2,
                await dbContext.ProductCategories.CountAsync(category =>
                    category.Code == code));
        }
        finally
        {
            await DeleteCategoriesAsync(
                dbContext,
                firstOrganisationId,
                secondOrganisationId);
        }
    }

    [Fact]
    public async Task HandleAsync_WithNullCommand_ThrowsArgumentNullException()
    {
        using IServiceScope scope = factory.Services.CreateScope();
        CatalogDbContext dbContext =
            scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        CreateProductCategoryHandler handler =
            new(dbContext, new TestClock(FixedUtcNow));

        ArgumentNullException exception =
            await Assert.ThrowsAsync<ArgumentNullException>(
                () => handler.HandleAsync(null!, CancellationToken.None));

        Assert.Equal("command", exception.ParamName);
    }

    private static CreateProductCategoryCommand ValidCommand(
        Guid organisationId)
    {
        return new CreateProductCategoryCommand(
            OrganisationId: organisationId,
            Code: $"CREATE-{CreateSuffix()}",
            Name: "Office products",
            Description: "Products used in the office.",
            CreatedBy: Guid.NewGuid());
    }

    private static ProductCategory CreateCategory(
        Guid organisationId,
        string code)
    {
        return ProductCategory.Create(
            organisationId,
            code,
            "Existing category",
            null,
            FixedUtcNow.AddDays(-1),
            Guid.NewGuid());
    }

    private static string CreateSuffix()
    {
        return Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();
    }

    private static async Task DeleteCategoriesAsync(
        CatalogDbContext dbContext,
        params Guid[] organisationIds)
    {
        dbContext.ChangeTracker.Clear();

        await dbContext.ProductCategories
            .Where(category =>
                organisationIds.Contains(category.OrganisationId))
            .ExecuteDeleteAsync();
    }

    private sealed class TestClock(DateTimeOffset utcNow) : IClock
    {
        public DateTimeOffset UtcNow { get; } = utcNow;
    }
}
