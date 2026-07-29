using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TradeFlow.Modules.Catalog.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class AddCatalogProductCategories : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "ProductCategories",
            schema: "catalog",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                OrganisationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Code = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false, defaultValue: ""),
                Status = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset(7)", precision: 7, nullable: false),
                CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                LastModifiedAt = table.Column<DateTimeOffset>(type: "datetimeoffset(7)", precision: 7, nullable: true),
                LastModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ProductCategories", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Products_ProductCategoryId",
            schema: "catalog",
            table: "Products",
            column: "ProductCategoryId");

        migrationBuilder.CreateIndex(
            name: "IX_ProductCategories_OrganisationId_Status",
            schema: "catalog",
            table: "ProductCategories",
            columns: new[] { "OrganisationId", "Status" });

        migrationBuilder.CreateIndex(
            name: "UX_ProductCategories_OrganisationId_Code",
            schema: "catalog",
            table: "ProductCategories",
            columns: new[] { "OrganisationId", "Code" },
            unique: true);

        migrationBuilder.AddForeignKey(
            name: "FK_Products_ProductCategories_ProductCategoryId",
            schema: "catalog",
            table: "Products",
            column: "ProductCategoryId",
            principalSchema: "catalog",
            principalTable: "ProductCategories",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_Products_ProductCategories_ProductCategoryId",
            schema: "catalog",
            table: "Products");

        migrationBuilder.DropTable(
            name: "ProductCategories",
            schema: "catalog");

        migrationBuilder.DropIndex(
            name: "IX_Products_ProductCategoryId",
            schema: "catalog",
            table: "Products");
    }
}
