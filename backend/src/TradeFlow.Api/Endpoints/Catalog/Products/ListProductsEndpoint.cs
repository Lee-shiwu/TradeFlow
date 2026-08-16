using TradeFlow.Api.Infrastructure.Identity;
using TradeFlow.Modules.Catalog.Application.Products.ListProducts;

namespace TradeFlow.Api.Endpoints.Catalog.Products;

public static class ListProductsEndpoint
{

    public static IEndpointRouteBuilder MapListProductsEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/v1/catalog/products", HandleAsync)
            .WithName("ListProducts")
            .WithTags("Catalog Products")
            .Produces<ListProductsResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status501NotImplemented);

        return endpoints;
    }

    private static async Task<IResult> HandleAsync(
        [AsParameters] ListProductsRequest request,
        HttpContext httpContext,
        ListProductsHandler handler,
        IHostEnvironment environment,
        CancellationToken cancellationToken)
    {
        IResult? identityError =
            TemporaryIdentityResolver.TryResolve(httpContext, environment, out TemporaryIdentity identity);

        if (identityError is not null)
        {
            return identityError;
        }


        ListProductsQuery query = CreateQuery(request, identity.OrganisationId);

        ListProductsResult result =
            await handler.HandleAsync(query, cancellationToken);

        ListProductsResponse response = CreateResponse(result);

        return Results.Ok(response);
    }

    private static ListProductsResponse CreateResponse(ListProductsResult result)
    {
        List<ListProductItemResponse> items =
            result.Items
                .Select(item =>
                    new ListProductItemResponse(
                        ProductId: item.ProductId,
                        Sku: item.Sku,
                        Name: item.Name,
                        UnitOfMeasureId: item.UnitOfMeasureId,
                        ProductCategoryId: item.ProductCategoryId,
                        TaxCategoryId: item.TaxCategoryId,
                        Status: item.Status,
                        CreatedAt: item.CreatedAt,
                        LastModifiedAt: item.LastModifiedAt))
                .ToList();

        return new ListProductsResponse(
            Items: items,
            PageNumber: result.PageNumber,
            PageSize: result.PageSize,
            TotalCount: result.TotalCount,
            TotalPages: result.TotalPages);
    }

    private static ListProductsQuery CreateQuery(ListProductsRequest request, Guid organisationId)
    {
        return new ListProductsQuery(
            OrganisationId: organisationId,
            Search: request.Search,
            Status: request.Status,
            PageNumber: request.PageNumber ?? 1,
            PageSize: request.PageSize ?? 20);
    }

}
