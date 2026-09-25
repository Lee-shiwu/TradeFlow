using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TradeFlow.Modules.Purchasing.Domain.Suppliers;

namespace TradeFlow.Modules.Purchasing.Infrastructure.Persistence.Configurations;

internal sealed class SupplierConfiguration
    : IEntityTypeConfiguration<Supplier>
{
    public void Configure(EntityTypeBuilder<Supplier> builder)
    {
        builder.ToTable("Suppliers", "purchasing");
        builder.HasKey(supplier => supplier.Id);

        builder.Property(supplier => supplier.Id)
            .ValueGeneratedNever();
        builder.Property(supplier => supplier.OrganisationId)
            .IsRequired();
        builder.Property(supplier => supplier.Code)
            .HasMaxLength(Supplier.MaxCodeLength)
            .IsUnicode(false)
            .IsRequired();
        builder.Property(supplier => supplier.Name)
            .HasMaxLength(Supplier.MaxNameLength)
            .IsUnicode()
            .IsRequired();
        builder.Property(supplier => supplier.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsUnicode(false)
            .IsRequired();
        builder.Property(supplier => supplier.CreatedAt)
            .IsRequired();
        builder.Property(supplier => supplier.CreatedBy)
            .IsRequired();
        builder.Property(supplier => supplier.RowVersion)
            .IsRowVersion();

        builder.HasIndex(supplier => new
        {
            supplier.OrganisationId,
            supplier.Code
        })
            .IsUnique()
            .HasDatabaseName("UX_Suppliers_OrganisationId_Code");

        builder.HasIndex(supplier => new
        {
            supplier.OrganisationId,
            supplier.Status,
            supplier.Name
        })
            .HasDatabaseName("IX_Suppliers_OrganisationId_Status_Name");
    }
}
