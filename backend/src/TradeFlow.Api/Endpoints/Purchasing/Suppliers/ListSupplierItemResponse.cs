using System.Text.Json.Serialization;
using TradeFlow.Modules.Purchasing.Domain.Suppliers;

namespace TradeFlow.Api.Endpoints.Purchasing.Suppliers;

public sealed record ListSupplierItemResponse(
    Guid SupplierId,
    string Code,
    string Name,
    [property: JsonConverter(
        typeof(JsonStringEnumConverter<SupplierStatus>))]
    SupplierStatus Status,
    DateTimeOffset CreatedAt);
