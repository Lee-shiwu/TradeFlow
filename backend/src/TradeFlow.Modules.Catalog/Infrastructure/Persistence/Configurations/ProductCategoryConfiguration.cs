using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TradeFlow.Modules.Catalog.Domain.ProductCategories;

namespace TradeFlow.Modules.Catalog.Infrastructure.Persistence.Configurations;

internal sealed class ProductCategoryConfiguration : IEntityTypeConfiguration<ProductCategory>
{
    public void Configure(EntityTypeBuilder<ProductCategory> builder)
    {
        builder.ToTable("ProductCategories", "catalog");

        builder.HasKey(productCategory => productCategory.Id);

        builder.Property(productCategory => productCategory.Id).ValueGeneratedNever();

        builder.Property(productCategory => productCategory.OrganisationId)
            .IsRequired();

        builder.Property(productCategory => productCategory.Code)
            .IsRequired()
            .IsUnicode(false)
            .HasMaxLength(ProductCategory.MaxCodeLength);

        builder.Property(productCategory => productCategory.Name)
            .IsRequired()
            .IsUnicode()
            .HasMaxLength(ProductCategory.MaxNameLength);

        builder.Property(productCategory => productCategory.Description)
            .IsRequired()
            .IsUnicode()
            .HasMaxLength(ProductCategory.MaxDescriptionLength)
            .HasDefaultValue(string.Empty);

        builder.Property(productCategory => productCategory.Status)
            .IsRequired()
            .HasConversion<string>()
            .IsUnicode(false)
            .HasMaxLength(20);

        builder.Property(productCategory => productCategory.CreatedAt)
            .IsRequired()
            .HasPrecision(7);

        builder.Property(productCategory => productCategory.CreatedBy)
            .IsRequired();

        builder.Property(productCategory => productCategory.LastModifiedAt)
            .IsRequired(false)
            .HasPrecision(7);

        builder.Property(productCategory => productCategory.LastModifiedBy)
            .IsRequired(false);

        builder.Property(productCategory => productCategory.RowVersion)
            .IsRowVersion();

        builder.HasIndex(productCategory => new
        {
            productCategory.OrganisationId,
            productCategory.Code
        })
            .IsUnique()
            .HasDatabaseName("UX_ProductCategories_OrganisationId_Code");

        builder.HasIndex(productCategory => new
        {
            productCategory.OrganisationId,
            productCategory.Status
        })
            .HasDatabaseName("IX_ProductCategories_OrganisationId_Status");
    }
}
