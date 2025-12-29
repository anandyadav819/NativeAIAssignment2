using Common.Application.Messaging;

namespace Order.Application.Commands;

public record CancelOrderCommand(Guid OrderId, string Reason) : ICommand<CancelOrderResult>;

public record CancelOrderResult(bool Success, string Message);
