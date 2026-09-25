using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TradeFlow.BuildingBlocks.Exceptions;
using TradeFlow.Modules.Purchasing.Application.Suppliers.ListSuppliers;
using TradeFlow.Modules.Purchasing.Domain.Suppliers;
using TradeFlow.Modules.Purchasing.Infrastructure.Persistence;

namespace TradeFlow.IntegrationTests.Purchasing.Suppliers;

public sealed class ListSuppliersHandlerTests(ApiFactory factory)
    : IClassFixture<ApiFactory>
{
    [Fact]
    public async Task HandleAsync_FiltersSearchStatusAndOrganisation()
    {
        using IServiceScope scope = factory.Services.CreateScope();
        PurchasingDbContext dbContext =
            scope.ServiceProvider.GetRequiredService<PurchasingDbContext>();
        await dbContext.Database.MigrateAsync();
        ListSuppliersHandler handler = new(dbContext);
        Guid organisationId = Guid.NewGuid();
        Guid otherOrganisationId = Guid.NewGuid();

        dbContext.Suppliers.AddRange(
            CreateSupplier(organisationId, "OFFICE-A", "Office supplier"),
            CreateSupplier(organisationId, "FOOD-A", "Food supplier"),
            CreateSupplier(otherOrganisationId, "OFFICE-B", "Office supplier"));
        await dbContext.SaveChangesAsync();

        try
        {
            dbContext.ChangeTracker.Clear();
            ListSuppliersResult result =
                await handler.HandleAsync(
                    new ListSuppliersQuery(
                        organisationId,
                        " office ",
                        SupplierStatus.Active,
                        1,
                        20),
                    CancellationToken.None);

            ListSupplierItem item = Assert.Single(result.Items);
            Assert.Equal("OFFICE-A", item.Code);
            Assert.Equal(1, result.TotalCount);
            Assert.Equal(1, result.TotalPages);
            Assert.Empty(dbContext.ChangeTracker.Entries<Supplier>());
        }
        finally
        {
            await DeleteOrganisationSuppliersAsync(
                dbContext,
                organisationId,
                otherOrganisationId);
        }
    }

    [Theory]
    [InlineData(0, 20, "LIST_SUPPLIERS_PAGE_NUMBER_INVALID")]
    [InlineData(1, 0, "LIST_SUPPLIERS_PAGE_SIZE_INVALID")]
    [InlineData(1, 101, "LIST_SUPPLIERS_PAGE_SIZE_INVALID")]
    public async Task HandleAsync_WithInvalidPagination_ThrowsValidation(
        int pageNumber,
        int pageSize,
        string expectedCode)
    {
        using IServiceScope scope = factory.Services.CreateScope();
        PurchasingDbContext dbContext =
            scope.ServiceProvider.GetRequiredService<PurchasingDbContext>();
        ListSuppliersHandler handler = new(dbContext);

        RequestValidationException exception =
            await Assert.ThrowsAsync<RequestValidationException>(() =>
                handler.HandleAsync(
                    new ListSuppliersQuery(
                        Guid.NewGuid(),
                        null,
                        null,
                        pageNumber,
                        pageSize),
                    CancellationToken.None));

        Assert.Equal(expectedCode, exception.Code);
    }

    [Fact]
    public async Task HandleAsync_WithMissingOrganisation_ThrowsValidation()
    {
        using IServiceScope scope = factory.Services.CreateScope();
        PurchasingDbContext dbContext =
            scope.ServiceProvider.GetRequiredService<PurchasingDbContext>();
        ListSuppliersHandler handler = new(dbContext);

        RequestValidationException exception =
            await Assert.ThrowsAsync<RequestValidationException>(() =>
                handler.HandleAsync(
                    new ListSuppliersQuery(Guid.Empty, null, null, 1, 20),
                    CancellationToken.None));

        Assert.Equal("LIST_SUPPLIERS_ORGANISATION_REQUIRED", exception.Code);
    }

    private static Supplier CreateSupplier(
        Guid organisationId,
        string code,
        string name)
    {
        return Supplier.Create(
            organisationId,
            code,
            name,
            DateTimeOffset.UtcNow,
            Guid.NewGuid());
    }

    private static async Task DeleteOrganisationSuppliersAsync(
        PurchasingDbContext dbContext,
        params Guid[] organisationIds)
    {
        dbContext.ChangeTracker.Clear();
        await dbContext.Suppliers
            .Where(supplier =>
                organisationIds.Contains(supplier.OrganisationId))
            .ExecuteDeleteAsync();
    }
}
