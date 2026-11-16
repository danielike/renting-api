using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using RentingApi.Application.Abstractions;
using RentingApi.Application.Dtos;
using RentingApi.Application.Ports.Output;
using RentingApi.Domain.Entities;

namespace RentingApi.Application.Features.Vehicles.ListVehicles;

/// <summary>
/// Handler for listing available vehicles.
/// </summary>
internal sealed class ListVehiclesQueryHandler : IQueryHandler<ListVehiclesQuery, IEnumerable<VehicleDto>>
{
    private readonly IVehicleRepository _vehicleRepository;

    public ListVehiclesQueryHandler(IVehicleRepository vehicleRepository)
    {
        _vehicleRepository = vehicleRepository;
    }

    public async Task<IEnumerable<VehicleDto>> Handle(ListVehiclesQuery query, CancellationToken cancellationToken)
    {
        var vehicles = await _vehicleRepository.ListAvailableAsync(cancellationToken);

        // Filter out vehicles older than 5 years
        var currentVehicles = vehicles.Where(v => v.IsWithinAgeLimit());

        return currentVehicles.Select(MapToDto);
    }

    private static VehicleDto MapToDto(Vehicle vehicle)
    {
        return new VehicleDto(
            vehicle.Id,
            vehicle.LicensePlate,
            vehicle.Brand,
            vehicle.Model,
            vehicle.Year,
            vehicle.ManufactureDate,
            vehicle.Status.ToString()
        );
    }
}
