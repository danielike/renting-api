using Microsoft.AspNetCore.Mvc.Testing;
using RentingApi.Domain.Entities;
using RentingApi.Infrastructure.Persistence;

namespace RentingApi.Api.Tests;

/// <summary>
/// Functional tests for the API using WebApplicationFactory.
/// </summary>
public class RentingApiFunctionalTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public RentingApiFunctionalTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task POST_CreateVehicle_Returns201WithVehicleId()
    {
        // Arrange
        var request = new
        {
            licensePlate = "TEST-001",
            brand = "Toyota",
            model = "Corolla",
            year = 2024,
            manufactureDate = "2024-01-15T00:00:00Z"
        };

        // Act
        var response = await _client.PostAsync("/api/v1/CreateVehicle",
            new StringContent(JsonSerializer.Serialize(request), System.Text.Encoding.UTF8, "application/json"));

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<CreateVehicleResponse>();
        body.Should().NotBeNull();
        body!.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task POST_CreateOldVehicle_Returns400()
    {
        // Arrange - vehicle older than 5 years
        var request = new
        {
            licensePlate = "OLD-001",
            brand = "Ford",
            model = "Focus",
            year = 2018,
            manufactureDate = "2018-06-01T00:00:00Z"
        };

        // Act
        var response = await _client.PostAsync("/api/v1/CreateVehicle",
            new StringContent(JsonSerializer.Serialize(request), System.Text.Encoding.UTF8, "application/json"));

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
        var body = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        body.Should().NotBeNull();
        body!.Detail.Should().Contain("older than 5 years");
    }

    [Fact]
    public async Task GET_ListVehicles_ReturnsCreatedVehicle()
    {
        // Arrange - create a vehicle first
        var createRequest = new
        {
            licensePlate = "TEST-002",
            brand = "Honda",
            model = "Civic",
            year = 2024,
            manufactureDate = "2024-01-15T00:00:00Z"
        };

        await _client.PostAsync("/api/v1/CreateVehicle",
            new StringContent(JsonSerializer.Serialize(createRequest), System.Text.Encoding.UTF8, "application/json"));

        // Act
        var response = await _client.GetAsync("/api/v1/ListVehicles");

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var vehicles = await response.Content.ReadFromJsonAsync<List<VehicleDto>>();
        vehicles.Should().NotBeNull();
        vehicles!.Count.Should().BeGreaterOrEqualTo(1);
        vehicles.Should().Contain(v => v.LicensePlate == "TEST-002");
    }

    [Fact]
    public async Task POST_CreateDuplicateVehicle_Returns400()
    {
        // Arrange - create two vehicles with the same plate
        var request = new
        {
            licensePlate = "DUP-001",
            brand = "Nissan",
            model = "Sentra",
            year = 2024,
            manufactureDate = "2024-01-15T00:00:00Z"
        };

        await _client.PostAsync("/api/v1/CreateVehicle",
            new StringContent(JsonSerializer.Serialize(request), System.Text.Encoding.UTF8, "application/json"));

        // Act
        var response = await _client.PostAsync("/api/v1/CreateVehicle",
            new StringContent(JsonSerializer.Serialize(request), System.Text.Encoding.UTF8, "application/json"));

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
        var body = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        body!.Detail.Should().Contain("already exists");
    }

    [Fact]
    public async Task POST_RentVehicle_Returns201WithRentalId()
    {
        // Arrange - create a vehicle first
        var createRequest = new
        {
            licensePlate = "RENT-001",
            brand = "Chevrolet",
            model = "Cruze",
            year = 2024,
            manufactureDate = "2024-01-15T00:00:00Z"
        };

        var createResponse = await _client.PostAsync("/api/v1/CreateVehicle",
            new StringContent(JsonSerializer.Serialize(createRequest), System.Text.Encoding.UTF8, "application/json"));
        var vehicle = await createResponse.Content.ReadFromJsonAsync<CreateVehicleResponse>();

        // Act
        var rentalRequest = new { customerId = Guid.NewGuid() };
        var response = await _client.PostAsync($"/api/v1/vehicles/{vehicle!.Id}/rentals",
            new StringContent(JsonSerializer.Serialize(rentalRequest), System.Text.Encoding.UTF8, "application/json"));

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<CreateRentalResponse>();
        body.Should().NotBeNull();
        body!.RentalId.Should().NotBeEmpty();
    }

    [Fact]
    public async Task POST_RentSameCustomerTwice_Returns400()
    {
        // Arrange - create two vehicles
        var vehicle1Request = new
        {
            licensePlate = "SINGLE-001",
            brand = "VW",
            model = "Golf",
            year = 2024,
            manufactureDate = "2024-01-15T00:00:00Z"
        };

        var vehicle2Request = new
        {
            licensePlate = "SINGLE-002",
            brand = "Audi",
            model = "A3",
            year = 2024,
            manufactureDate = "2024-01-15T00:00:00Z"
        };

        var customer = Guid.NewGuid();

        var v1 = await _client.PostAsync("/api/v1/CreateVehicle",
            new StringContent(JsonSerializer.Serialize(vehicle1Request), System.Text.Encoding.UTF8, "application/json"));
        var v1Data = await v1.Content.ReadFromJsonAsync<CreateVehicleResponse>();

        var v2 = await _client.PostAsync("/api/v1/CreateVehicle",
            new StringContent(JsonSerializer.Serialize(vehicle2Request), System.Text.Encoding.UTF8, "application/json"));
        var v2Data = await v2.Content.ReadFromJsonAsync<CreateVehicleResponse>();

        // Rent first vehicle
        var r1 = await _client.PostAsync($"/api/v1/vehicles/{v1Data!.Id}/rentals",
            new StringContent(JsonSerializer.Serialize(new { customerId = customer }), System.Text.Encoding.UTF8, "application/json"));
        r1.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);

        // Act - rent second vehicle with same customer
        var r2 = await _client.PostAsync($"/api/v1/vehicles/{v2Data!.Id}/rentals",
            new StringContent(JsonSerializer.Serialize(new { customerId = customer }), System.Text.Encoding.UTF8, "application/json"));

        // Assert
        r2.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
        var body = await r2.Content.ReadFromJsonAsync<ErrorResponse>();
        body!.Detail.Should().Contain("already has an active rental");
    }

    [Fact]
    public async Task POST_ReturnVehicle_Returns200()
    {
        // Arrange - create a vehicle and rent it
        var createRequest = new
        {
            licensePlate = "RETURN-001",
            brand = "Hyundai",
            model = "Elantra",
            year = 2024,
            manufactureDate = "2024-01-15T00:00:00Z"
        };

        var createResponse = await _client.PostAsync("/api/v1/CreateVehicle",
            new StringContent(JsonSerializer.Serialize(createRequest), System.Text.Encoding.UTF8, "application/json"));
        var vehicle = await createResponse.Content.ReadFromJsonAsync<CreateVehicleResponse>();

        var rentalRequest = new { customerId = Guid.NewGuid() };
        var rentalResponse = await _client.PostAsync($"/api/v1/vehicles/{vehicle!.Id}/rentals",
            new StringContent(JsonSerializer.Serialize(rentalRequest), System.Text.Encoding.UTF8, "application/json"));
        var rental = await rentalResponse.Content.ReadFromJsonAsync<CreateRentalResponse>();

        // Act
        var returnResponse = await _client.PostAsync($"/api/v1/rentals/{rental!.RentalId}/return",
            new StringContent("{}", System.Text.Encoding.UTF8, "application/json"));

        // Assert
        returnResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var body = await returnResponse.Content.ReadFromJsonAsync<ReturnVehicleResponse>();
        body!.Message.Should().Contain("returned successfully");
    }

    // Response DTOs
    private record CreateVehicleResponse(Guid Id);
    private record ErrorResponse(string Title, string Detail);
    private record VehicleDto(Guid Id, string LicensePlate, string Brand, string Model, int Year, string Status);
    private record CreateRentalResponse(Guid RentalId);
    private record ReturnVehicleResponse(string Message);
}
