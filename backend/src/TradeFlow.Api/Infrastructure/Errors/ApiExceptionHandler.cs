using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TradeFlow.BuildingBlocks.Exceptions;

namespace TradeFlow.Api.Infrastructure.Errors;

public sealed class ApiExceptionHandler(
    ILogger<ApiExceptionHandler> logger,
    IHostEnvironment environment)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        (int status, string title, string code) = exception switch
        {
            NotFoundException notFoundException =>
                (StatusCodes.Status404NotFound, "Resource not found", notFoundException.Code),
            BusinessRuleException businessException =>
                (StatusCodes.Status409Conflict, "Business rule conflict", businessException.Code),
            _ =>
                (StatusCodes.Status500InternalServerError, "Unexpected server error", "UNEXPECTED_ERROR"),
        };

        logger.LogError(
            exception,
            "Request failed with error code {ErrorCode} and trace ID {TraceId}.",
            code,
            httpContext.TraceIdentifier);

        ProblemDetails problem = new()
        {
            Status = status,
            Title = title,
            Detail = status == StatusCodes.Status500InternalServerError && !environment.IsDevelopment()
                ? "An unexpected error occurred."
                : exception.Message,
            Instance = httpContext.Request.Path,
        };

        problem.Extensions["code"] = code;
        problem.Extensions["traceId"] = httpContext.TraceIdentifier;

        httpContext.Response.StatusCode = status;
        httpContext.Response.ContentType = "application/problem+json";

        await httpContext.Response.WriteAsJsonAsync(
            problem,
            options: null,
            contentType: "application/problem+json",
            cancellationToken: cancellationToken);

        return true;
    }
}
