using MediatR;
using Microsoft.AspNetCore.Mvc;
using Tracking.Application.DTOs;
using Tracking.Application.Queries;

namespace Tracking.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TrackingController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<TrackingController> _logger;

    public TrackingController(IMediator mediator, ILogger<TrackingController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpGet("order/{orderId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DeliveryTrackingDto>> GetDeliveryTracking(
        Guid orderId,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting delivery tracking for order {OrderId}", orderId);

        var query = new GetDeliveryTrackingByOrderIdQuery(orderId);
        var result = await _mediator.Send(query, cancellationToken);

        if (result == null)
        {
            return NotFound($"Delivery tracking not found for order {orderId}");
        }

        return Ok(result);
    }

    [HttpGet("drivers/available")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<DriverDto>>> GetAvailableDrivers(
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting available drivers");

        var query = new GetAvailableDriversQuery();
        var result = await _mediator.Send(query, cancellationToken);

        return Ok(result);
    }
}
