using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TradeFlow.Modules.Catalog.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class AddCatalogUnitsOfMeasure : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "UnitsOfMeasure",
            schema: "catalog",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Code = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: false),
                Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                Status = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_UnitsOfMeasure", x => x.Id);
                table.CheckConstraint("CK_UnitsOfMeasure_Code", "[Code] IN ('EA', 'KG', 'L', 'M')");
            });

        migrationBuilder.InsertData(
            schema: "catalog",
            table: "UnitsOfMeasure",
            columns: new[] { "Id", "Code", "Name", "Status" },
            values: new object[,]
            {
                { new Guid("10000000-0000-0000-0000-000000000001"), "EA", "Each", "Active" },
                { new Guid("10000000-0000-0000-0000-000000000002"), "KG", "Kilogram", "Active" },
                { new Guid("10000000-0000-0000-0000-000000000003"), "L", "Litre", "Active" },
                { new Guid("10000000-0000-0000-0000-000000000004"), "M", "Metre", "Active" }
            });

        migrationBuilder.CreateIndex(
            name: "IX_Products_UnitOfMeasureId",
            schema: "catalog",
            table: "Products",
            column: "UnitOfMeasureId");

        migrationBuilder.CreateIndex(
            name: "UX_UnitsOfMeasure_Code",
            schema: "catalog",
            table: "UnitsOfMeasure",
            column: "Code",
            unique: true);

        migrationBuilder.AddForeignKey(
            name: "FK_Products_UnitsOfMeasure_UnitOfMeasureId",
            schema: "catalog",
            table: "Products",
            column: "UnitOfMeasureId",
            principalSchema: "catalog",
            principalTable: "UnitsOfMeasure",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_Products_UnitsOfMeasure_UnitOfMeasureId",
            schema: "catalog",
            table: "Products");

        migrationBuilder.DropTable(
            name: "UnitsOfMeasure",
            schema: "catalog");

        migrationBuilder.DropIndex(
            name: "IX_Products_UnitOfMeasureId",
            schema: "catalog",
            table: "Products");
    }
}
