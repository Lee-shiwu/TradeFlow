using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TradeFlow.Modules.Purchasing.Domain.GoodsReceipts;

namespace TradeFlow.Modules.Purchasing.Infrastructure.Persistence.Configurations;

internal sealed class GoodsReceiptLineConfiguration : IEntityTypeConfiguration<GoodsReceiptLine>
{
    public void Configure(EntityTypeBuilder<GoodsReceiptLine> builder)
    {
        builder.ToTable("GoodsReceiptLines");
        builder.HasKey(line => line.Id);
        builder.Property(line => line.ProductSku).HasMaxLength(50).IsUnicode(false);
        builder.Property(line => line.ProductName).HasMaxLength(100);
        builder.Property(line => line.QuantityReceived).HasPrecision(18, 4);
        builder.HasIndex(line => line.PurchaseOrderLineId).IsUnique();
    }
}
