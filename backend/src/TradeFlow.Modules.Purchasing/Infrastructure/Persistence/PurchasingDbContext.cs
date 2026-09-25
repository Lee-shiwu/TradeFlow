using Microsoft.EntityFrameworkCore;
using TradeFlow.Modules.Purchasing.Domain.Suppliers;

namespace TradeFlow.Modules.Purchasing.Infrastructure.Persistence;

public sealed class PurchasingDbContext(
    DbContextOptions<PurchasingDbContext> options)
    : DbContext(options)
{
    public DbSet<Supplier> Suppliers => Set<Supplier>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("purchasing");
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(PurchasingDbContext).Assembly);
    }
}
