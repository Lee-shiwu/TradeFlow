using Microsoft.AspNetCore.Builder;
using TradeFlow.Api.Infrastructure.Identity;
using TradeFlow.Modules.Catalog.Application.Products.CreateProduct;

namespace TradeFlow.Api.Endpoints.Catalog.Products;


public static class CreateProductEndpoint
{
    public static IEndpointRouteBuilder MapCreateProductEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/v1/catalog/products", HandleAsync)
            .WithName("CreateProduct")
            .WithTags("Catalog Products")
            .Accepts<CreateProductRequest>("application/json")
            .Produces<CreateProductResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status501NotImplemented);

        return endpoints;
    }

    private static async Task<IResult> HandleAsync(CreateProductRequest request,
            HttpContext httpContext,
            CreateProductHandler handler,
            IHostEnvironment environment,
            CancellationToken cancellationToken)
    {
        IResult? identityError =
            TemporaryIdentityResolver.TryResolve(httpContext, environment, out TemporaryIdentity identity);

        if (identityError is not null)
        {
            return identityError;
        }

        CreateProductCommand command = CreateCommand(identity.OrganisationId, identity.UserId, request);

        CreateProductResult result =
            await handler.HandleAsync(command, cancellationToken);

        CreateProductResponse response = CreateResponse(result);

        return Results.Created(CreateProductLocation(result.ProductId), response);
    }

    private static CreateProductCommand CreateCommand(Guid organisationId, Guid userId, CreateProductRequest request)
    {
        return new CreateProductCommand(
            OrganisationId: organisationId,
            Sku: request.Sku,
            Name: request.Name,
            Description: request.Description,
            UnitOfMeasureId: request.UnitOfMeasureId,
            ProductCategoryId: request.ProductCategoryId,
            TaxCategoryId: request.TaxCategoryId,
            CreatedBy: userId
            );
    }

    private static CreateProductResponse CreateResponse(CreateProductResult result)
    {
        return new CreateProductResponse(ProductId: result.ProductId, Sku: result.Sku);
    }

    private static string CreateProductLocation(
       Guid productId)
    {
        return $"/api/v1/catalog/products/{productId}";
    }
}
