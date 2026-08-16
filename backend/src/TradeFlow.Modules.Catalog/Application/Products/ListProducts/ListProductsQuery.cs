
using TradeFlow.Modules.Catalog.Domain.Products;

namespace TradeFlow.Modules.Catalog.Application.Products.ListProducts;

public sealed record ListProductsQuery
(
    Guid OrganisationId,
    string? Search,
    ProductStatus? Status,
    int PageNumber,
    int PageSize
    );
