using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TradeFlow.Modules.Purchasing.Domain.PurchaseOrders;

namespace TradeFlow.Modules.Purchasing.Infrastructure.Persistence.Configurations;

internal sealed class PurchaseOrderLineConfiguration : IEntityTypeConfiguration<PurchaseOrderLine>
{
    public void Configure(EntityTypeBuilder<PurchaseOrderLine> builder)
    {
        builder.ToTable("PurchaseOrderLines");
        builder.HasKey(line => line.Id);
        builder.Property(line => line.ProductSku).HasMaxLength(50).IsUnicode(false);
        builder.Property(line => line.ProductName).HasMaxLength(100);
        builder.Property(line => line.Quantity).HasPrecision(18, 4);
        builder.Property(line => line.UnitPrice).HasPrecision(18, 4);
        builder.Ignore(line => line.LineTotal);
        builder.HasIndex(line => new { line.PurchaseOrderId, line.ProductId }).IsUnique();
    }
}
