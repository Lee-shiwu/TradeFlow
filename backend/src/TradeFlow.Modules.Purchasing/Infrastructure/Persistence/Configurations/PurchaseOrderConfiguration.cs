using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TradeFlow.Modules.Purchasing.Domain.PurchaseOrders;

namespace TradeFlow.Modules.Purchasing.Infrastructure.Persistence.Configurations;

internal sealed class PurchaseOrderConfiguration : IEntityTypeConfiguration<PurchaseOrder>
{
    public void Configure(EntityTypeBuilder<PurchaseOrder> builder)
    {
        builder.ToTable("PurchaseOrders");
        builder.HasKey(order => order.Id);
        builder.Property(order => order.Reference).HasMaxLength(50).IsUnicode(false);
        builder.Property(order => order.Status).HasConversion<string>().HasMaxLength(20).IsUnicode(false);
        builder.Property(order => order.RowVersion).IsRowVersion().IsConcurrencyToken();
        builder.Ignore(order => order.TotalAmount);

        builder.HasIndex(order => new { order.OrganisationId, order.Reference })
            .IsUnique()
            .HasDatabaseName("UX_PurchaseOrders_OrganisationId_Reference");
        builder.HasIndex(order => new { order.OrganisationId, order.Status, order.CreatedAt });

        builder.HasOne<Domain.Suppliers.Supplier>()
            .WithMany()
            .HasForeignKey(order => order.SupplierId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(order => order.Lines)
            .WithOne()
            .HasForeignKey(line => line.PurchaseOrderId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(order => order.Lines)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
