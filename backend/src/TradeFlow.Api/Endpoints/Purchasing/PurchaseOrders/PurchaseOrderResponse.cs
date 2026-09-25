using System.Text.Json.Serialization;
using TradeFlow.Modules.Purchasing.Domain.PurchaseOrders;

namespace TradeFlow.Api.Endpoints.Purchasing.PurchaseOrders;

public sealed record PurchaseOrderResponse(
    Guid PurchaseOrderId,
    Guid SupplierId,
    string SupplierCode,
    string SupplierName,
    string Reference,
    [property: JsonConverter(typeof(JsonStringEnumConverter<PurchaseOrderStatus>))]
    PurchaseOrderStatus Status,
    IReadOnlyList<PurchaseOrderLineResponse> Lines,
    decimal TotalAmount,
    DateTimeOffset CreatedAt,
    Guid CreatedBy,
    string RowVersion);

public sealed record PurchaseOrderLineResponse(
    Guid LineId,
    Guid ProductId,
    string ProductSku,
    string ProductName,
    decimal Quantity,
    decimal UnitPrice,
    decimal LineTotal);
