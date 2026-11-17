using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using RentingApi.Api.Features.Rentals.CreateRental;
using RentingApi.Api.Features.Vehicles.Shared;
using RentingApi.Application.Abstractions;
using RentingApi.Application.Features.Rentals.CreateRental;

namespace RentingApi.Api.Features.Rentals.CreateRental;

/// <summary>
/// Controller for creating rentals.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}")]
public class CreateRentalController : ControllerBase
{
    private readonly Mediator _mediator;

    public CreateRentalController(Mediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Rents a vehicle to a customer.
    /// </summary>
    /// <param name="vehicleId">The vehicle to rent</param>
    /// <param name="request">Customer information</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created rental ID</returns>
    [HttpPost("/vehicles/{vehicleId:guid}/rentals")]
    [ProducesResponseType(typeof(CreateRentalResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateRental(
        Guid vehicleId,
        [FromBody] CreateRentalRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateRentalCommand(vehicleId, request.CustomerId);
        var rentalId = (Guid)await _mediator.Send<CreateRentalCommand, Guid>(command, cancellationToken);
        var location = "/api/v1/vehicles/" + vehicleId + "/rentals/" + rentalId;
        return Created(location, new CreateRentalResponse(rentalId));
    }
}
