using TradeFlow.Api.Infrastructure.Identity;
using TradeFlow.Modules.Catalog.Application.ProductCategories.UpdateProductCategoryDetails;

namespace TradeFlow.Api.Endpoints.Catalog.ProductCategories;

public static class UpdateProductCategoryDetailsEndpoint
{
    public static IEndpointRouteBuilder MapUpdateProductCategoryDetailsEndpoint(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPut(
                "/api/v1/catalog/product-categories/{productCategoryId:guid}",
                HandleAsync)
            .WithName("UpdateProductCategoryDetails")
            .WithTags("Catalog Product Categories")
            .Accepts<UpdateProductCategoryDetailsRequest>("application/json")
            .Produces<UpdateProductCategoryDetailsResponse>(
                StatusCodes.Status200OK)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status501NotImplemented);

        return endpoints;
    }

    private static async Task<IResult> HandleAsync(
        Guid productCategoryId,
        UpdateProductCategoryDetailsRequest request,
        HttpContext httpContext,
        UpdateProductCategoryDetailsHandler handler,
        IHostEnvironment environment,
        CancellationToken cancellationToken)
    {
        IResult? identityError =
            TemporaryIdentityResolver.TryResolve(
                httpContext,
                environment,
                out TemporaryIdentity identity);

        if (identityError is not null)
        {
            return identityError;
        }

        if (!TryDecodeRowVersion(request.RowVersion, out byte[] rowVersion))
        {
            return CreateInvalidRowVersionResult();
        }

        UpdateProductCategoryDetailsCommand command =
            new(
                OrganisationId: identity.OrganisationId,
                ProductCategoryId: productCategoryId,
                Name: request.Name,
                Description: request.Description,
                ModifiedBy: identity.UserId,
                RowVersion: rowVersion);

        UpdateProductCategoryDetailsResult result =
            await handler.HandleAsync(command, cancellationToken);

        return Results.Ok(CreateResponse(result));
    }

    private static UpdateProductCategoryDetailsResponse CreateResponse(
        UpdateProductCategoryDetailsResult result)
    {
        return new UpdateProductCategoryDetailsResponse(
            ProductCategoryId: result.ProductCategoryId,
            Code: result.Code,
            Name: result.Name,
            Description: result.Description,
            Status: result.Status,
            CreatedAt: result.CreatedAt,
            CreatedBy: result.CreatedBy,
            LastModifiedAt: result.LastModifiedAt,
            LastModifiedBy: result.LastModifiedBy,
            RowVersion: Convert.ToBase64String(result.RowVersion));
    }

    private static bool TryDecodeRowVersion(
        string? rowVersion,
        out byte[] value)
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
}
