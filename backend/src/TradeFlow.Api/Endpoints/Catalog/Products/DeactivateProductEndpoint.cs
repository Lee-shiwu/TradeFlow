using TradeFlow.Api.Infrastructure.Identity;
using TradeFlow.Modules.Catalog.Application.Products.DeactivateProduct;

namespace TradeFlow.Api.Endpoints.Catalog.Products;

public static class DeactivateProductEndpoint
{
    public static IEndpointRouteBuilder MapDeactivateProductEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/v1/catalog/products/{productId:guid}/deactivate", HandleAsync)
            .WithName("DeactivateProduct")
            .WithTags("Catalog Products")
            .Accepts<DeactivateProductRequest>("application/json")
            .Produces<DeactivateProductResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status501NotImplemented)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest);

        return endpoints;

    }

    private static async Task<IResult> HandleAsync(
        Guid productId,
        DeactivateProductRequest request,
        DeactivateProductHandler handler,
        IHostEnvironment environment,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        IResult? identityError =
            TemporaryIdentityResolver.TryResolve(httpContext, environment, out TemporaryIdentity identity);

        if(identityError is not null)
        {
            return identityError;
        }

        if(!TryDecodeRowVersion(request.RowVersion, out byte[] rowVersion))
        {
            return CreateInvalidRowVersionResult();
        }

        DeactivateProductCommand command = CreateCommand(productId, identity, rowVersion);

        DeactivateProductResult result = await handler.HandleAsync(command, cancellationToken);

        DeactivateProductResponse response = CreateResponse(result);

        return Results.Ok(response);
    }

    private static DeactivateProductResponse CreateResponse (DeactivateProductResult result)
    {
        return new DeactivateProductResponse(

            ProductId: result.ProductId,
            Status: result.Status,
            LastModifiedAt: result.LastModifiedAt,
            LastModifiedBy: result.LastModifiedBy,
            RowVersion: Convert.ToBase64String(result.RowVersion)
            );

    }

    private static bool TryDecodeRowVersion(string? rowVersion, out byte[] value)
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
                ["RowVersion"] =
                [
                 "RowVersion must be a valid non-empty Base64 value."
                 ]
            };
        return Results.ValidationProblem(errors);

    }

    private static DeactivateProductCommand CreateCommand(
        Guid productId,
        TemporaryIdentity identity,
        byte[] rowVersion)
    {
        return new DeactivateProductCommand(
            OrganisationId: identity.OrganisationId,
            ProductId: productId,
            ModifiedBy: identity.UserId,
            RowVersion: rowVersion
            );
    }
}
