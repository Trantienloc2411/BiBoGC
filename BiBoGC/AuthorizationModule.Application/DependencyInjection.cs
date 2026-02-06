using Microsoft.Extensions.DependencyInjection;

namespace AuthorizationModule.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddAuthorizationModuleApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);
        });
        
        
        return services;
    }
}