using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TradeFlow.Modules.Catalog.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class AddCatalogTaxCategories : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "TaxCategories",
            schema: "catalog",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Code = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false),
                Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false, defaultValue: ""),
                Rate = table.Column<decimal>(type: "decimal(5,4)", precision: 5, scale: 4, nullable: false),
                Treatment = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                Status = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                EffectiveFrom = table.Column<DateOnly>(type: "date", nullable: false),
                EffectiveTo = table.Column<DateOnly>(type: "date", nullable: true),
                RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TaxCategories", x => x.Id);
                table.CheckConstraint("CK_TaxCategories_EffectivePeriod", "[EffectiveTo] IS NULL OR [EffectiveTo] >= [EffectiveFrom]");
                table.CheckConstraint("CK_TaxCategories_Rate", "[Rate] >= 0 AND [Rate] <= 1");
                table.CheckConstraint("CK_TaxCategories_TreatmentRate", "([Treatment] = 'StandardRated' AND [Rate] > 0) OR ([Treatment] IN ('ZeroRated', 'Exempt') AND [Rate] = 0)");
            });

        migrationBuilder.InsertData(
            schema: "catalog",
            table: "TaxCategories",
            columns: new[] { "Id", "Code", "Description", "EffectiveFrom", "EffectiveTo", "Name", "Rate", "Status", "Treatment" },
            values: new object[,]
            {
                { new Guid("20000000-0000-0000-0000-000000000001"), "GST15", "Standard-rated supplies at 15% GST.", new DateOnly(2010, 10, 1), null, "Standard GST", 0.1500m, "Active", "StandardRated" },
                { new Guid("20000000-0000-0000-0000-000000000002"), "GST0", "Zero-rated supplies charged at 0% GST.", new DateOnly(2010, 10, 1), null, "Zero-rated GST", 0m, "Active", "ZeroRated" },
                { new Guid("20000000-0000-0000-0000-000000000003"), "EXEMPT", "Supplies that are exempt from GST.", new DateOnly(2010, 10, 1), null, "GST exempt", 0m, "Active", "Exempt" }
            });

        migrationBuilder.CreateIndex(
            name: "IX_Products_TaxCategoryId",
            schema: "catalog",
            table: "Products",
            column: "TaxCategoryId");

        migrationBuilder.CreateIndex(
            name: "IX_TaxCategories_Status_EffectiveFrom_EffectiveTo",
            schema: "catalog",
            table: "TaxCategories",
            columns: new[] { "Status", "EffectiveFrom", "EffectiveTo" });

        migrationBuilder.CreateIndex(
            name: "UX_TaxCategories_Code",
            schema: "catalog",
            table: "TaxCategories",
            column: "Code",
            unique: true);

        migrationBuilder.AddForeignKey(
            name: "FK_Products_TaxCategories_TaxCategoryId",
            schema: "catalog",
            table: "Products",
            column: "TaxCategoryId",
            principalSchema: "catalog",
            principalTable: "TaxCategories",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_Products_TaxCategories_TaxCategoryId",
            schema: "catalog",
            table: "Products");

        migrationBuilder.DropTable(
            name: "TaxCategories",
            schema: "catalog");

        migrationBuilder.DropIndex(
            name: "IX_Products_TaxCategoryId",
            schema: "catalog",
            table: "Products");
    }
}
