using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using RentingApi.Api.Features.Rentals.ReturnVehicle;
using RentingApi.Api.Features.Vehicles.Shared;
using RentingApi.Application.Abstractions;
using RentingApi.Application.Features.Rentals.ReturnVehicle;

namespace RentingApi.Api.Features.Rentals;

/// <summary>
/// Controller for returning rented vehicles.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}")]
public class ReturnVehicleController : ControllerBase
{
    private readonly Mediator _mediator;

    public ReturnVehicleController(Mediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Returns a rented vehicle.
    /// </summary>
    /// <param name="rentalId">The rental to return</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Confirmation message</returns>
    [HttpPost("/rentals/{rentalId:guid}/return")]
    [ProducesResponseType(typeof(ReturnVehicleResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ReturnVehicle(
        Guid rentalId,
        CancellationToken cancellationToken)
    {
        // Get the rental to find the associated vehicle
        var vehicleId = await _mediator.Query<ReturnVehicleQuery, Guid>(new ReturnVehicleQuery(rentalId), cancellationToken);

        var command = new ReturnVehicleCommand(rentalId, vehicleId);
        await _mediator.Send<ReturnVehicleCommand>(command, cancellationToken);
        return Ok(new ReturnVehicleResponse("Vehicle returned successfully."));
    }
}
