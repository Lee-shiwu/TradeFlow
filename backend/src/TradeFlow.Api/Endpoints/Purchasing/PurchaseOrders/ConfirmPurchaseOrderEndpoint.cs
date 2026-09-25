using TradeFlow.Api.Infrastructure.Identity;
using TradeFlow.Modules.Purchasing.Application.PurchaseOrders.ConfirmPurchaseOrder;

namespace TradeFlow.Api.Endpoints.Purchasing.PurchaseOrders;

public static class ConfirmPurchaseOrderEndpoint
{
    public static IEndpointRouteBuilder MapConfirmPurchaseOrderEndpoint(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost(
                "/api/v1/purchasing/purchase-orders/{purchaseOrderId:guid}/confirm",
                HandleAsync)
            .WithName("ConfirmPurchaseOrder")
            .WithTags("Purchasing Purchase Orders")
            .Accepts<ConfirmPurchaseOrderRequest>("application/json")
            .Produces<ConfirmPurchaseOrderResponse>(StatusCodes.Status200OK)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status501NotImplemented);
        return endpoints;
    }

    private static async Task<IResult> HandleAsync(
        Guid purchaseOrderId,
        ConfirmPurchaseOrderRequest request,
        HttpContext httpContext,
        ConfirmPurchaseOrderHandler handler,
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

        if (!TryDecodeRowVersion(request.RowVersion, out byte[] rowVersion))
        {
            return Results.ValidationProblem(
                new Dictionary<string, string[]>
                {
                    ["RowVersion"] = ["RowVersion must be a valid non-empty Base64 value."],
                });
        }

        ConfirmPurchaseOrderResult result = await handler.HandleAsync(
            new ConfirmPurchaseOrderCommand(
                identity.OrganisationId,
                purchaseOrderId,
                identity.UserId,
                rowVersion),
            cancellationToken);

        return Results.Ok(new ConfirmPurchaseOrderResponse(
            result.PurchaseOrderId,
            result.Status,
            result.ConfirmedAt,
            result.ConfirmedBy,
            Convert.ToBase64String(result.RowVersion)));
    }

    private static bool TryDecodeRowVersion(string? encoded, out byte[] rowVersion)
    {
        rowVersion = [];
        if (string.IsNullOrWhiteSpace(encoded))
        {
            return false;
        }

        try
        {
            rowVersion = Convert.FromBase64String(encoded);
            return rowVersion.Length > 0;
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
