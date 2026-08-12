using TradeFlow.Modules.Catalog.Application.Products.GetProductById;
namespace TradeFlow.Api.Endpoints.Catalog.Products;

public static class GetProductByIdEndpoint
{
    private const string OrganisationIdHeader =
       "X-Organisation-Id";

    private const string UserIdHeader =
        "X-User-Id";

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
        if (!TemporaryIdentityIsAllowed(environment))
        {
            return CreateAuthenticationNotConfiguredResult();
        }

        if (!TryReadGuidHeader(httpContext, OrganisationIdHeader, out Guid organisationId))
        {
            return CreateInvalidHeaderResult(OrganisationIdHeader);
        }

        if (!TryReadGuidHeader(httpContext, UserIdHeader, out _))
        {
            return CreateInvalidHeaderResult(UserIdHeader);
        }

        GetProductByIdQuery query = CreateQuery(productId, organisationId);

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

    private static bool TemporaryIdentityIsAllowed(IHostEnvironment environment)
    {
        return environment.IsDevelopment() ||
            environment.IsEnvironment("Testing");
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

    private static bool TryReadGuidHeader(HttpContext httpContext, string headerName, out Guid value)
    {
        string headerValue =
            httpContext.Request.Headers[headerName].ToString();

        return Guid.TryParse(headerValue, out value) && value != Guid.Empty;

    }
}
