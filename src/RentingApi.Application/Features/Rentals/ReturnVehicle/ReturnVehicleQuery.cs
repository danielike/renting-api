using RentingApi.Application.Abstractions;

namespace RentingApi.Application.Features.Rentals.ReturnVehicle;

/// <summary>
/// Query to get the vehicle ID for a rental.
/// </summary>
public record ReturnVehicleQuery(Guid RentalId) : IQuery<Guid>;
