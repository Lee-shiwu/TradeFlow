using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TradeFlow.Modules.Purchasing.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class AddGoodsReceiptsAndStockTransactions : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<DateTimeOffset>(
            name: "ReceivedAt",
            schema: "purchasing",
            table: "PurchaseOrders",
            type: "datetimeoffset",
            nullable: true);

        migrationBuilder.AddColumn<Guid>(
            name: "ReceivedBy",
            schema: "purchasing",
            table: "PurchaseOrders",
            type: "uniqueidentifier",
            nullable: true);

        migrationBuilder.CreateTable(
            name: "GoodsReceipts",
            schema: "purchasing",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                OrganisationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                PurchaseOrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ReceiptNumber = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: false),
                ReceivedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                ReceivedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_GoodsReceipts", x => x.Id);
                table.ForeignKey(
                    name: "FK_GoodsReceipts_PurchaseOrders_PurchaseOrderId",
                    column: x => x.PurchaseOrderId,
                    principalSchema: "purchasing",
                    principalTable: "PurchaseOrders",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "GoodsReceiptLines",
            schema: "purchasing",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                GoodsReceiptId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                PurchaseOrderLineId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ProductSku = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                ProductName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                QuantityReceived = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_GoodsReceiptLines", x => x.Id);
                table.ForeignKey(
                    name: "FK_GoodsReceiptLines_GoodsReceipts_GoodsReceiptId",
                    column: x => x.GoodsReceiptId,
                    principalSchema: "purchasing",
                    principalTable: "GoodsReceipts",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "StockTransactions",
            schema: "purchasing",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                OrganisationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ProductSku = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                ProductName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                Type = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false),
                Quantity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                SourceDocumentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                SourceLineId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                OccurredAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_StockTransactions", x => x.Id);
                table.ForeignKey(
                    name: "FK_StockTransactions_GoodsReceiptLines_SourceLineId",
                    column: x => x.SourceLineId,
                    principalSchema: "purchasing",
                    principalTable: "GoodsReceiptLines",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_GoodsReceiptLines_GoodsReceiptId",
            schema: "purchasing",
            table: "GoodsReceiptLines",
            column: "GoodsReceiptId");

        migrationBuilder.CreateIndex(
            name: "IX_GoodsReceiptLines_PurchaseOrderLineId",
            schema: "purchasing",
            table: "GoodsReceiptLines",
            column: "PurchaseOrderLineId",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_GoodsReceipts_OrganisationId_ReceiptNumber",
            schema: "purchasing",
            table: "GoodsReceipts",
            columns: new[] { "OrganisationId", "ReceiptNumber" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_GoodsReceipts_PurchaseOrderId",
            schema: "purchasing",
            table: "GoodsReceipts",
            column: "PurchaseOrderId",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_StockTransactions_OrganisationId_ProductId_OccurredAt",
            schema: "purchasing",
            table: "StockTransactions",
            columns: new[] { "OrganisationId", "ProductId", "OccurredAt" });

        migrationBuilder.CreateIndex(
            name: "IX_StockTransactions_SourceLineId",
            schema: "purchasing",
            table: "StockTransactions",
            column: "SourceLineId",
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "StockTransactions",
            schema: "purchasing");

        migrationBuilder.DropTable(
            name: "GoodsReceiptLines",
            schema: "purchasing");

        migrationBuilder.DropTable(
            name: "GoodsReceipts",
            schema: "purchasing");

        migrationBuilder.DropColumn(
            name: "ReceivedAt",
            schema: "purchasing",
            table: "PurchaseOrders");

        migrationBuilder.DropColumn(
            name: "ReceivedBy",
            schema: "purchasing",
            table: "PurchaseOrders");
    }
}
