using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using RentingApi.Api.Features.Vehicles.Shared;
using RentingApi.Api.Tests;

/// <summary>
/// Infrastructure tests: validate the REST endpoint at host level
/// (request reception and model validation only, not full flow).
/// </summary>
public class CreateRentalEndpointTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public CreateRentalEndpointTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    [Fact]
    public async Task POST_CreateRental_WithEmptyCustomerId_Returns400()
    {
        // Arrange
        var json = """{ "customerId": "00000000-0000-0000-0000-000000000000" }""";
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/v1/vehicles/12345678-1234-1234-1234-123456789abc/rentals", content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ErrorResponse>(new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        Assert.NotNull(body);
        Assert.Contains("not found", body!.Detail);
    }

    [Fact]
    public async Task POST_CreateRental_WithInvalidCustomerIdFormat_Returns400()
    {
        // Arrange
        var json = """{ "customerId": "not-a-guid" }""";
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/v1/vehicles/12345678-1234-1234-1234-123456789abc/rentals", content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task POST_CreateRental_WithMissingBody_Returns415()
    {
        // Act
        var response = await _client.PostAsync("/api/v1/vehicles/12345678-1234-1234-1234-123456789abc/rentals", null);

        // Assert
        Assert.Equal(HttpStatusCode.UnsupportedMediaType, response.StatusCode);
    }

    [Fact]
    public async Task POST_CreateRental_WithMissingCustomerIdField_Returns400()
    {
        // Arrange
        var json = """{ "otherField": "value" }""";
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/v1/vehicles/12345678-1234-1234-1234-123456789abc/rentals", content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GET_CreateRental_WithGetMethod_Returns405()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/vehicles/12345678-1234-1234-1234-123456789abc/rentals");

        // Assert
        Assert.Equal(HttpStatusCode.MethodNotAllowed, response.StatusCode);
    }

    [Fact]
    public async Task POST_CreateRental_WithInvalidVehicleIdPath_Returns400()
    {
        // Arrange
        var json = """{ "customerId": "87654321-4321-4321-4321-cba987654321" }""";
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/v1/vehicles/not-a-guid/rentals", content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task POST_CreateRental_WithWrongContentType_Returns415()
    {
        // Act
        var content = new StringContent("{ \"customerId\": \"123\" }", Encoding.UTF8, "text/plain");

        // Act
        var response = await _client.PostAsync("/api/v1/vehicles/12345678-1234-1234-1234-123456789abc/rentals", content);

        // Assert
        Assert.Equal(HttpStatusCode.UnsupportedMediaType, response.StatusCode);
    }

}
