using TradeFlow.Api.Infrastructure.Identity;
using TradeFlow.Modules.Catalog.Application.ProductCategories.GetProductCategoryById;

namespace TradeFlow.Api.Endpoints.Catalog.ProductCategories;

public static class GetProductCategoryByIdEndpoint
{
    public static IEndpointRouteBuilder MapGetProductCategoryByIdEndpoint(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet(
                "/api/v1/catalog/product-categories/{productCategoryId:guid}",
                HandleAsync)
            .WithName("GetProductCategoryById")
            .WithTags("Catalog Product Categories")
            .Produces<GetProductCategoryByIdResponse>(StatusCodes.Status200OK)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status501NotImplemented);

        return endpoints;
    }

    private static async Task<IResult> HandleAsync(
        Guid productCategoryId,
        HttpContext httpContext,
        GetProductCategoryByIdHandler handler,
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

        GetProductCategoryByIdQuery query =
            new(
                OrganisationId: identity.OrganisationId,
                ProductCategoryId: productCategoryId);

        GetProductCategoryByIdResult result =
            await handler.HandleAsync(query, cancellationToken);

        return Results.Ok(CreateResponse(result));
    }

    private static GetProductCategoryByIdResponse CreateResponse(
        GetProductCategoryByIdResult result)
    {
        return new GetProductCategoryByIdResponse(
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
}
