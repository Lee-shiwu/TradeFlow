using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TradeFlow.Modules.Catalog.Domain.UnitsOfMeasure;

namespace TradeFlow.Modules.Catalog.Infrastructure.Persistence.Configurations;

internal sealed class UnitOfMeasureConfiguration : IEntityTypeConfiguration<UnitOfMeasure>
{
    public void Configure(EntityTypeBuilder<UnitOfMeasure> builder)
    {
        builder.ToTable("UnitsOfMeasure", "catalog", tableBuilder =>
        {
            tableBuilder.HasCheckConstraint("CK_UnitsOfMeasure_Code",
                    "[Code] IN ('EA', 'KG', 'L', 'M')");
        });

        builder.HasKey(unitOfMeasure => unitOfMeasure.Id);

        builder.Property(unitOfMeasure => unitOfMeasure.Id).ValueGeneratedNever();

        builder.Property(unitOfMeasure => unitOfMeasure.Name)
            .IsRequired()
            .IsUnicode()
            .HasMaxLength(UnitOfMeasure.MaxNameLength);

        builder.Property(unitOfMeasure => unitOfMeasure.Code)
            .IsUnicode(false)
            .HasMaxLength(UnitOfMeasure.MaxCodeLength)
            .IsRequired();

        builder.Property(unitOfMeasure => unitOfMeasure.Status)
            .IsUnicode(false)
            .HasConversion<string>()
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(unitOfMeasure => unitOfMeasure.RowVersion)
            .IsRowVersion();


        builder.HasIndex(unitOfMeasure => new
        {
            unitOfMeasure.Code
        })
            .IsUnique()
            .HasDatabaseName("UX_UnitsOfMeasure_Code");

        builder.HasData(
            new
            {
                Id = Guid.Parse(
                    "10000000-0000-0000-0000-000000000001"),
                Code = "EA",
                Name = "Each",
                Status = UnitOfMeasureStatus.Active
            },
            new
            {
                Id = Guid.Parse(
                    "10000000-0000-0000-0000-000000000002"),
                Code = "KG",
                Name = "Kilogram",
                Status = UnitOfMeasureStatus.Active
            },
            new
            {
                Id = Guid.Parse(
                    "10000000-0000-0000-0000-000000000003"),
                Code = "L",
                Name = "Litre",
                Status = UnitOfMeasureStatus.Active
            },
            new
            {
                Id = Guid.Parse(
                    "10000000-0000-0000-0000-000000000004"),
                Code = "M",
                Name = "Metre",
                Status = UnitOfMeasureStatus.Active
            });


    }
}
