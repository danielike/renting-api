namespace RentingApi.Api.Features.Rentals.CreateRental;

/// <summary>
/// Request model for renting a vehicle.
/// </summary>
public record CreateRentalRequest(
    Guid CustomerId
);

/// <summary>
/// Response model for creating a rental.
/// </summary>
public record CreateRentalResponse(Guid RentalId);
