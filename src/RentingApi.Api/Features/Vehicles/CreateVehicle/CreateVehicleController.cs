using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using RentingApi.Application.Abstractions;
using RentingApi.Application.Features.Vehicles.CreateVehicle;
using RentingApi.Api.Features.Vehicles.Shared;

namespace RentingApi.Api.Features.Vehicles.CreateVehicle;

/// <summary>
/// Controller for vehicle creation.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}")]
public class CreateVehicleController : ControllerBase
{
    private readonly Mediator _mediator;

    public CreateVehicleController(Mediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Creates a new vehicle for the fleet.
    /// </summary>
    /// <param name="request">Vehicle details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created vehicle ID</returns>
    [HttpPost("/vehicles")]
    [ProducesResponseType(typeof(CreateVehicleResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateVehicle(
        [FromBody] CreateVehicleRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateVehicleCommand(
            Guid.NewGuid(),
            request.LicensePlate,
            request.Brand,
            request.Model,
            request.Year,
            request.ManufactureDate
        );

        var id = (Guid)await _mediator.Send<CreateVehicleCommand, Guid>(command, cancellationToken);
        var location = "/api/v1/vehicles/" + id;
        return Created(location, new CreateVehicleResponse(id));
    }
}
