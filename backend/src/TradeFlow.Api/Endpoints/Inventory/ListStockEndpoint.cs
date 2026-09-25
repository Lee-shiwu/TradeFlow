using TradeFlow.Api.Infrastructure.Identity;
using TradeFlow.Modules.Purchasing.Application.Inventory.ListStock;

namespace TradeFlow.Api.Endpoints.Inventory;

public static class ListStockEndpoint
{
    public static IEndpointRouteBuilder MapListStockEndpoint(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/v1/inventory/stock", HandleAsync)
            .WithName("ListStock")
            .WithTags("Inventory")
            .Produces<ListStockResponse>(StatusCodes.Status200OK)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status501NotImplemented);
        return endpoints;
    }

    private static async Task<IResult> HandleAsync(
        [AsParameters] ListStockRequest request,
        HttpContext httpContext,
        ListStockHandler handler,
        IHostEnvironment environment,
        CancellationToken cancellationToken)
    {
        IResult? identityError = TemporaryIdentityResolver.TryResolve(
            httpContext,
            environment,
            out TemporaryIdentity identity);
        if (identityError is not null) return identityError;

        ListStockResult result = await handler.HandleAsync(
            new ListStockQuery(
                identity.OrganisationId,
                request.Search,
                request.PageNumber ?? 1,
                request.PageSize ?? 20),
            cancellationToken);

        return Results.Ok(new ListStockResponse(
            result.Items.Select(item => new StockItemResponse(
                item.ProductId,
                item.ProductSku,
                item.ProductName,
                item.QuantityOnHand,
                item.LastMovementAt)).ToList(),
            result.PageNumber,
            result.PageSize,
            result.TotalCount,
            result.TotalPages));
    }
}
