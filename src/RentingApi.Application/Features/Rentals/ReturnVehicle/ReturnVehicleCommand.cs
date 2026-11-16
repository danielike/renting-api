using System;
using RentingApi.Application.Abstractions;

namespace RentingApi.Application.Features.Rentals.ReturnVehicle;

/// <summary>
/// Command to return a rented vehicle.
/// </summary>
public record ReturnVehicleCommand(
    Guid RentalId,
    Guid VehicleId
) : ICommand;
