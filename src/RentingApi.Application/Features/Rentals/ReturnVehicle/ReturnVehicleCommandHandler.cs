using System.Threading;
using System.Threading.Tasks;
using RentingApi.Application.Abstractions;
using RentingApi.Application.Ports.Output;
using RentingApi.Domain.Entities;
using RentingApi.Domain.Exceptions;

namespace RentingApi.Application.Features.Rentals.ReturnVehicle;

/// <summary>
/// Handler for returning a rented vehicle.
/// </summary>
internal sealed class ReturnVehicleCommandHandler : ICommandHandler<ReturnVehicleCommand>
{
    private readonly IVehicleRepository _vehicleRepository;
    private readonly IRentalRepository _rentalRepository;

    public ReturnVehicleCommandHandler(IVehicleRepository vehicleRepository, IRentalRepository rentalRepository)
    {
        _vehicleRepository = vehicleRepository;
        _rentalRepository = rentalRepository;
    }

    public async Task Handle(ReturnVehicleCommand command, CancellationToken cancellationToken)
    {
        // Check if the rental exists and is active
        var rental = await _rentalRepository.GetByIdAsync(command.RentalId, cancellationToken);
        if (rental == null)
        {
            throw new DomainException($"Rental with ID '{command.RentalId}' not found.");
        }

        if (rental.Status != RentalStatus.Active)
        {
            throw new DomainException(
                $"Rental '{command.RentalId}' is not active (status: {rental.Status}).");
        }

        if (rental.VehicleId != command.VehicleId)
        {
            throw new DomainException(
                $"Rental '{command.RentalId}' is for a different vehicle.");
        }

        // Check if the vehicle exists and is rented
        var vehicle = await _vehicleRepository.GetByIdAsync(command.VehicleId, cancellationToken);
        if (vehicle == null)
        {
            throw new DomainException($"Vehicle with ID '{command.VehicleId}' not found.");
        }

        if (vehicle.Status != VehicleStatus.Rented)
        {
            throw new DomainException(
                $"Vehicle '{vehicle.LicensePlate}' is not currently rented (status: {vehicle.Status}).");
        }

        // Complete the rental
        var endDate = DateTime.UtcNow;
        rental.Complete(endDate);

        // Update the vehicle status
        vehicle.ReturnVehicle();

        await _rentalRepository.AddAsync(rental, cancellationToken);
        // Note: In a real application, we'd update the vehicle repository
    }
}
