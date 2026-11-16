using RentingApi.Domain.Exceptions;

namespace RentingApi.Domain.Entities;

/// <summary>
/// Represents a vehicle in the fleet.
/// </summary>
public class Vehicle
{
    public Guid Id { get; }
    public string LicensePlate { get; }
    public string Brand { get; }
    public string Model { get; }
    public int Year { get; }
    public DateTime ManufactureDate { get; }
    public VehicleStatus Status { get; private set; }
    public Guid? CurrentRentalId { get; private set; }

    private Vehicle() { }

    public Vehicle(Guid id, string licensePlate, string brand, string model, int year, DateTime manufactureDate)
    {
        if (string.IsNullOrWhiteSpace(licensePlate))
            throw new DomainException("License plate cannot be empty.");

        if (string.IsNullOrWhiteSpace(brand))
            throw new DomainException("Brand cannot be empty.");

        if (string.IsNullOrWhiteSpace(model))
            throw new DomainException("Model cannot be empty.");

        if (year < 1900 || year > DateTime.UtcNow.Year + 1)
            throw new DomainException("Invalid vehicle year.");

        if (manufactureDate > DateTime.UtcNow)
            throw new DomainException("Manufacture date cannot be in the future.");

        Id = id;
        LicensePlate = licensePlate.Trim();
        Brand = brand.Trim();
        Model = model.Trim();
        Year = year;
        ManufactureDate = manufactureDate;
        Status = VehicleStatus.Available;
        CurrentRentalId = null;
    }

    /// <summary>
    /// Marks the vehicle as rented.
    /// </summary>
    public void Rent(Guid rentalId)
    {
        if (Status != VehicleStatus.Available)
            throw new DomainException($"Vehicle is not available. Current status: {Status}");

        Status = VehicleStatus.Rented;
        CurrentRentalId = rentalId;
    }

    /// <summary>
    /// Marks the vehicle as available again after return.
    /// </summary>
    public void ReturnVehicle()
    {
        if (Status != VehicleStatus.Rented)
            throw new DomainException($"Vehicle is not currently rented. Current status: {Status}");

        Status = VehicleStatus.Available;
        CurrentRentalId = null;
    }

    /// <summary>
    /// Checks if the vehicle is still within the 5-year age limit from manufacture date.
    /// </summary>
    public bool IsWithinAgeLimit(DateTime? asOfDate = null)
    {
        var referenceDate = asOfDate ?? DateTime.UtcNow;
        return (referenceDate - ManufactureDate).TotalDays <= 5 * 365.25;
    }
}

/// <summary>
/// Represents the possible states of a vehicle in the fleet.
/// </summary>
public enum VehicleStatus
{
    Available = 0,
    Rented = 1
}
