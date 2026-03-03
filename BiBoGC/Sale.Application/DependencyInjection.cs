using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Sale.Application.Commands.CreateSalesOrder;

namespace Sale.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddSaleApplication(this IServiceCollection services)
    {
        var assembly = typeof(CreateSalesOrderCommand).Assembly;

        services.AddMediatR(cfg => { cfg.RegisterServicesFromAssembly(assembly); });

        services.AddValidatorsFromAssembly(assembly);

        return services;
    }
}