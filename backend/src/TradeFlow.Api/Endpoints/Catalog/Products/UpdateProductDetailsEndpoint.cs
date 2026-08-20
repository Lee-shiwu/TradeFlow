using TradeFlow.Api.Infrastructure.Identity;
using TradeFlow.Modules.Catalog.Application.Products.UpdateProductDetails;

namespace TradeFlow.Api.Endpoints.Catalog.Products;

public static class UpdateProductDetailsEndpoint
{
    public static IEndpointRouteBuilder MapUpdateProductDetailsEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPut("/api/v1/catalog/products/{productId:guid}", HandleAsync)
            .WithName("UpdateProductDetails")
            .WithTags("Catalog Products")
            .Accepts<UpdateProductDetailsRequest>("application/json")
            .Produces<UpdateProductDetailsResponse>(StatusCodes.Status200OK)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status501NotImplemented);
        return endpoints;

    }

    private static async Task<IResult> HandleAsync(
        Guid productId,
        UpdateProductDetailsRequest request,
        HttpContext httpContext,
        UpdateProductDetailsHandler handler,
        IHostEnvironment environment,
        CancellationToken cancellationToken)
    {
        IResult? identityError =
            TemporaryIdentityResolver.TryResolve(httpContext, environment, out TemporaryIdentity identity);

        if (identityError is not null)
        {
            return identityError;
        }

        if (!TryDecodeRowVersion(request.RowVersion, out byte[] rowVersion))
        {
            return CreateInvalidRowVersionResult();
        }

        UpdateProductDetailsCommand command = CreateCommand(productId, request, identity, rowVersion);

        UpdateProductDetailsResult result = await handler.HandleAsync(command, cancellationToken);

        UpdateProductDetailsResponse response = CreateResponse(result);

        return Results.Ok(response);

    }

    private static UpdateProductDetailsResponse CreateResponse(UpdateProductDetailsResult result)
    {
        return new UpdateProductDetailsResponse(
            ProductId: result.ProductId,
            Name: result.Name,
            Description: result.Description,
            ProductCategoryId: result.ProductCategoryId,
            TaxCategoryId: result.TaxCategoryId,
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

    private static UpdateProductDetailsCommand CreateCommand(Guid productId, UpdateProductDetailsRequest request, TemporaryIdentity identity, byte[] rowVersion)
    {

        return new UpdateProductDetailsCommand(
            OrganisationId: identity.OrganisationId,
            ProductId: productId,
            Name: request.Name,
            Description: request.Description ?? string.Empty,
            ProductCategoryId: request.ProductCategoryId,
            TaxCategoryId: request.TaxCategoryId,
            ModifiedBy: identity.UserId,
            RowVersion: rowVersion
            );
    }



}
