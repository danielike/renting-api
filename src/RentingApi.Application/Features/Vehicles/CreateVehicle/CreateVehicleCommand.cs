using System;
using RentingApi.Application.Abstractions;

namespace RentingApi.Application.Features.Vehicles.CreateVehicle;

/// <summary>
/// Command to add a new vehicle to the fleet.
/// </summary>
public record CreateVehicleCommand(
    Guid Id,
    string LicensePlate,
    string Brand,
    string Model,
    int Year,
    DateTime ManufactureDate
) : ICommand<Guid>;
