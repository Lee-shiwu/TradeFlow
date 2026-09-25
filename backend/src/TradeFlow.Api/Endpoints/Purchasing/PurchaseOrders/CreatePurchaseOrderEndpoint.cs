using TradeFlow.Api.Infrastructure.Identity;
using TradeFlow.Modules.Purchasing.Application.PurchaseOrders.CreatePurchaseOrder;

namespace TradeFlow.Api.Endpoints.Purchasing.PurchaseOrders;

public static class CreatePurchaseOrderEndpoint
{
    public static IEndpointRouteBuilder MapCreatePurchaseOrderEndpoint(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/v1/purchasing/purchase-orders", HandleAsync)
            .WithName("CreatePurchaseOrder")
            .WithTags("Purchasing Purchase Orders")
            .Accepts<CreatePurchaseOrderRequest>("application/json")
            .Produces<PurchaseOrderResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status501NotImplemented);
        return endpoints;
    }

    private static async Task<IResult> HandleAsync(
        CreatePurchaseOrderRequest request,
        HttpContext httpContext,
        CreatePurchaseOrderHandler handler,
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

        CreatePurchaseOrderResult result = await handler.HandleAsync(
            new CreatePurchaseOrderCommand(
                identity.OrganisationId,
                request.SupplierId,
                request.Reference,
                (request.Lines ?? []).Select(line =>
                    new CreatePurchaseOrderLineCommand(
                        line.ProductId,
                        line.Quantity,
                        line.UnitPrice)).ToList(),
                identity.UserId),
            cancellationToken);

        PurchaseOrderResponse response = new(
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
            Convert.ToBase64String(result.RowVersion));

        return Results.Created(
            $"/api/v1/purchasing/purchase-orders/{result.PurchaseOrderId}",
            response);
    }
}
