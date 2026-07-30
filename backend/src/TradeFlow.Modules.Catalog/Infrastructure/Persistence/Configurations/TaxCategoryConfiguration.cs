using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TradeFlow.Modules.Catalog.Domain.TaxCategories;

namespace TradeFlow.Modules.Catalog.Infrastructure.Persistence.Configurations;

internal sealed class TaxCategoryConfiguration : IEntityTypeConfiguration<TaxCategory>
{
    public void Configure(EntityTypeBuilder<TaxCategory> builder)
    {
        builder.ToTable(
            "TaxCategories",
            "catalog",
            tableBuilder =>
            {
                tableBuilder.HasCheckConstraint(
                    "CK_TaxCategories_Rate",
                    "[Rate] >= 0 AND [Rate] <= 1");

                tableBuilder.HasCheckConstraint(
                    "CK_TaxCategories_TreatmentRate",
                    "([Treatment] = 'StandardRated' AND [Rate] > 0) "
                    + "OR ([Treatment] IN ('ZeroRated', 'Exempt') "
                    + "AND [Rate] = 0)");

                tableBuilder.HasCheckConstraint(
                    "CK_TaxCategories_EffectivePeriod",
                    "[EffectiveTo] IS NULL "
                    + "OR [EffectiveTo] >= [EffectiveFrom]");
            });

        builder.HasKey(taxCategory => taxCategory.Id);

        builder.Property(taxCategory => taxCategory.Id)
            .ValueGeneratedNever();


        builder.Property(taxCategory => taxCategory.Code)
            .HasMaxLength(TaxCategory.MaxCodeLength)
            .IsUnicode(false)
            .IsRequired();

        builder.Property(taxCategory => taxCategory.Name)
            .HasMaxLength(TaxCategory.MaxNameLength)
            .IsUnicode()
            .IsRequired();

        builder.Property(taxCategory => taxCategory.Description)
            .HasMaxLength(TaxCategory.MaxDescriptionLength)
            .IsUnicode()
            .HasDefaultValue(string.Empty)
            .IsRequired();

        builder.Property(taxCategory => taxCategory.Rate)
            .HasPrecision(5, 4)
            .IsRequired();

        builder.Property(taxCategory => taxCategory.Treatment)
            .HasConversion<string>()
            .IsUnicode(false)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(taxCategory => taxCategory.Status)
            .HasConversion<string>()
            .IsUnicode(false)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(taxCategory => taxCategory.EffectiveFrom)
            .HasColumnType("date")
            .IsRequired();

        builder.Property(taxCategory => taxCategory.EffectiveTo)
            .HasColumnType("date")
            .IsRequired(false);

        builder.Property(taxCategory => taxCategory.RowVersion)
            .IsRowVersion();

        builder.HasIndex(taxCategory => taxCategory.Code)
            .IsUnique()
            .HasDatabaseName("UX_TaxCategories_Code");

        builder.HasIndex(taxCategory => new
        {
            taxCategory.Status,
            taxCategory.EffectiveFrom,
            taxCategory.EffectiveTo
        })
            .HasDatabaseName("IX_TaxCategories_Status_EffectiveFrom_EffectiveTo");

        builder.HasData(
            new
            {
                Id = Guid.Parse(
            "20000000-0000-0000-0000-000000000001"),
                Code = "GST15",
                Name = "Standard GST",
                Description =
            "Standard-rated supplies at 15% GST.",
                Rate = 0.1500m,
                Treatment = TaxTreatment.StandardRated,
                Status = TaxCategoryStatus.Active,
                EffectiveFrom = new DateOnly(2010, 10, 1),
                EffectiveTo = (DateOnly?)null
            },
            new
            {
                Id = Guid.Parse(
            "20000000-0000-0000-0000-000000000002"),
                Code = "GST0",
                Name = "Zero-rated GST",
                Description =
            "Zero-rated supplies charged at 0% GST.",
                Rate = 0m,
                Treatment = TaxTreatment.ZeroRated,
                Status = TaxCategoryStatus.Active,
                EffectiveFrom = new DateOnly(2010, 10, 1),
                EffectiveTo = (DateOnly?)null

            },
            new
            {
                Id = Guid.Parse(
            "20000000-0000-0000-0000-000000000003"),
                Code = "EXEMPT",
                Name = "GST exempt",
                Description =
            "Supplies that are exempt from GST.",
                Rate = 0m,
                Treatment = TaxTreatment.Exempt,
                Status = TaxCategoryStatus.Active,
                EffectiveFrom = new DateOnly(2010, 10, 1),
                EffectiveTo = (DateOnly?)null

            });



    }
}
