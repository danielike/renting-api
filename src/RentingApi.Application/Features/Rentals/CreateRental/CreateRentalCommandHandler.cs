using RentingApi.Application.Abstractions;
using RentingApi.Application.Ports.Output;
using RentingApi.Domain.Entities;
using RentingApi.Domain.Exceptions;

namespace RentingApi.Application.Features.Rentals.CreateRental;

/// <summary>
/// Handler for creating a new rental.
/// </summary>
public sealed class CreateRentalCommandHandler : ICommandHandler<CreateRentalCommand, Guid>
{
    private readonly IVehicleRepository _vehicleRepository;
    private readonly IRentalRepository _rentalRepository;

    public CreateRentalCommandHandler(IVehicleRepository vehicleRepository, IRentalRepository rentalRepository)
    {
        _vehicleRepository = vehicleRepository;
        _rentalRepository = rentalRepository;
    }

    public async Task<Guid> Handle(CreateRentalCommand command, CancellationToken cancellationToken)
    {
        // Check if the vehicle exists and is available
        var vehicle = await _vehicleRepository.GetByIdAsync(command.VehicleId, cancellationToken);
        if (vehicle == null)
        {
            throw new DomainException($"Vehicle with ID '{command.VehicleId}' not found.");
        }

        if (vehicle.Status != VehicleStatus.Available)
        {
            throw new DomainException($"Vehicle '{vehicle.LicensePlate}' is not available (status: {vehicle.Status}).");
        }

        // Check if the customer already has an active rental
        var customerActiveRentals = await _rentalRepository.GetActiveRentalsByCustomerIdAsync(
            command.CustomerId, cancellationToken);

        if (customerActiveRentals.Any())
        {
            throw new DomainException(
                $"Customer '{command.CustomerId}' already has an active rental. A customer can only rent one vehicle at a time.");
        }

        // Check if the vehicle is already rented
        var existingRental = await _rentalRepository.GetActiveRentalByVehicleIdAsync(
            command.VehicleId, cancellationToken);
        if (existingRental != null)
        {
            throw new DomainException(
                $"Vehicle '{vehicle.LicensePlate}' already has an active rental.");
        }

        // Create the rental
        var rental = new Rental(
            Guid.NewGuid(),
            command.VehicleId,
            command.CustomerId,
            DateTime.UtcNow
        );

        await _rentalRepository.AddAsync(rental, cancellationToken);

        // Update the vehicle status
        vehicle.Rent(rental.Id);
        // Note: In a real application, we'd call a method on the vehicle repository to save the update
        // For now, we'll mark it in the vehicle entity

        return rental.Id;
    }
}
