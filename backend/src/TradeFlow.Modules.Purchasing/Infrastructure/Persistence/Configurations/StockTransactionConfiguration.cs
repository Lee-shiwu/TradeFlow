using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TradeFlow.Modules.Purchasing.Domain.GoodsReceipts;
using TradeFlow.Modules.Purchasing.Domain.Inventory;

namespace TradeFlow.Modules.Purchasing.Infrastructure.Persistence.Configurations;

internal sealed class StockTransactionConfiguration : IEntityTypeConfiguration<StockTransaction>
{
    public void Configure(EntityTypeBuilder<StockTransaction> builder)
    {
        builder.ToTable("StockTransactions");
        builder.HasKey(transaction => transaction.Id);
        builder.Property(transaction => transaction.ProductSku).HasMaxLength(50).IsUnicode(false);
        builder.Property(transaction => transaction.ProductName).HasMaxLength(100);
        builder.Property(transaction => transaction.Type).HasConversion<string>().HasMaxLength(30).IsUnicode(false);
        builder.Property(transaction => transaction.Quantity).HasPrecision(18, 4);
        builder.HasIndex(transaction => new { transaction.OrganisationId, transaction.ProductId, transaction.OccurredAt });
        builder.HasIndex(transaction => transaction.SourceLineId).IsUnique();
        builder.HasOne<GoodsReceiptLine>()
            .WithOne()
            .HasForeignKey<StockTransaction>(transaction => transaction.SourceLineId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
