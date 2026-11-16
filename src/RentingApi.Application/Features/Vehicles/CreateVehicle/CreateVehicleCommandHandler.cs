using System;
using System.Threading;
using System.Threading.Tasks;
using RentingApi.Application.Abstractions;
using RentingApi.Application.Ports.Output;
using RentingApi.Domain.Entities;
using RentingApi.Domain.Exceptions;

namespace RentingApi.Application.Features.Vehicles.CreateVehicle;

/// <summary>
/// Handler for creating a new vehicle in the fleet.
/// </summary>
internal sealed class CreateVehicleCommandHandler : ICommandHandler<CreateVehicleCommand, Guid>
{
    private readonly IVehicleRepository _vehicleRepository;

    public CreateVehicleCommandHandler(IVehicleRepository vehicleRepository)
    {
        _vehicleRepository = vehicleRepository;
    }

    public async Task<Guid> Handle(CreateVehicleCommand command, CancellationToken cancellationToken)
    {
        // Check if a vehicle with the same license plate already exists
        var existing = await _vehicleRepository.GetByLicensePlateAsync(command.LicensePlate, cancellationToken);
        if (existing != null)
        {
            throw new DomainException($"A vehicle with license plate '{command.LicensePlate}' already exists.");
        }

        // Check if the vehicle is within the 5-year age limit
        var vehicle = new Vehicle(
            command.Id,
            command.LicensePlate,
            command.Brand,
            command.Model,
            command.Year,
            command.ManufactureDate
        );

        if (!vehicle.IsWithinAgeLimit())
        {
            throw new DomainException(
                $"Vehicle with license plate '{command.LicensePlate}' is older than 5 years and cannot be added to the fleet.");
        }

        await _vehicleRepository.AddAsync(vehicle, cancellationToken);
        return vehicle.Id;
    }
}
