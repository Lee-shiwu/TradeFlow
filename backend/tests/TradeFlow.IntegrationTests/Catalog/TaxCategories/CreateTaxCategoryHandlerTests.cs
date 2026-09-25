using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TradeFlow.BuildingBlocks.Exceptions;
using TradeFlow.Modules.Catalog.Application.TaxCategories.CreateTaxCategory;
using TradeFlow.Modules.Catalog.Domain.TaxCategories;
using TradeFlow.Modules.Catalog.Infrastructure.Persistence;

namespace TradeFlow.IntegrationTests.Catalog.TaxCategories;

public sealed class CreateTaxCategoryHandlerTests(ApiFactory factory)
    : IClassFixture<ApiFactory>
{
    [Fact]
    public async Task HandleAsync_WithValidCommand_CreatesNormalizedCategory()
    {
        using IServiceScope scope = factory.Services.CreateScope();
        CatalogDbContext dbContext =
            scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        CreateTaxCategoryHandler handler =
            scope.ServiceProvider.GetRequiredService<CreateTaxCategoryHandler>();
        await dbContext.Database.MigrateAsync();

        string suffix = CreateSuffix();
        CreateTaxCategoryResult result =
            await handler.HandleAsync(
                new CreateTaxCategoryCommand(
                    $" tax-{suffix} ",
                    "  Regional   levy  ",
                    "  Regional standard tax.  ",
                    0.125m,
                    TaxTreatment.StandardRated,
                    new DateOnly(2026, 10, 1),
                    new DateOnly(2027, 9, 30)),
                CancellationToken.None);

        try
        {
            Assert.NotEqual(Guid.Empty, result.TaxCategoryId);
            Assert.Equal($"TAX-{suffix}", result.Code);
            Assert.Equal("Regional levy", result.Name);
            Assert.Equal("Regional standard tax.", result.Description);
            Assert.Equal(0.125m, result.Rate);
            Assert.Equal(TaxTreatment.StandardRated, result.Treatment);
            Assert.Equal(TaxCategoryStatus.Active, result.Status);
            Assert.Equal(new DateOnly(2026, 10, 1), result.EffectiveFrom);
            Assert.Equal(new DateOnly(2027, 9, 30), result.EffectiveTo);
            Assert.NotEmpty(result.RowVersion);

            TaxCategory? persisted =
                await dbContext.TaxCategories
                    .AsNoTracking()
                    .SingleOrDefaultAsync(category =>
                        category.Id == result.TaxCategoryId);

            Assert.NotNull(persisted);
            Assert.Equal(result.Code, persisted.Code);
            Assert.Equal(result.RowVersion, persisted.RowVersion);
        }
        finally
        {
            await DeleteCategoriesAsync(dbContext, result.TaxCategoryId);
        }
    }

    [Fact]
    public async Task HandleAsync_WithDuplicateCode_ThrowsConflict()
    {
        using IServiceScope scope = factory.Services.CreateScope();
        CatalogDbContext dbContext =
            scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        CreateTaxCategoryHandler handler =
            scope.ServiceProvider.GetRequiredService<CreateTaxCategoryHandler>();
        await dbContext.Database.MigrateAsync();

        TaxCategory existing = CreateCategory($"DUP-{CreateSuffix()}");
        dbContext.TaxCategories.Add(existing);
        await dbContext.SaveChangesAsync();

        try
        {
            BusinessRuleException exception =
                await Assert.ThrowsAsync<BusinessRuleException>(() =>
                    handler.HandleAsync(
                        ValidCommand() with
                        {
                            Code = $" {existing.Code.ToLowerInvariant()} "
                        },
                        CancellationToken.None));

            Assert.Equal("TAX_CATEGORY_CODE_ALREADY_EXISTS", exception.Code);
        }
        finally
        {
            await DeleteCategoriesAsync(dbContext, existing.Id);
        }
    }

    [Fact]
    public async Task HandleAsync_WithNullCommand_ThrowsArgumentNullException()
    {
        using IServiceScope scope = factory.Services.CreateScope();
        CreateTaxCategoryHandler handler =
            scope.ServiceProvider.GetRequiredService<CreateTaxCategoryHandler>();

        ArgumentNullException exception =
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                handler.HandleAsync(null!, CancellationToken.None));

        Assert.Equal("command", exception.ParamName);
    }

    private static CreateTaxCategoryCommand ValidCommand()
    {
        return new CreateTaxCategoryCommand(
            $"CREATE-{CreateSuffix()}",
            "Regional levy",
            null,
            0.125m,
            TaxTreatment.StandardRated,
            new DateOnly(2026, 10, 1),
            null);
    }

    private static TaxCategory CreateCategory(string code)
    {
        return TaxCategory.Create(
            code,
            "Existing tax category",
            null,
            0m,
            TaxTreatment.ZeroRated,
            new DateOnly(2026, 1, 1),
            null);
    }

    private static string CreateSuffix()
    {
        return Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();
    }

    private static async Task DeleteCategoriesAsync(
        CatalogDbContext dbContext,
        params Guid[] taxCategoryIds)
    {
        dbContext.ChangeTracker.Clear();
        await dbContext.TaxCategories
            .Where(category => taxCategoryIds.Contains(category.Id))
            .ExecuteDeleteAsync();
    }
}
