using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TradeFlow.Modules.Catalog.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class InitialCatalogProducts : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(
            name: "catalog");

        migrationBuilder.CreateTable(
            name: "Products",
            schema: "catalog",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                OrganisationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                SKU = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false, defaultValue: ""),
                UnitOfMeasureId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ProductCategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                TaxCategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Status = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset(7)", precision: 7, nullable: false),
                CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                LastModifiedAt = table.Column<DateTimeOffset>(type: "datetimeoffset(7)", precision: 7, nullable: true),
                LastModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Products", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Products_OrganisationId_Status",
            schema: "catalog",
            table: "Products",
            columns: new[] { "OrganisationId", "Status" });

        migrationBuilder.CreateIndex(
            name: "UX_Products_OrganisationId_Sku",
            schema: "catalog",
            table: "Products",
            columns: new[] { "OrganisationId", "SKU" },
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Products",
            schema: "catalog");
    }
}
