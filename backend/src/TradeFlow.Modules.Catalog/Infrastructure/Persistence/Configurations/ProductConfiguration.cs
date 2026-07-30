using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TradeFlow.Modules.Catalog.Domain.Products;
using TradeFlow.Modules.Catalog.Domain.UnitsOfMeasure;
using TradeFlow.Modules.Catalog.Domain.ProductCategories;
using TradeFlow.Modules.Catalog.Domain.TaxCategories;

namespace TradeFlow.Modules.Catalog.Infrastructure.Persistence.Configurations;

internal sealed class ProductConfiguration
    : IEntityTypeConfiguration<Product>

{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products", "catalog");

        builder.HasKey(product => product.Id);

        builder.Property(product => product.Id).ValueGeneratedNever();

        builder.Property(product => product.OrganisationId).IsRequired();

        builder.Property(product => product.Sku)
            .HasColumnName("SKU")
            .HasMaxLength(Product.MaxSkuLength)
            .IsUnicode(false)
            .IsRequired();

        builder.Property(product => product.Name)
            .HasMaxLength(Product.MaxNameLength)
            .IsUnicode()
            .IsRequired();

        builder.Property(product => product.Description)
            .HasMaxLength(Product.MaxDescriptionLength)
            .IsUnicode()
            .HasDefaultValue(string.Empty)
            .IsRequired();

        builder.Property(product => product.UnitOfMeasureId)
            .IsRequired();

        builder.Property(product => product.ProductCategoryId)
            .IsRequired(false);

        builder.Property(product => product.TaxCategoryId)
            .IsRequired();

        builder.Property(product => product.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsUnicode(false)
            .IsRequired();

        builder.Property(product => product.CreatedAt)
            .HasPrecision(7)
            .IsRequired();

        builder.Property(product => product.CreatedBy)
            .IsRequired();

        builder.Property(product => product.LastModifiedAt)
            .HasPrecision(7)
            .IsRequired(false);

        builder.Property(product => product.LastModifiedBy)
            .IsRequired(false);

        builder.Property(product => product.RowVersion)
            .IsRowVersion();

        builder.HasIndex(product => new
        {
            product.OrganisationId,
            product.Sku
        })
            .IsUnique()
            .HasDatabaseName("UX_Products_OrganisationId_Sku");

        builder.HasIndex(product => new
        {
            product.OrganisationId,
            product.Status

        })
            .HasDatabaseName("IX_Products_OrganisationId_Status");

        builder.HasIndex(product => product.UnitOfMeasureId)
            .HasDatabaseName("IX_Products_UnitOfMeasureId");

        builder.HasIndex(product => product.ProductCategoryId)
            .HasDatabaseName("IX_Products_ProductCategoryId");

        builder.HasIndex(product => product.TaxCategoryId)
            .HasDatabaseName("IX_Products_TaxCategoryId");

        builder.HasOne<UnitOfMeasure>()
            .WithMany()
            .HasConstraintName("FK_Products_UnitsOfMeasure_UnitOfMeasureId")
            .HasForeignKey(product => product.UnitOfMeasureId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ProductCategory>()
            .WithMany()
            .HasConstraintName("FK_Products_ProductCategories_ProductCategoryId")
            .HasForeignKey(product => product.ProductCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<TaxCategory>()
            .WithMany()
            .HasForeignKey(product => product.TaxCategoryId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_Products_TaxCategories_TaxCategoryId");
    }
}
