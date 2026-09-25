using TradeFlow.Api.Infrastructure.Identity;
using TradeFlow.Modules.Purchasing.Application.PurchaseOrders.GetPurchaseOrderById;

namespace TradeFlow.Api.Endpoints.Purchasing.PurchaseOrders;

public static class GetPurchaseOrderByIdEndpoint
{
    public static IEndpointRouteBuilder MapGetPurchaseOrderByIdEndpoint(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/v1/purchasing/purchase-orders/{purchaseOrderId:guid}", HandleAsync)
            .WithName("GetPurchaseOrderById")
            .WithTags("Purchasing Purchase Orders")
            .Produces<PurchaseOrderResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status501NotImplemented);
        return endpoints;
    }

    private static async Task<IResult> HandleAsync(
        Guid purchaseOrderId,
        HttpContext httpContext,
        GetPurchaseOrderByIdHandler handler,
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

        GetPurchaseOrderByIdResult result = await handler.HandleAsync(
            identity.OrganisationId,
            purchaseOrderId,
            cancellationToken);

        return Results.Ok(new PurchaseOrderResponse(
            result.PurchaseOrderId,
            result.SupplierId,
            result.SupplierCode,
            result.SupplierName,
            result.Reference,
            result.Status,
            result.Lines.Select(line => new PurchaseOrderLineResponse(
                line.LineId,
                line.ProductId,
                line.ProductSku,
                line.ProductName,
                line.Quantity,
                line.UnitPrice,
                line.LineTotal)).ToList(),
            result.TotalAmount,
            result.CreatedAt,
            result.CreatedBy,
            Convert.ToBase64String(result.RowVersion)));
    }
}
