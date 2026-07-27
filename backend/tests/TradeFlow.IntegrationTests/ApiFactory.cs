using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace TradeFlow.IntegrationTests;

public sealed class ApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            Dictionary<string, string?> settings = new()
            {
                ["ConnectionStrings:TradeFlowDatabase"] =
                    "Server=localhost,14330;Database=TradeFlowTests;"
                    + "User Id=sa;Password=IntegrationTestsOnly1!;"
                    + "TrustServerCertificate=True",
            };

            configuration.AddInMemoryCollection(settings);
        });
    }
}
