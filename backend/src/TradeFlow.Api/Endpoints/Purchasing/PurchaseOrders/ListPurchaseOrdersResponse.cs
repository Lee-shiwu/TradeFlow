using System.Text.Json.Serialization;
using TradeFlow.Modules.Purchasing.Domain.PurchaseOrders;

namespace TradeFlow.Api.Endpoints.Purchasing.PurchaseOrders;

public sealed record ListPurchaseOrdersResponse(
    IReadOnlyList<ListPurchaseOrderItemResponse> Items,
    int PageNumber,
    int PageSize,
    int TotalCount,
    int TotalPages);

public sealed record ListPurchaseOrderItemResponse(
    Guid PurchaseOrderId,
    Guid SupplierId,
    string SupplierCode,
    string SupplierName,
    string Reference,
    [property: JsonConverter(typeof(JsonStringEnumConverter<PurchaseOrderStatus>))]
    PurchaseOrderStatus Status,
    int LineCount,
    decimal TotalAmount,
    DateTimeOffset CreatedAt,
    string RowVersion);
