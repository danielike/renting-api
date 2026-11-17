using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using RentingApi.Api.Features.Rentals.ReturnVehicle;
using RentingApi.Api.Features.Rentals.CreateRental;
using RentingApi.Api.Features.Vehicles.CreateVehicle;
using RentingApi.Api.Features.Vehicles.Shared;
using RentingApi.Application.Dtos;

namespace RentingApi.Api.Tests;

/// <summary>
/// Functional tests for the API using WebApplicationFactory.
/// Uses REAL DTOs from the API project to ensure contract changes are detected.
/// </summary>
public class RentingApiFunctionalTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public RentingApiFunctionalTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    private StringContent JsonContent(object obj) =>
        new(JsonSerializer.Serialize(obj), Encoding.UTF8, "application/json");

    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

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
        var response = await _client.PostAsync("/api/v1/CreateVehicle", JsonContent(request));

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<CreateVehicleResponse>(JsonOptions);
        Assert.NotNull(body);
        Assert.NotEqual(Guid.Empty, body!.Id);
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
        var response = await _client.PostAsync("/api/v1/CreateVehicle", JsonContent(request));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ErrorResponse>(JsonOptions);
        Assert.NotNull(body);
        Assert.Contains("older than 5 years", body!.Detail);
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

        await _client.PostAsync("/api/v1/CreateVehicle", JsonContent(createRequest));

        // Act
        var response = await _client.GetAsync("/api/v1/ListVehicles");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var vehicles = JsonSerializer.Deserialize<List<VehicleDto>>(
            await response.Content.ReadAsStringAsync(), JsonOptions);
        Assert.NotNull(vehicles);
        Assert.True(vehicles!.Count >= 1);
        Assert.Contains(vehicles, v => v.LicensePlate == "TEST-002");
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

        await _client.PostAsync("/api/v1/CreateVehicle", JsonContent(request));

        // Act
        var response = await _client.PostAsync("/api/v1/CreateVehicle", JsonContent(request));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ErrorResponse>(JsonOptions);
        Assert.NotNull(body);
        Assert.Contains("already exists", body!.Detail);
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

        var createResponse = await _client.PostAsync("/api/v1/CreateVehicle", JsonContent(createRequest));
        var vehicle = await createResponse.Content.ReadFromJsonAsync<CreateVehicleResponse>(JsonOptions);

        // Act
        var rentalRequest = new { customerId = Guid.NewGuid() };
        var response = await _client.PostAsync($"/api/v1/vehicles/{vehicle!.Id}/rentals", JsonContent(rentalRequest));

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<CreateRentalResponse>(JsonOptions);
        Assert.NotNull(body);
        Assert.NotEqual(Guid.Empty, body!.RentalId);
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

        var v1 = await _client.PostAsync("/api/v1/CreateVehicle", JsonContent(vehicle1Request));
        var v1Data = await v1.Content.ReadFromJsonAsync<CreateVehicleResponse>(JsonOptions);

        var v2 = await _client.PostAsync("/api/v1/CreateVehicle", JsonContent(vehicle2Request));
        var v2Data = await v2.Content.ReadFromJsonAsync<CreateVehicleResponse>(JsonOptions);

        // Rent first vehicle
        var r1 = await _client.PostAsync(
            $"/api/v1/vehicles/{v1Data!.Id}/rentals",
            JsonContent(new { customerId = customer }));
        Assert.Equal(HttpStatusCode.Created, r1.StatusCode);

        // Act - rent second vehicle with same customer
        var r2 = await _client.PostAsync(
            $"/api/v1/vehicles/{v2Data!.Id}/rentals",
            JsonContent(new { customerId = customer }));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, r2.StatusCode);
        var body = await r2.Content.ReadFromJsonAsync<ErrorResponse>(JsonOptions);
        Assert.NotNull(body);
        Assert.Contains("already has an active rental", body!.Detail);
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

        var createResponse = await _client.PostAsync("/api/v1/CreateVehicle", JsonContent(createRequest));
        var vehicle = await createResponse.Content.ReadFromJsonAsync<CreateVehicleResponse>(JsonOptions);

        var rentalRequest = new { customerId = Guid.NewGuid() };
        var rentalResponse = await _client.PostAsync(
            $"/api/v1/vehicles/{vehicle!.Id}/rentals",
            JsonContent(rentalRequest));
        var rental = await rentalResponse.Content.ReadFromJsonAsync<CreateRentalResponse>(JsonOptions);

        // Act
        var returnResponse = await _client.PostAsync(
            $"/api/v1/rentals/{rental!.RentalId}/return",
            JsonContent(new { }));

        // Assert
        Assert.Equal(HttpStatusCode.OK, returnResponse.StatusCode);
        var body = await returnResponse.Content.ReadFromJsonAsync<ReturnVehicleResponse>(JsonOptions);
        Assert.NotNull(body);
        Assert.Contains("returned successfully", body!.Message);
    }
}
