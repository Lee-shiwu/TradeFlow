using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TradeFlow.Modules.Purchasing.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class AddPurchaseOrders : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "PurchaseOrders",
            schema: "purchasing",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                OrganisationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                SupplierId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Reference = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                Status = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PurchaseOrders", x => x.Id);
                table.ForeignKey(
                    name: "FK_PurchaseOrders_Suppliers_SupplierId",
                    column: x => x.SupplierId,
                    principalSchema: "purchasing",
                    principalTable: "Suppliers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "PurchaseOrderLines",
            schema: "purchasing",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                PurchaseOrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ProductSku = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                ProductName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                Quantity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                UnitPrice = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PurchaseOrderLines", x => x.Id);
                table.ForeignKey(
                    name: "FK_PurchaseOrderLines_PurchaseOrders_PurchaseOrderId",
                    column: x => x.PurchaseOrderId,
                    principalSchema: "purchasing",
                    principalTable: "PurchaseOrders",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_PurchaseOrderLines_PurchaseOrderId_ProductId",
            schema: "purchasing",
            table: "PurchaseOrderLines",
            columns: new[] { "PurchaseOrderId", "ProductId" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_PurchaseOrders_OrganisationId_Status_CreatedAt",
            schema: "purchasing",
            table: "PurchaseOrders",
            columns: new[] { "OrganisationId", "Status", "CreatedAt" });

        migrationBuilder.CreateIndex(
            name: "IX_PurchaseOrders_SupplierId",
            schema: "purchasing",
            table: "PurchaseOrders",
            column: "SupplierId");

        migrationBuilder.CreateIndex(
            name: "UX_PurchaseOrders_OrganisationId_Reference",
            schema: "purchasing",
            table: "PurchaseOrders",
            columns: new[] { "OrganisationId", "Reference" },
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "PurchaseOrderLines",
            schema: "purchasing");

        migrationBuilder.DropTable(
            name: "PurchaseOrders",
            schema: "purchasing");
    }
}
