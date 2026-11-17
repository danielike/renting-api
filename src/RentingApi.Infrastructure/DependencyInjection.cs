using Microsoft.Extensions.DependencyInjection;
using RentingApi.Application.Abstractions;
using RentingApi.Application.Features.Rentals.CreateRental;
using RentingApi.Application.Features.Rentals.ReturnVehicle;
using RentingApi.Application.Features.Vehicles.CreateVehicle;
using RentingApi.Application.Features.Vehicles.ListVehicles;
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
        services.AddSingleton<IVehicleRepository, InMemoryVehicleRepository>();
        services.AddSingleton<IRentalRepository, InMemoryRentalRepository>();

        // Register command/query handlers
        services.AddScoped<ICommandHandler<CreateRentalCommand, Guid>, CreateRentalCommandHandler>();
        services.AddScoped<ICommandHandler<CreateVehicleCommand, Guid>, CreateVehicleCommandHandler>();
        services.AddScoped<ICommandHandler<ReturnVehicleCommand>, ReturnVehicleCommandHandler>();
        services.AddScoped<IQueryHandler<ListVehiclesQuery, IEnumerable<Application.Dtos.VehicleDto>>, ListVehiclesQueryHandler>();
        services.AddScoped<IQueryHandler<ReturnVehicleQuery, Guid>, ReturnVehicleQueryHandler>();

        return services;
    }
}
