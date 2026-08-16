using TradeFlow.Modules.Catalog.Domain.Products;

namespace TradeFlow.Api.Endpoints.Catalog.Products;

public sealed class ListProductsRequest
{
    public string? Search { get; init; }
    public ProductStatus? Status { get; init; }
    public int? PageNumber { get; init; }
    public int? PageSize { get; init; }
}
