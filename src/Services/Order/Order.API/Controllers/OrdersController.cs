using MediatR;
using Microsoft.AspNetCore.Mvc;
using Order.Application.Commands;
using Order.Application.Queries;

namespace Order.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(IMediator mediator, ILogger<OrdersController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Create a new order
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderCommand command)
    {
        var result = await _mediator.Send(command);

        if (result.IsSuccess)
        {
            return CreatedAtAction(nameof(GetOrder), new { id = result.Value }, result.Value);
        }

        return BadRequest(result.Error);
    }

    /// <summary>
    /// Get order by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrder(Guid id)
    {
        var query = new GetOrderByIdQuery(id);
        var order = await _mediator.Send(query);

        if (order == null)
        {
            return NotFound();
        }

        return Ok(order);
    }

    /// <summary>
    /// Get customer orders with pagination
    /// </summary>
    [HttpGet("customer/{customerId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCustomerOrders(
        Guid customerId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var query = new GetCustomerOrdersQuery(customerId, pageNumber, pageSize);
        var result = await _mediator.Send(query);

        return Ok(result);
    }

    /// <summary>
    /// Process payment for an order
    /// </summary>
    [HttpPost("{id}/payment")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ProcessPayment(
        Guid id,
        [FromBody] ProcessPaymentRequest request)
    {
        _logger.LogInformation("Processing payment for order {OrderId}", id);

        var command = new ProcessPaymentCommand(
            id,
            request.PaymentMethod);

        var result = await _mediator.Send(command);

        if (result.Success)
        {
            return Ok(new 
            { 
                message = result.Message,
                transactionId = result.TransactionId
            });
        }

        return BadRequest(new { error = result.Message });
    }

    /// <summary>
    /// Cancel an order
    /// </summary>
    [HttpPost("{id}/cancel")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CancelOrder(
        Guid id,
        [FromBody] CancelOrderRequest request)
    {
        _logger.LogInformation("Cancelling order {OrderId}", id);

        var command = new CancelOrderCommand(id, request.Reason);
        var result = await _mediator.Send(command);

        if (result.Success)
        {
            return Ok(new { message = result.Message });
        }

        return BadRequest(new { error = result.Message });
    }

    /// <summary>
    /// Assign a driver to an order
    /// </summary>
    [HttpPost("{id}/assign-driver")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AssignDriver(
        Guid id,
        [FromBody] AssignDriverRequest request)
    {
        _logger.LogInformation(
            "Assigning driver {DriverId} to order {OrderId}",
            request.DriverId,
            id);

        var command = new AssignDriverCommand(id, request.DriverId);
        var result = await _mediator.Send(command);

        if (result.Success)
        {
            return Ok(new { message = result.Message });
        }

        return BadRequest(new { error = result.Message });
    }
}

// Request DTOs
public record ProcessPaymentRequest(string PaymentMethod);
public record CancelOrderRequest(string Reason);
public record AssignDriverRequest(Guid DriverId);
