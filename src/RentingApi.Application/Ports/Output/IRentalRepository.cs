using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RentingApi.Domain.Entities;

namespace RentingApi.Application.Ports.Output;

/// <summary>
/// Output port for rental persistence operations.
/// </summary>
public interface IRentalRepository
{
    Task AddAsync(Rental rental, CancellationToken cancellationToken);
    Task<Rental?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IEnumerable<Rental>> GetActiveRentalsByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken);
    Task<Rental?> GetActiveRentalByVehicleIdAsync(Guid vehicleId, CancellationToken cancellationToken);
}
