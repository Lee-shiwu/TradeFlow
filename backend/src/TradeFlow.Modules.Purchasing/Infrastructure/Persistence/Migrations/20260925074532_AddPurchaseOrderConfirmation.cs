using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TradeFlow.Modules.Purchasing.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class AddPurchaseOrderConfirmation : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<DateTimeOffset>(
            name: "ConfirmedAt",
            schema: "purchasing",
            table: "PurchaseOrders",
            type: "datetimeoffset",
            nullable: true);

        migrationBuilder.AddColumn<Guid>(
            name: "ConfirmedBy",
            schema: "purchasing",
            table: "PurchaseOrders",
            type: "uniqueidentifier",
            nullable: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "ConfirmedAt",
            schema: "purchasing",
            table: "PurchaseOrders");

        migrationBuilder.DropColumn(
            name: "ConfirmedBy",
            schema: "purchasing",
            table: "PurchaseOrders");
    }
}
