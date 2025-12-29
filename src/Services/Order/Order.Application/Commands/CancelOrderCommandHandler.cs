using Common.Application.Messaging;
using Microsoft.Extensions.Logging;
using Order.Domain.Repositories;

namespace Order.Application.Commands;

public class CancelOrderCommandHandler : ICommandHandler<CancelOrderCommand, CancelOrderResult>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ILogger<CancelOrderCommandHandler> _logger;

    public CancelOrderCommandHandler(
        IOrderRepository orderRepository,
        ILogger<CancelOrderCommandHandler> logger)
    {
        _orderRepository = orderRepository;
        _logger = logger;
    }

    public async Task<CancelOrderResult> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Cancelling order {OrderId} with reason: {Reason}", request.OrderId, request.Reason);

        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order == null)
        {
            _logger.LogWarning("Order {OrderId} not found", request.OrderId);
            return new CancelOrderResult(false, $"Order {request.OrderId} not found");
        }

        try
        {
            order.Cancel(request.Reason);
            await _orderRepository.UpdateAsync(order, cancellationToken);

            _logger.LogInformation("Order {OrderId} cancelled successfully", request.OrderId);
            return new CancelOrderResult(true, "Order cancelled successfully");
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to cancel order {OrderId}", request.OrderId);
            return new CancelOrderResult(false, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling order {OrderId}", request.OrderId);
            return new CancelOrderResult(false, "Failed to cancel order");
        }
    }
}
