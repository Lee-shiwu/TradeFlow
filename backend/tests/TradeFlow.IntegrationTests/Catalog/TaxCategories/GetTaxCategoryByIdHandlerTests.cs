using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TradeFlow.BuildingBlocks.Exceptions;
using TradeFlow.Modules.Catalog.Application.TaxCategories.GetTaxCategoryById;
using TradeFlow.Modules.Catalog.Domain.TaxCategories;
using TradeFlow.Modules.Catalog.Infrastructure.Persistence;

namespace TradeFlow.IntegrationTests.Catalog.TaxCategories;

public sealed class GetTaxCategoryByIdHandlerTests(ApiFactory factory)
    : IClassFixture<ApiFactory>
{
    private static readonly Guid StandardGstId =
        Guid.Parse("20000000-0000-0000-0000-000000000001");

    [Fact]
    public async Task HandleAsync_WithExistingCategory_ReturnsDetails()
    {
        using IServiceScope scope = factory.Services.CreateScope();
        CatalogDbContext dbContext =
            scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        GetTaxCategoryByIdHandler handler =
            scope.ServiceProvider.GetRequiredService<GetTaxCategoryByIdHandler>();
        await dbContext.Database.MigrateAsync();

        dbContext.ChangeTracker.Clear();

        GetTaxCategoryByIdResult result =
            await handler.HandleAsync(
                new GetTaxCategoryByIdQuery(StandardGstId),
                CancellationToken.None);

        Assert.Equal(StandardGstId, result.TaxCategoryId);
        Assert.Equal("GST15", result.Code);
        Assert.Equal("Standard GST", result.Name);
        Assert.Equal("Standard-rated supplies at 15% GST.", result.Description);
        Assert.Equal(0.1500m, result.Rate);
        Assert.Equal(TaxTreatment.StandardRated, result.Treatment);
        Assert.Equal(TaxCategoryStatus.Active, result.Status);
        Assert.Equal(new DateOnly(2010, 10, 1), result.EffectiveFrom);
        Assert.Null(result.EffectiveTo);
        Assert.NotEmpty(result.RowVersion);
        Assert.Empty(dbContext.ChangeTracker.Entries<TaxCategory>());
    }

    [Fact]
    public async Task HandleAsync_WithUnknownCategory_ThrowsNotFound()
    {
        using IServiceScope scope = factory.Services.CreateScope();
        GetTaxCategoryByIdHandler handler =
            scope.ServiceProvider.GetRequiredService<GetTaxCategoryByIdHandler>();

        NotFoundException exception =
            await Assert.ThrowsAsync<NotFoundException>(() =>
                handler.HandleAsync(
                    new GetTaxCategoryByIdQuery(Guid.NewGuid()),
                    CancellationToken.None));

        Assert.Equal("TAX_CATEGORY_NOT_FOUND", exception.Code);
        Assert.Equal(
            "The requested tax category was not found.",
            exception.Message);
    }

    [Fact]
    public async Task HandleAsync_WithNullQuery_ThrowsArgumentNullException()
    {
        using IServiceScope scope = factory.Services.CreateScope();
        GetTaxCategoryByIdHandler handler =
            scope.ServiceProvider.GetRequiredService<GetTaxCategoryByIdHandler>();

        ArgumentNullException exception =
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                handler.HandleAsync(null!, CancellationToken.None));

        Assert.Equal("query", exception.ParamName);
    }
}
