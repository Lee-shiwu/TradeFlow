using TradeFlow.Modules.Catalog.Application.Products.ListProducts;

namespace TradeFlow.Api.Endpoints.Catalog.Products;

public static class ListProductsEndpoint
{
    private const string OrganisationIdHeader =
       "X-Organisation-Id";

    private const string UserIdHeader =
        "X-User-Id";

    public static IEndpointRouteBuilder MapListProductsEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/v1/catalog/products", HandleAsync)
            .WithName("ListProducts")
            .WithTags("Catalog Products")
            .Produces<ListProductsResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status501NotImplemented);

        return endpoints;
    }

    private static async Task<IResult> HandleAsync(
        [AsParameters] ListProductsRequest request,
        HttpContext httpContext,
        ListProductsHandler handler,
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

        ListProductsQuery query = CreateQuery(request, organisationId);

        ListProductsResult result =
            await handler.HandleAsync(query, cancellationToken);

        ListProductsResponse response = CreateResponse(result);

        return Results.Ok(response);
    }

    private static ListProductsResponse CreateResponse(ListProductsResult result)
    {
        List<ListProductItemResponse> items =
            result.Items
                .Select(item =>
                    new ListProductItemResponse(
                        ProductId: item.ProductId,
                        Sku: item.Sku,
                        Name: item.Name,
                        UnitOfMeasureId: item.UnitOfMeasureId,
                        ProductCategoryId: item.ProductCategoryId,
                        TaxCategoryId: item.TaxCategoryId,
                        Status: item.Status,
                        CreatedAt: item.CreatedAt,
                        LastModifiedAt: item.LastModifiedAt))
                .ToList();

        return new ListProductsResponse(
            Items: items,
            PageNumber: result.PageNumber,
            PageSize: result.PageSize,
            TotalCount: result.TotalCount,
            TotalPages: result.TotalPages);
    }

    private static ListProductsQuery CreateQuery(ListProductsRequest request, Guid organisationId)
    {
        return new ListProductsQuery(
            OrganisationId: organisationId,
            Search: request.Search,
            Status: request.Status,
            PageNumber: request.PageNumber ?? 1,
            PageSize: request.PageSize ?? 20);
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

    private static bool TryReadGuidHeader(HttpContext httpContext, string headerName, out Guid value)
    {
        string headerValue =
            httpContext.Request.Headers[headerName].ToString();

        return Guid.TryParse(headerValue, out value) && value != Guid.Empty;
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
