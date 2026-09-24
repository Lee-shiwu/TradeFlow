using TradeFlow.Api.Infrastructure.Identity;
using TradeFlow.Modules.Catalog.Application.ProductCategories.ActivateProductCategory;

namespace TradeFlow.Api.Endpoints.Catalog.ProductCategories;

public static class ActivateProductCategoryEndpoint
{
    public static IEndpointRouteBuilder MapActivateProductCategoryEndpoint(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost(
                "/api/v1/catalog/product-categories/{productCategoryId:guid}/activate",
                HandleAsync)
            .WithName("ActivateProductCategory")
            .WithTags("Catalog Product Categories")
            .Accepts<ProductCategoryStatusRequest>("application/json")
            .Produces<ProductCategoryStatusResponse>(StatusCodes.Status200OK)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status501NotImplemented);

        return endpoints;
    }

    private static async Task<IResult> HandleAsync(
        Guid productCategoryId,
        ProductCategoryStatusRequest request,
        HttpContext httpContext,
        ActivateProductCategoryHandler handler,
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

        if (!ProductCategoryStatusEndpointHelper.TryDecodeRowVersion(
                request.RowVersion,
                out byte[] rowVersion))
        {
            return ProductCategoryStatusEndpointHelper
                .CreateInvalidRowVersionResult();
        }

        ActivateProductCategoryResult result =
            await handler.HandleAsync(
                new ActivateProductCategoryCommand(
                    identity.OrganisationId,
                    productCategoryId,
                    identity.UserId,
                    rowVersion),
                cancellationToken);

        return Results.Ok(
            new ProductCategoryStatusResponse(
                result.ProductCategoryId,
                result.Status,
                result.LastModifiedAt,
                result.LastModifiedBy,
                Convert.ToBase64String(result.RowVersion)));
    }
}
