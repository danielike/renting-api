using RentingApi.Domain.Entities;
using RentingApi.Domain.Exceptions;

namespace RentingApi.Domain.Tests;

public class VehicleTests
{
    [Fact]
    public void Constructor_WhenValid_CreatesVehicle()
    {
        // Arrange & Act
        var vehicle = new Vehicle(
            Guid.NewGuid(),
            "ABC-123",
            "Toyota",
            "Corolla",
            2024,
            new DateTime(2024, 1, 15));

        // Assert
        Assert.Equal("ABC-123", vehicle.LicensePlate);
        Assert.Equal("Toyota", vehicle.Brand);
        Assert.Equal("Corolla", vehicle.Model);
        Assert.Equal(2024, vehicle.Year);
        Assert.Equal(VehicleStatus.Available, vehicle.Status);
        Assert.True(vehicle.IsWithinAgeLimit());
    }

    [Theory]
    [InlineData("", "Toyota", "Corolla", 2024)]
    [InlineData("ABC-123", "", "Corolla", 2024)]
    [InlineData("ABC-123", "Toyota", "", 2024)]
    public void Constructor_WhenEmptyField_ThrowsDomainException(string plate, string brand, string model, int year)
    {
        // Act & Assert
        var act = () => new Vehicle(Guid.NewGuid(), plate, brand, model, year, new DateTime(2024, 1, 15));
        Assert.Throws<DomainException>(act);
    }
}
