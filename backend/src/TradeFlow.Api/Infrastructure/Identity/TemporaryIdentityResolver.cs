namespace TradeFlow.Api.Infrastructure.Identity;

public static class TemporaryIdentityResolver
{
    private const string OrganisationIdHeader =
      "X-Organisation-Id";

    private const string UserIdHeader =
        "X-User-Id";

    public static IResult? TryResolve
        (HttpContext httpContext,
        IHostEnvironment environment,
        out TemporaryIdentity identity)
    {
        identity = new TemporaryIdentity(
             OrganisationId: Guid.Empty,
             UserId: Guid.Empty
            );
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

        identity = new TemporaryIdentity(
            OrganisationId: organisationId,
            UserId: userId
            );

        return null;
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
