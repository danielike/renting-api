using RentingApi.Application.Abstractions;
using RentingApi.Application.Ports.Output;

namespace RentingApi.Application.Features.Rentals.ReturnVehicle;

/// <summary>
/// Handler for returning a rented vehicle.
/// </summary>
public sealed class ReturnVehicleQueryHandler : IQueryHandler<ReturnVehicleQuery, Guid>
{
    private readonly IRentalRepository _rentalRepository;

    public ReturnVehicleQueryHandler(IRentalRepository rentalRepository)
    {
        _rentalRepository = rentalRepository;
    }

    public async Task<Guid> Handle(ReturnVehicleQuery query, CancellationToken cancellationToken)
    {
        var rental = await _rentalRepository.GetByIdAsync(query.RentalId, cancellationToken);
        if (rental is null)
        {
            throw new Domain.Exceptions.DomainException($"Rental with ID '{query.RentalId}' not found.");
        }

        return rental.VehicleId;
    }
}
