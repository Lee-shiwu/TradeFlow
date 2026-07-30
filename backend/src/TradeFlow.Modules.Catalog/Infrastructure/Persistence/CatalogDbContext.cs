using Microsoft.EntityFrameworkCore;
using TradeFlow.Modules.Catalog.Domain.ProductCategories;
using TradeFlow.Modules.Catalog.Domain.Products;
using TradeFlow.Modules.Catalog.Domain.TaxCategories;
using TradeFlow.Modules.Catalog.Domain.UnitsOfMeasure;
namespace TradeFlow.Modules.Catalog.Infrastructure.Persistence;

public sealed class CatalogDbContext(DbContextOptions<CatalogDbContext> options)
    : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();

    public DbSet<UnitOfMeasure> UnitsOfMeasures => Set<UnitOfMeasure>();

    public DbSet<ProductCategory> ProductCategories => Set<ProductCategory>();

    public DbSet<TaxCategory> TaxCategories => Set<TaxCategory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("catalog");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CatalogDbContext).Assembly);
    }
}
