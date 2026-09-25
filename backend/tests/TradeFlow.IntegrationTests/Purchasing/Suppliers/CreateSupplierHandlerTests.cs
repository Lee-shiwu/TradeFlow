using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TradeFlow.BuildingBlocks.Exceptions;
using TradeFlow.BuildingBlocks.Time;
using TradeFlow.Modules.Purchasing.Application.Suppliers.CreateSupplier;
using TradeFlow.Modules.Purchasing.Domain.Suppliers;
using TradeFlow.Modules.Purchasing.Infrastructure.Persistence;

namespace TradeFlow.IntegrationTests.Purchasing.Suppliers;

public sealed class CreateSupplierHandlerTests(ApiFactory factory)
    : IClassFixture<ApiFactory>
{
    private static readonly DateTimeOffset FixedUtcNow =
        new(2026, 9, 25, 9, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task HandleAsync_WithValidCommand_CreatesSupplier()
    {
        using IServiceScope scope = factory.Services.CreateScope();
        PurchasingDbContext dbContext =
            scope.ServiceProvider.GetRequiredService<PurchasingDbContext>();
        await dbContext.Database.MigrateAsync();
        CreateSupplierHandler handler =
            new(dbContext, new TestClock(FixedUtcNow));
        Guid organisationId = Guid.NewGuid();
        Guid createdBy = Guid.NewGuid();

        CreateSupplierResult result =
            await handler.HandleAsync(
                new CreateSupplierCommand(
                    organisationId,
                    " supplier-01 ",
                    "  Auckland   Office Supplies  ",
                    createdBy),
                CancellationToken.None);

        try
        {
            Assert.Equal("SUPPLIER-01", result.Code);
            Assert.Equal("Auckland Office Supplies", result.Name);
            Assert.Equal(SupplierStatus.Active, result.Status);
            Assert.Equal(FixedUtcNow, result.CreatedAt);
            Assert.Equal(createdBy, result.CreatedBy);
            Assert.NotEmpty(result.RowVersion);

            Supplier? persisted =
                await dbContext.Suppliers
                    .AsNoTracking()
                    .SingleOrDefaultAsync(supplier =>
                        supplier.Id == result.SupplierId);
            Assert.NotNull(persisted);
            Assert.Equal(organisationId, persisted.OrganisationId);
        }
        finally
        {
            await DeleteOrganisationSuppliersAsync(dbContext, organisationId);
        }
    }

    [Fact]
    public async Task HandleAsync_WithDuplicateCodeInOrganisation_ThrowsConflict()
    {
        using IServiceScope scope = factory.Services.CreateScope();
        PurchasingDbContext dbContext =
            scope.ServiceProvider.GetRequiredService<PurchasingDbContext>();
        await dbContext.Database.MigrateAsync();
        CreateSupplierHandler handler =
            new(dbContext, new TestClock(FixedUtcNow));
        Guid organisationId = Guid.NewGuid();
        string code = $"DUP-{CreateSuffix()}";

        dbContext.Suppliers.Add(CreateSupplier(organisationId, code));
        await dbContext.SaveChangesAsync();

        try
        {
            BusinessRuleException exception =
                await Assert.ThrowsAsync<BusinessRuleException>(() =>
                    handler.HandleAsync(
                        new CreateSupplierCommand(
                            organisationId,
                            code.ToLowerInvariant(),
                            "Duplicate supplier",
                            Guid.NewGuid()),
                        CancellationToken.None));

            Assert.Equal("SUPPLIER_CODE_ALREADY_EXISTS", exception.Code);
        }
        finally
        {
            await DeleteOrganisationSuppliersAsync(dbContext, organisationId);
        }
    }

    [Fact]
    public async Task HandleAsync_WithSameCodeInAnotherOrganisation_Succeeds()
    {
        using IServiceScope scope = factory.Services.CreateScope();
        PurchasingDbContext dbContext =
            scope.ServiceProvider.GetRequiredService<PurchasingDbContext>();
        await dbContext.Database.MigrateAsync();
        CreateSupplierHandler handler =
            new(dbContext, new TestClock(FixedUtcNow));
        Guid firstOrganisationId = Guid.NewGuid();
        Guid secondOrganisationId = Guid.NewGuid();
        string code = $"SHARED-{CreateSuffix()}";
        dbContext.Suppliers.Add(CreateSupplier(firstOrganisationId, code));
        await dbContext.SaveChangesAsync();

        try
        {
            CreateSupplierResult result =
                await handler.HandleAsync(
                    new CreateSupplierCommand(
                        secondOrganisationId,
                        code,
                        "Second supplier",
                        Guid.NewGuid()),
                    CancellationToken.None);

            Assert.Equal(code, result.Code);
            Assert.Equal(
                2,
                await dbContext.Suppliers.CountAsync(supplier =>
                    supplier.Code == code));
        }
        finally
        {
            await DeleteOrganisationSuppliersAsync(
                dbContext,
                firstOrganisationId,
                secondOrganisationId);
        }
    }

    [Fact]
    public async Task HandleAsync_WithNullCommand_ThrowsArgumentNullException()
    {
        using IServiceScope scope = factory.Services.CreateScope();
        PurchasingDbContext dbContext =
            scope.ServiceProvider.GetRequiredService<PurchasingDbContext>();
        CreateSupplierHandler handler =
            new(dbContext, new TestClock(FixedUtcNow));

        ArgumentNullException exception =
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                handler.HandleAsync(null!, CancellationToken.None));

        Assert.Equal("command", exception.ParamName);
    }

    private static Supplier CreateSupplier(Guid organisationId, string code)
    {
        return Supplier.Create(
            organisationId,
            code,
            "Existing supplier",
            FixedUtcNow,
            Guid.NewGuid());
    }

    private static string CreateSuffix() =>
        Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();

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

    private sealed class TestClock(DateTimeOffset utcNow) : IClock
    {
        public DateTimeOffset UtcNow { get; } = utcNow;
    }
}
