using Microsoft.Extensions.DependencyInjection;
using RentingApi.Application.Abstractions;
using RentingApi.Application.Dtos;
using RentingApi.Application.Features.Rentals.CreateRental;
using RentingApi.Application.Features.Vehicles.CreateVehicle;
using RentingApi.Application.Features.Vehicles.ListVehicles;
using RentingApi.Application.Ports.Output;
using RentingApi.Domain.Entities;
using RentingApi.Infrastructure.Persistence;

namespace RentingApi.Application.Tests;

/// <summary>
/// Functional (integration) tests: validate complete business flows
/// using in-memory repositories (excluding the HTTP host).
/// </summary>
public class CreateRentalFlowTests
{
    private static ServiceProvider CreateServiceProvider(IVehicleRepository vehicleRepo, IRentalRepository rentalRepo)
    {
        var services = new ServiceCollection();
        services.AddSingleton(vehicleRepo);
        services.AddSingleton(rentalRepo);
        services.AddScoped(typeof(ICommandHandler<CreateVehicleCommand, Guid>), CreateVehicleCommandHandlerFactory);
        services.AddScoped(typeof(ICommandHandler<CreateRentalCommand, Guid>), CreateRentalCommandHandlerFactory);
        services.AddScoped(typeof(IQueryHandler<ListVehiclesQuery, IEnumerable<VehicleDto>>), ListVehiclesQueryHandlerFactory);
        services.AddScoped<Mediator>();
        return services.BuildServiceProvider();
    }

    private static CreateVehicleCommandHandler CreateVehicleCommandHandlerFactory(IServiceProvider sp)
    {
        return new CreateVehicleCommandHandler(sp.GetRequiredService<IVehicleRepository>());
    }

    private static CreateRentalCommandHandler CreateRentalCommandHandlerFactory(IServiceProvider sp)
    {
        return new CreateRentalCommandHandler(
            sp.GetRequiredService<IVehicleRepository>(),
            sp.GetRequiredService<IRentalRepository>());
    }

    private static ListVehiclesQueryHandler ListVehiclesQueryHandlerFactory(IServiceProvider sp)
    {
        return new ListVehiclesQueryHandler(sp.GetRequiredService<IVehicleRepository>());
    }

    [Fact]
    public async Task FullFlow_CreateVehicleThenRentIt_ReturnsRentalId()
    {
        // Arrange
        var vehicleRepo = new InMemoryVehicleRepository();
        var rentalRepo = new InMemoryRentalRepository();
        var mediator = new Mediator(CreateServiceProvider(vehicleRepo, rentalRepo));

        var vehicleId = Guid.NewGuid();
        var customerId = Guid.NewGuid();

        // Act: Create vehicle
        var createVehicleCommand = new CreateVehicleCommand(
            vehicleId,
            "ABC-123",
            "Toyota",
            "Corolla",
            2024,
            new DateTime(2024, 1, 15)
        );
        var createdVehicleId = await mediator.Send<CreateVehicleCommand, Guid>(createVehicleCommand, CancellationToken.None);

        // Act: Rent the vehicle
        var createRentalCommand = new CreateRentalCommand(createdVehicleId, customerId);
        var rentalId = await mediator.Send<CreateRentalCommand, Guid>(createRentalCommand, CancellationToken.None);

        // Assert
        Assert.NotEqual(Guid.Empty, rentalId);
        Assert.NotEqual(Guid.Empty, createdVehicleId);

        var vehicle = await vehicleRepo.GetByIdAsync(createdVehicleId, CancellationToken.None);
        Assert.NotNull(vehicle);
        Assert.Equal(VehicleStatus.Rented, vehicle!.Status);
        Assert.Equal(rentalId, vehicle.CurrentRentalId);
    }

    [Fact]
    public async Task FullFlow_ReuseVehicleForAnotherCustomer_ReturnsSecondRentalId()
    {
        // Arrange
        var vehicleRepo = new InMemoryVehicleRepository();
        var rentalRepo = new InMemoryRentalRepository();
        var mediator = new Mediator(CreateServiceProvider(vehicleRepo, rentalRepo));

        var vehicleId = Guid.NewGuid();
        var customerId1 = Guid.NewGuid();
        var customerId2 = Guid.NewGuid();

        // Create vehicle
        var createVehicleCommand = new CreateVehicleCommand(
            vehicleId, "XYZ-456", "Honda", "Civic", 2024, new DateTime(2024, 6, 1));
        var createdId = await mediator.Send<CreateVehicleCommand, Guid>(createVehicleCommand, CancellationToken.None);

        // Rent to customer 1
        var command1 = new CreateRentalCommand(createdId, customerId1);
        var rentalId1 = await mediator.Send<CreateRentalCommand, Guid>(command1, CancellationToken.None);

        // Return vehicle
        var rental1 = await rentalRepo.GetByIdAsync(rentalId1, CancellationToken.None);
        rental1!.Complete(DateTime.UtcNow);
        var vehicle = await vehicleRepo.GetByIdAsync(createdId, CancellationToken.None);
        vehicle!.ReturnVehicle();

        // Act: Rent to customer 2
        var command2 = new CreateRentalCommand(createdId, customerId2);
        var rentalId2 = await mediator.Send<CreateRentalCommand, Guid>(command2, CancellationToken.None);

        // Assert
        Assert.NotEqual(rentalId1, rentalId2);
        var vehicle2 = await vehicleRepo.GetByIdAsync(createdId, CancellationToken.None);
        Assert.NotNull(vehicle2);
        Assert.Equal(VehicleStatus.Rented, vehicle2!.Status);
        Assert.Equal(rentalId2, vehicle2.CurrentRentalId);
    }

    [Fact]
    public async Task FullFlow_ThreeRentalsForThreeVehicles()
    {
        // Arrange
        var vehicleRepo = new InMemoryVehicleRepository();
        var rentalRepo = new InMemoryRentalRepository();
        var mediator = new Mediator(CreateServiceProvider(vehicleRepo, rentalRepo));

        var customerIds = new[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
        var vehicleIds = new[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };

        // Create three vehicles
        for (int i = 0; i < 3; i++)
        {
            var cmd = new CreateVehicleCommand(
                vehicleIds[i],
                $"PLATE-{i}",
                $"Brand{i}",
                $"Model{i}",
                2024,
                new DateTime(2024, 1, 1)
            );
            await mediator.Send<CreateVehicleCommand, Guid>(cmd, CancellationToken.None);
        }

        // Act: Rent one vehicle to each customer
        var rentalIds = new List<Guid>();
        for (int i = 0; i < 3; i++)
        {
            var cmd = new CreateRentalCommand(vehicleIds[i], customerIds[i]);
            var rentalId = await mediator.Send<CreateRentalCommand, Guid>(cmd, CancellationToken.None);
            rentalIds.Add(rentalId);
        }

        // Assert
        Assert.Equal(3, rentalIds.Count);
        Assert.Equal(rentalIds.Distinct().Count(), rentalIds.Count);
        foreach (var vid in vehicleIds)
        {
            var v = await vehicleRepo.GetByIdAsync(vid, CancellationToken.None);
            Assert.NotNull(v);
            Assert.Equal(VehicleStatus.Rented, v!.Status);
        }
    }
}
