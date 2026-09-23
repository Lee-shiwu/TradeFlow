using TradeFlow.Api.Infrastructure.Identity;
using TradeFlow.Modules.Catalog.Application.ProductCategories.CreateProductCategory;

namespace TradeFlow.Api.Endpoints.Catalog.ProductCategories;

public static class CreateProductCategoryEndpoint
{
    public static IEndpointRouteBuilder MapCreateProductCategoryEndpoint(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost(
                "/api/v1/catalog/product-categories",
                HandleAsync)
            .WithName("CreateProductCategory")
            .WithTags("Catalog Product Categories")
            .Accepts<CreateProductCategoryRequest>("application/json")
            .Produces<CreateProductCategoryResponse>(
                StatusCodes.Status201Created)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status501NotImplemented);

        return endpoints;
    }

    private static async Task<IResult> HandleAsync(
        CreateProductCategoryRequest request,
        HttpContext httpContext,
        CreateProductCategoryHandler handler,
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

        CreateProductCategoryCommand command =
            new(
                OrganisationId: identity.OrganisationId,
                Code: request.Code,
                Name: request.Name,
                Description: request.Description,
                CreatedBy: identity.UserId);

        CreateProductCategoryResult result =
            await handler.HandleAsync(command, cancellationToken);

        CreateProductCategoryResponse response = CreateResponse(result);

        return Results.Created(
            $"/api/v1/catalog/product-categories/{result.ProductCategoryId}",
            response);
    }

    private static CreateProductCategoryResponse CreateResponse(
        CreateProductCategoryResult result)
    {
        return new CreateProductCategoryResponse(
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
