using System.Text.Json;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace TradeFlow.Api.Infrastructure.Health;

public static class HealthResponseWriter
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static Task WriteAsync(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/json";

        object response = new
        {
            status = report.Status.ToString(),
            service = "TradeFlow.Api",
            durationMilliseconds = report.TotalDuration.TotalMilliseconds,
            checks = report.Entries.Select(entry => new
            {
                name = entry.Key,
                status = entry.Value.Status.ToString(),
                durationMilliseconds = entry.Value.Duration.TotalMilliseconds,
            }),
            traceId = context.TraceIdentifier,
        };

        return context.Response.WriteAsync(
            JsonSerializer.Serialize(response, JsonOptions));
    }
}
