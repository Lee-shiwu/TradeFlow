using Microsoft.Extensions.DependencyInjection;

namespace TradeFlow.Modules.Identity;

public static class IdentityModule
{
    public static IServiceCollection AddIdentityModule(this IServiceCollection services)
    {
        return services;
    }
}
