namespace RentingApi.Api.Features.Vehicles.CreateVehicle;

/// <summary>
/// Request model for creating a new vehicle.
/// </summary>
public record CreateVehicleRequest(
    string LicensePlate,
    string Brand,
    string Model,
    int Year,
    DateTime ManufactureDate
);

/// <summary>
/// Response model for creating a new vehicle.
/// </summary>
public record CreateVehicleResponse(Guid Id);
