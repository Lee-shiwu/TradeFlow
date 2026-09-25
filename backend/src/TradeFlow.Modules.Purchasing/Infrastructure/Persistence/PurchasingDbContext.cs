using Microsoft.EntityFrameworkCore;
using TradeFlow.Modules.Purchasing.Domain.PurchaseOrders;
using TradeFlow.Modules.Purchasing.Domain.Suppliers;

namespace TradeFlow.Modules.Purchasing.Infrastructure.Persistence;

public sealed class PurchasingDbContext(
    DbContextOptions<PurchasingDbContext> options)
    : DbContext(options)
{
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();
    public DbSet<PurchaseOrderLine> PurchaseOrderLines => Set<PurchaseOrderLine>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("purchasing");
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(PurchasingDbContext).Assembly);
    }
}
