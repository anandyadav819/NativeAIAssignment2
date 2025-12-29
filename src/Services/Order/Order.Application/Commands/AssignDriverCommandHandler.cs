using Common.Application.Messaging;
using Microsoft.Extensions.Logging;
using Order.Domain.Repositories;

namespace Order.Application.Commands;

public class AssignDriverCommandHandler : ICommandHandler<AssignDriverCommand, AssignDriverResult>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ILogger<AssignDriverCommandHandler> _logger;

    public AssignDriverCommandHandler(
        IOrderRepository orderRepository,
        ILogger<AssignDriverCommandHandler> logger)
    {
        _orderRepository = orderRepository;
        _logger = logger;
    }

    public async Task<AssignDriverResult> Handle(AssignDriverCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Assigning driver {DriverId} to order {OrderId}",
            request.DriverId,
            request.OrderId);

        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order == null)
        {
            _logger.LogWarning("Order {OrderId} not found", request.OrderId);
            return new AssignDriverResult(false, $"Order {request.OrderId} not found");
        }

        try
        {
            order.AssignDriver(request.DriverId);
            await _orderRepository.UpdateAsync(order, cancellationToken);

            _logger.LogInformation(
                "Driver {DriverId} assigned to order {OrderId} successfully",
                request.DriverId,
                request.OrderId);

            return new AssignDriverResult(true, "Driver assigned successfully");
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to assign driver to order {OrderId}", request.OrderId);
            return new AssignDriverResult(false, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning driver to order {OrderId}", request.OrderId);
            return new AssignDriverResult(false, "Failed to assign driver");
        }
    }
}
