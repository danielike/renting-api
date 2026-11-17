using RentingApi.Domain.Exceptions;

namespace RentingApi.Domain.Entities;

/// <summary>
/// Represents a vehicle rental transaction.
/// </summary>
public class Rental
{
    public Guid Id { get; }
    public Guid VehicleId { get; }
    public Guid CustomerId { get; }
    public DateTime RentalStartDate { get; }
    public DateTime? RentalEndDate { get; set; }
    public RentalStatus Status { get; private set; }

    public Rental(Guid id, Guid vehicleId, Guid customerId, DateTime rentalStartDate)
    {
        if (vehicleId == Guid.Empty)
            throw new DomainException("Vehicle ID cannot be empty.");

        if (customerId == Guid.Empty)
            throw new DomainException("Customer ID cannot be empty.");

        if (rentalStartDate > DateTime.UtcNow)
            throw new DomainException("Rental start date cannot be in the future.");

        Id = id;
        VehicleId = vehicleId;
        CustomerId = customerId;
        RentalStartDate = rentalStartDate;
        RentalEndDate = null;
        Status = RentalStatus.Active;
    }

    /// <summary>
    /// Marks the rental as completed (vehicle returned).
    /// </summary>
    public void Complete(DateTime endDate)
    {
        if (Status != RentalStatus.Active)
            throw new DomainException($"Rental is not active. Current status: {Status}");

        if (endDate < RentalStartDate)
            throw new DomainException("End date cannot be before start date.");

        Status = RentalStatus.Completed;
        RentalEndDate = endDate;
    }
}

/// <summary>
/// Represents the possible states of a rental.
/// </summary>
public enum RentalStatus
{
    Active = 0,
    Completed = 1
}
