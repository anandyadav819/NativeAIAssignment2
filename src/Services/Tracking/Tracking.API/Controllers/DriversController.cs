using MediatR;
using Microsoft.AspNetCore.Mvc;
using Tracking.Application.Commands;
using Tracking.Application.DTOs;

namespace Tracking.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DriversController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<DriversController> _logger;

    public DriversController(IMediator mediator, ILogger<DriversController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpPost("{driverId}/location")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DriverDto>> UpdateLocation(
        Guid driverId,
        [FromBody] LocationDto location,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating location for driver {DriverId}", driverId);

        var command = new UpdateDriverLocationCommand(
            driverId,
            location.Latitude,
            location.Longitude);

        var result = await _mediator.Send(command, cancellationToken);

        return Ok(result);
    }
}
