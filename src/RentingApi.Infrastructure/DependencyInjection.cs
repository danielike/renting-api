using Microsoft.Extensions.DependencyInjection;
using RentingApi.Application.Ports.Output;
using RentingApi.Infrastructure.Persistence;

namespace RentingApi.Infrastructure;

/// <summary>
/// Dependency injection extensions for the infrastructure layer.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        // Register in-memory repositories (swap for real implementations in production)
        services.AddScoped<IVehicleRepository, InMemoryVehicleRepository>();
        services.AddScoped<IRentalRepository, InMemoryRentalRepository>();

        return services;
    }
}
