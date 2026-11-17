using RentingApi.Application.Ports.Output;
using RentingApi.Domain.Entities;

namespace RentingApi.Infrastructure.Persistence;

/// <summary>
/// In-memory implementation of the rental repository.
/// </summary>
public class InMemoryRentalRepository : IRentalRepository
{
    private readonly Dictionary<Guid, Rental> _rentals = new();

    public Task AddAsync(Rental rental, CancellationToken cancellationToken)
    {
        _rentals[rental.Id] = rental;
        return Task.CompletedTask;
    }

    public Task<Rental?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        _rentals.TryGetValue(id, out var rental);
        return Task.FromResult(rental);
    }

    public Task<IEnumerable<Rental>> GetActiveRentalsByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken)
    {
        var rentals = _rentals.Values.Where(r =>
            r.CustomerId == customerId && r.Status == RentalStatus.Active);
        return Task.FromResult(rentals);
    }

    public Task<Rental?> GetActiveRentalByVehicleIdAsync(Guid vehicleId, CancellationToken cancellationToken)
    {
        var rental = _rentals.Values.FirstOrDefault(r =>
            r.VehicleId == vehicleId && r.Status == RentalStatus.Active);
        return Task.FromResult(rental);
    }
}
