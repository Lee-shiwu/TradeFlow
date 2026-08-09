using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace TradeFlow.IntegrationTests;

public sealed class ApiFactory : WebApplicationFactory<Program>
{
    private const string TestConnectionStringEnvironmentVariable =
       "TRADEFLOW_TEST_DB_CONNECTION_STRING";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        string? connectionString = Environment.GetEnvironmentVariable(TestConnectionStringEnvironmentVariable);
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException($"Environment variable "
                + $"'{TestConnectionStringEnvironmentVariable}' "
                + "is not configured.");

        }
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            Dictionary<string, string?> settings = new()
            {
                ["ConnectionStrings:TradeFlowDatabase"] =
                   connectionString
            };

            configuration.AddInMemoryCollection(settings);
        });
    }

}
