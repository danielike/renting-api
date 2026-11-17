using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using RentingApi.Application.Dtos;
using RentingApi.Application.Abstractions;
using RentingApi.Application.Features.Vehicles.ListVehicles;

namespace RentingApi.Api.Features.Vehicles.ListVehicles;

/// <summary>
/// Controller for listing available vehicles.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}")]
public class ListVehiclesController : ControllerBase
{
    private readonly Mediator _mediator;

    public ListVehiclesController(Mediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Lists all available vehicles in the fleet.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of available vehicles</returns>
    [HttpGet("vehicles")]
    [ProducesResponseType(typeof(IEnumerable<VehicleDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListVehicles(CancellationToken cancellationToken)
    {
        var query = new ListVehiclesQuery();
        var vehicles = await _mediator.Query<ListVehiclesQuery, IEnumerable<VehicleDto>>(query, cancellationToken);
        return Ok(vehicles);
    }
}