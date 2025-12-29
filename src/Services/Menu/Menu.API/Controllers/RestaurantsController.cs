using MediatR;
using Menu.Application.Commands;
using Menu.Application.Queries;
using Microsoft.AspNetCore.Mvc;

namespace Menu.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RestaurantsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<RestaurantsController> _logger;

    public RestaurantsController(IMediator mediator, ILogger<RestaurantsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Create a new restaurant
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateRestaurant([FromBody] CreateRestaurantCommand command)
    {
        var result = await _mediator.Send(command);

        if (result.IsSuccess)
        {
            return CreatedAtAction(nameof(GetRestaurant), new { id = result.Value }, result.Value);
        }

        return BadRequest(result.Error);
    }

    /// <summary>
    /// Get restaurant by ID with menu items
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRestaurant(Guid id)
    {
        var query = new GetRestaurantByIdQuery(id);
        var restaurant = await _mediator.Send(query);

        if (restaurant == null)
        {
            return NotFound();
        }

        return Ok(restaurant);
    }

    /// <summary>
    /// Get active restaurants with pagination
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetActiveRestaurants(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = new GetActiveRestaurantsQuery(pageNumber, pageSize);
        var result = await _mediator.Send(query);

        return Ok(result);
    }

    /// <summary>
    /// Add menu item to restaurant
    /// </summary>
    [HttpPost("{restaurantId}/menu-items")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddMenuItem(
        Guid restaurantId,
        [FromBody] AddMenuItemRequest request)
    {
        var command = new AddMenuItemCommand(
            restaurantId,
            request.Name,
            request.Description,
            request.Price,
            request.Category,
            request.ImageUrl);

        var result = await _mediator.Send(command);

        if (result.IsSuccess)
        {
            return CreatedAtAction(
                nameof(GetRestaurant), 
                new { id = restaurantId }, 
                result.Value);
        }

        return BadRequest(result.Error);
    }
}

public record AddMenuItemRequest(
    string Name,
    string Description,
    decimal Price,
    string Category,
    string? ImageUrl = null);
