using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TradeFlow.Modules.Purchasing.Domain.GoodsReceipts;
using TradeFlow.Modules.Purchasing.Domain.PurchaseOrders;

namespace TradeFlow.Modules.Purchasing.Infrastructure.Persistence.Configurations;

internal sealed class GoodsReceiptConfiguration : IEntityTypeConfiguration<GoodsReceipt>
{
    public void Configure(EntityTypeBuilder<GoodsReceipt> builder)
    {
        builder.ToTable("GoodsReceipts");
        builder.HasKey(receipt => receipt.Id);
        builder.Property(receipt => receipt.ReceiptNumber).HasMaxLength(40).IsUnicode(false);
        builder.HasIndex(receipt => new { receipt.OrganisationId, receipt.ReceiptNumber }).IsUnique();
        builder.HasIndex(receipt => receipt.PurchaseOrderId).IsUnique();
        builder.HasOne<PurchaseOrder>()
            .WithMany()
            .HasForeignKey(receipt => receipt.PurchaseOrderId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(receipt => receipt.Lines)
            .WithOne()
            .HasForeignKey(line => line.GoodsReceiptId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(receipt => receipt.Lines)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
