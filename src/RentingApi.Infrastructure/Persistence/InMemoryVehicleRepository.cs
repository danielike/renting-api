using RentingApi.Application.Ports.Output;
using RentingApi.Domain.Entities;

namespace RentingApi.Infrastructure.Persistence;

/// <summary>
/// In-memory implementation of the vehicle repository.
/// </summary>
public class InMemoryVehicleRepository : IVehicleRepository
{
    private readonly Dictionary<Guid, Vehicle> _vehicles = new();

    public Task AddAsync(Vehicle vehicle, CancellationToken cancellationToken)
    {
        _vehicles[vehicle.Id] = vehicle;
        return Task.CompletedTask;
    }

    public Task<Vehicle?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        _vehicles.TryGetValue(id, out var vehicle);
        return Task.FromResult(vehicle);
    }

    public Task<IEnumerable<Vehicle>> ListAvailableAsync(CancellationToken cancellationToken)
    {
        var vehicles = _vehicles.Values.Where(v => v.Status == VehicleStatus.Available);
        return Task.FromResult(vehicles);
    }

    public Task<Vehicle?> GetByLicensePlateAsync(string licensePlate, CancellationToken cancellationToken)
    {
        var vehicle = _vehicles.Values.FirstOrDefault(v =>
            string.Equals(v.LicensePlate, licensePlate, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(vehicle);
    }
}
