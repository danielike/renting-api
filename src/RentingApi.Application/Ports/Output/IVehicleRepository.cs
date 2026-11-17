using RentingApi.Domain.Entities;

namespace RentingApi.Application.Ports.Output;

/// <summary>
/// Output port for vehicle persistence operations.
/// </summary>
public interface IVehicleRepository
{
    Task AddAsync(Vehicle vehicle, CancellationToken cancellationToken);
    Task<Vehicle?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IEnumerable<Vehicle>> ListAvailableAsync(CancellationToken cancellationToken);
    Task<Vehicle?> GetByLicensePlateAsync(string licensePlate, CancellationToken cancellationToken);
}
