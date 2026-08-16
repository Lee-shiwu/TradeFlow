using TradeFlow.Api.Infrastructure.Identity;
using TradeFlow.Modules.Catalog.Application.Products.GetProductById;

namespace TradeFlow.Api.Endpoints.Catalog.Products;

public static class GetProductByIdEndpoint
{

    public static IEndpointRouteBuilder MapGetProductByIdEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/v1/catalog/products/{productId:guid}", HandleAsync)
            .WithName("GetProductById")
            .WithTags("Catalog Products")
            .Produces<GetProductByIdResponse>(StatusCodes.Status200OK)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status501NotImplemented);
        return endpoints;

    }

    private static async Task<IResult> HandleAsync(
        Guid productId,
        HttpContext httpContext,
        GetProductByIdHandler handler,
        IHostEnvironment environment,
        CancellationToken cancellationToken)
    {

        IResult? identityError =
            TemporaryIdentityResolver.TryResolve(httpContext, environment, out TemporaryIdentity identity);

        if (identityError is not null)
        {
            return identityError;
        }


        GetProductByIdQuery query = CreateQuery(productId, identity.OrganisationId);

        GetProductByIdResult result =
            await handler.HandleAsync(query, cancellationToken);

        GetProductByIdResponse response = CreateResponse(result);

        return Results.Ok(response);
    }

    private static GetProductByIdResponse CreateResponse(GetProductByIdResult result)
    {
        return new GetProductByIdResponse(
            ProductId: result.ProductId,
            Sku: result.Sku,
            Name: result.Name,
            Description: result.Description,
            UnitOfMeasureId: result.UnitOfMeasureId,
            ProductCategoryId: result.ProductCategoryId,
            TaxCategoryId: result.TaxCategoryId,
            Status: result.Status,
            CreatedAt: result.CreatedAt,
            CreatedBy: result.CreatedBy,
            LastModifiedAt: result.LastModifiedAt,
            LastModifiedBy: result.LastModifiedBy,
            RowVersion: Convert.ToBase64String(result.RowVersion)
            );
    }

    private static GetProductByIdQuery CreateQuery(Guid productId, Guid organisationId)
    {
        return new GetProductByIdQuery(
            ProductId: productId,
            OrganisationId: organisationId
            );
    }

}
