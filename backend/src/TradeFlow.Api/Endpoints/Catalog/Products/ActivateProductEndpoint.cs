using TradeFlow.Modules.Catalog.Application.Products.ActivateProduct;
using TradeFlow.Api.Infrastructure.Identity;

namespace TradeFlow.Api.Endpoints.Catalog.Products;

public static class ActivateProductEndpoint
{
    public static IEndpointRouteBuilder MapActivateProductEndpoint(this IEndpointRouteBuilder endpoints )
    {
        endpoints.MapPost("/api/v1/catalog/products/{productId:guid}/activate", HandleAsync)
            .WithName("ActivateProduct")
            .WithTags("Catalog Products")
            .Accepts<ActivateProductRequest>("application/json")
            .Produces<ActivateProductResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status501NotImplemented)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest);

        return endpoints;
    }

    private static async Task<IResult> HandleAsync(
        Guid productId,
        ActivateProductRequest request,
        HttpContext httpContext,
        ActivateProductHandler handler,
        IHostEnvironment environment,
        CancellationToken cancellationToken)
    {
        IResult? identityError =
             TemporaryIdentityResolver.TryResolve(httpContext, environment, out TemporaryIdentity identity);

        if(identityError is not null)
        {
            return identityError;
        }

        if(!TryDecodeRowVersion(request.RowVersion,out byte[] rowVersion))
        {
            return CreateInvalidRowVersionResult();
        }

        ActivateProductCommand command = CreateCommand(productId, identity, rowVersion);
        ActivateProductResult result = await handler.HandleAsync(command, cancellationToken);
        ActivateProductResponse response = CreateResponse(result);

        return Results.Ok(response);

    }

    private static ActivateProductResponse CreateResponse(ActivateProductResult result)
    {
        return new ActivateProductResponse(
            ProductId: result.ProductId,
            Status: result.Status,
            LastModifiedAt: result.LastModifiedAt,
            LastModifiedBy: result.LastModifiedBy,
            RowVersion: Convert.ToBase64String(result.RowVersion)
            );
    }

    private static ActivateProductCommand CreateCommand(Guid productId, TemporaryIdentity identity, byte[] rowVersion)
    {
        return new ActivateProductCommand(
            ProductId: productId,
            OrganisationId: identity.OrganisationId,
            ModifiedBy: identity.UserId,
            RowVersion: rowVersion
            );
    }

    private static bool TryDecodeRowVersion(string? rowVersion,out byte[] value)
    {
        value = Array.Empty<byte>();

        if (string.IsNullOrWhiteSpace(rowVersion))
        {
            return false;
        }
        try
        {
        value = Convert.FromBase64String(rowVersion);

        }
        catch (FormatException)
        {

            return false;
        }

        return value.Length > 0;
    }

    private static IResult CreateInvalidRowVersionResult()
    {
        Dictionary<string, string[]> errors =
            new()
            {
                ["RowVersion"]=
                ["RowVersion must be a valid non-empty Base64 value."]
            };

        return Results.ValidationProblem(errors);
    }
}
