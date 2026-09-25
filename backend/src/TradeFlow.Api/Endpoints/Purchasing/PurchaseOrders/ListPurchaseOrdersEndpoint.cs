using TradeFlow.Api.Infrastructure.Identity;
using TradeFlow.Modules.Purchasing.Application.PurchaseOrders.ListPurchaseOrders;

namespace TradeFlow.Api.Endpoints.Purchasing.PurchaseOrders;

public static class ListPurchaseOrdersEndpoint
{
    public static IEndpointRouteBuilder MapListPurchaseOrdersEndpoint(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/v1/purchasing/purchase-orders", HandleAsync)
            .WithName("ListPurchaseOrders")
            .WithTags("Purchasing Purchase Orders")
            .Produces<ListPurchaseOrdersResponse>(StatusCodes.Status200OK)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status501NotImplemented);
        return endpoints;
    }

    private static async Task<IResult> HandleAsync(
        [AsParameters] ListPurchaseOrdersRequest request,
        HttpContext httpContext,
        ListPurchaseOrdersHandler handler,
        IHostEnvironment environment,
        CancellationToken cancellationToken)
    {
        IResult? identityError = TemporaryIdentityResolver.TryResolve(
            httpContext,
            environment,
            out TemporaryIdentity identity);
        if (identityError is not null)
        {
            return identityError;
        }

        ListPurchaseOrdersResult result = await handler.HandleAsync(
            new ListPurchaseOrdersQuery(
                identity.OrganisationId,
                request.Search,
                request.Status,
                request.PageNumber ?? 1,
                request.PageSize ?? 20),
            cancellationToken);

        return Results.Ok(new ListPurchaseOrdersResponse(
            result.Items.Select(item => new ListPurchaseOrderItemResponse(
                item.PurchaseOrderId,
                item.SupplierId,
                item.SupplierCode,
                item.SupplierName,
                item.Reference,
                item.Status,
                item.LineCount,
                item.TotalAmount,
                item.CreatedAt,
                Convert.ToBase64String(item.RowVersion))).ToList(),
            result.PageNumber,
            result.PageSize,
            result.TotalCount,
            result.TotalPages));
    }
}
