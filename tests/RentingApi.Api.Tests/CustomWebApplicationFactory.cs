using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using RentingApi.Application.Ports.Output;
using RentingApi.Infrastructure.Persistence;

namespace RentingApi.Api.Tests;

/// <summary>
/// Custom factory that registers in-memory repositories as singletons
/// so they persist across requests.
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove scoped registrations
            var vehicleRepoDesc = services.FirstOrDefault(d => d.ServiceType == typeof(IVehicleRepository));
            if (vehicleRepoDesc is not null)
                services.Remove(vehicleRepoDesc);

            var rentalRepoDesc = services.FirstOrDefault(d => d.ServiceType == typeof(IRentalRepository));
            if (rentalRepoDesc is not null)
                services.Remove(rentalRepoDesc);

            // Register as singletons to persist across requests
            services.AddSingleton<IVehicleRepository, InMemoryVehicleRepository>();
            services.AddSingleton<IRentalRepository, InMemoryRentalRepository>();
        });
    }
}
