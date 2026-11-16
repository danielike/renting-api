using RentingApi.Application.Abstractions;
using RentingApi.Application.Dtos;

namespace RentingApi.Application.Features.Vehicles.ListVehicles;

/// <summary>
/// Query to list all available vehicles in the fleet.
/// </summary>
public record ListVehiclesQuery() : IQuery<IEnumerable<VehicleDto>>;
