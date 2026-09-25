using System.Text.Json.Serialization;
using TradeFlow.Modules.Purchasing.Domain.PurchaseOrders;

namespace TradeFlow.Api.Endpoints.Purchasing.PurchaseOrders;

public sealed record ConfirmPurchaseOrderResponse(
    Guid PurchaseOrderId,
    [property: JsonConverter(typeof(JsonStringEnumConverter<PurchaseOrderStatus>))]
    PurchaseOrderStatus Status,
    DateTimeOffset ConfirmedAt,
    Guid ConfirmedBy,
    string RowVersion);
