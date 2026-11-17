using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using RentingApi.Api.Features.Vehicles.Shared;
using RentingApi.Domain.Entities;

namespace RentingApi.Api.Tests;

/// <summary>
/// Infrastructure tests: validate the REST endpoint at host level
/// (request reception and model validation only, not full flow).
/// </summary>
public class CreateRentalEndpointTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private StringContent JsonContent(object obj) => new(JsonSerializer.Serialize(obj), Encoding.UTF8, "application/json");

    public CreateRentalEndpointTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    [Fact]
    public async Task POST_CreateRental_WithEmptyCustomerId_ReturnsDomainError()
    {
        // Arrange
        var customerId = new { customerId = Guid.CreateVersion7().ToString() };
        
        var vehicle = new Vehicle(
            Guid.NewGuid(),
            "ABC-111",
            "Mazda",
            "3",
            2020,
            new DateTime(2020, 1, 15));
        // Act
        await _client.PostAsync($"/vehicles", JsonContent(vehicle));
        var response = await _client.PostAsync($"/api/v1/vehicles/{vehicle.Id}/rentals", JsonContent(customerId));
        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ErrorResponse>(new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        Assert.NotNull(body);
        Assert.Contains("Domain Error", body.Title);
        Assert.Contains("not found", body!.Detail);
    }
}
