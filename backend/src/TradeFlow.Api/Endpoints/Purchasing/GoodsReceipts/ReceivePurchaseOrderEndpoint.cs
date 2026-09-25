using TradeFlow.Api.Infrastructure.Identity;
using TradeFlow.Modules.Purchasing.Application.GoodsReceipts.ReceivePurchaseOrder;

namespace TradeFlow.Api.Endpoints.Purchasing.GoodsReceipts;

public static class ReceivePurchaseOrderEndpoint
{
    public static IEndpointRouteBuilder MapReceivePurchaseOrderEndpoint(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost(
                "/api/v1/purchasing/purchase-orders/{purchaseOrderId:guid}/receive",
                HandleAsync)
            .WithName("ReceivePurchaseOrder")
            .WithTags("Purchasing Goods Receipts")
            .Accepts<ReceivePurchaseOrderRequest>("application/json")
            .Produces<ReceivePurchaseOrderResponse>(StatusCodes.Status200OK)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status501NotImplemented);
        return endpoints;
    }

    private static async Task<IResult> HandleAsync(
        Guid purchaseOrderId,
        ReceivePurchaseOrderRequest request,
        HttpContext httpContext,
        ReceivePurchaseOrderHandler handler,
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

        ReceivePurchaseOrderResult result = await handler.HandleAsync(
            new ReceivePurchaseOrderCommand(
                identity.OrganisationId,
                purchaseOrderId,
                identity.UserId,
                rowVersion),
            cancellationToken);

        return Results.Ok(new ReceivePurchaseOrderResponse(
            result.GoodsReceiptId,
            result.ReceiptNumber,
            result.PurchaseOrderId,
            result.Lines.Select(line => new ReceivePurchaseOrderLineResponse(
                line.ProductId,
                line.ProductSku,
                line.ProductName,
                line.QuantityReceived)).ToList(),
            result.ReceivedAt,
            result.ReceivedBy,
            Convert.ToBase64String(result.PurchaseOrderRowVersion)));
    }

    private static bool TryDecodeRowVersion(string? encoded, out byte[] rowVersion)
    {
        rowVersion = [];
        if (string.IsNullOrWhiteSpace(encoded)) return false;
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
