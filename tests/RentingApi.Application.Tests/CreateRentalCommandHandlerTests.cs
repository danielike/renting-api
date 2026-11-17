using Moq;
using RentingApi.Application.Features.Rentals.CreateRental;
using RentingApi.Application.Ports.Output;
using RentingApi.Domain.Entities;
using RentingApi.Domain.Exceptions;

namespace RentingApi.Application.Tests;

/// <summary>
/// Unit tests: validate the CreateRentalCommandHandler logic without its dependencies.
/// Mock IVehicleRepository and IRentalRepository.
/// </summary>
public class CreateRentalCommandHandlerTests
{
    [Fact]
    public async Task Handle_WithNullVehicle_ThrowsDomainException()
    {
        // Arrange
        var mockVehicleRepository = new Mock<IVehicleRepository>();
        mockVehicleRepository
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Vehicle?)null);

        var mockRentalRepository = new Mock<IRentalRepository>();

        var handler = new CreateRentalCommandHandler(
            mockVehicleRepository.Object,
            mockRentalRepository.Object);

        var command = new CreateRentalCommand(Guid.NewGuid(), Guid.NewGuid());

        // Act & Assert
        var act = async () => await handler.Handle(command, CancellationToken.None);
        await Assert.ThrowsAsync<DomainException>(act);
    }

    [Fact]
    public async Task Handle_WithValidData_CreatesRentalAndReturnsId()
    {
        // Arrange
        var vehicle = new Vehicle(
            Guid.NewGuid(),
            "ABC-123",
            "Toyota",
            "Corolla",
            2024,
            new DateTime(2024, 1, 15));

        var mockVehicleRepository = new Mock<IVehicleRepository>();
        mockVehicleRepository
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(vehicle);

        var mockRentalRepository = new Mock<IRentalRepository>();
        mockRentalRepository
            .Setup(r => r.GetActiveRentalsByCustomerIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<Rental>());
        mockRentalRepository
            .Setup(r => r.GetActiveRentalByVehicleIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Rental?)null);

        var handler = new CreateRentalCommandHandler(
            mockVehicleRepository.Object,
            mockRentalRepository.Object);

        var command = new CreateRentalCommand(vehicle.Id, Guid.NewGuid());

        // Act
        var rentalId = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotEqual(Guid.Empty, rentalId);
        mockRentalRepository.Verify(r => r.AddAsync(It.IsAny<Rental>(), It.IsAny<CancellationToken>()), Times.Once);
        Assert.Equal(VehicleStatus.Rented, vehicle.Status);
    }
}
