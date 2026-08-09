using TradeFlow.Modules.Catalog.Application.Products.CreateProduct;
using Microsoft.AspNetCore.Builder;

namespace TradeFlow.Api.Endpoints.Catalog.Products;


public static class CreateProductEndpoint
{
    private const string OrganisationIdHeader =
        "X-Organisation-Id";

    private const string UserIdHeader =
        "X-User-Id";
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
        if (!TemporaryIdentityIsAllowed(environment))
        {
            return CreateAuthenticationNotConfiguredResult();
        }

        if (!TryReadGuidHeader(httpContext, OrganisationIdHeader, out Guid organisationId))
        {
            return CreateInvalidHeaderResult(OrganisationIdHeader);

        }

        if (!TryReadGuidHeader(httpContext, UserIdHeader, out Guid userId))
        {
            return CreateInvalidHeaderResult(UserIdHeader);
        }

        CreateProductCommand command = CreateCommand(organisationId, userId, request);

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

    private static bool TemporaryIdentityIsAllowed(IHostEnvironment environment)
    {
        return environment.IsDevelopment() ||
            environment.IsEnvironment("Testing");
    }

    private static bool TryReadGuidHeader(HttpContext httpContext, string headerName, out Guid value)
    {
        string headerValue =
            httpContext.Request.Headers[headerName].ToString();

        return Guid.TryParse(headerValue, out value) && value != Guid.Empty;
    }
    private static IResult CreateAuthenticationNotConfiguredResult()
    {
        return Results.Problem(statusCode: StatusCodes.Status501NotImplemented, title: "Authentication is not configured.",
            detail: "Temporary identity headers are disabled " + "outside Development and Testing.");
    }

    private static IResult CreateInvalidHeaderResult(string headerName)
    {
        Dictionary<string, string[]> errors = new()
        {
            [headerName] =
            [
                $"Header '{headerName}' must contain " + "a non-empty GUID."

                ]
        };
        return Results.ValidationProblem(errors);
    }
}
