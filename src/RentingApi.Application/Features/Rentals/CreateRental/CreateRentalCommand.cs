using RentingApi.Application.Abstractions;

namespace RentingApi.Application.Features.Rentals.CreateRental;

/// <summary>
/// Command to rent a vehicle to a customer.
/// </summary>
public record CreateRentalCommand(
    Guid VehicleId,
    Guid CustomerId
) : ICommand<Guid>;
