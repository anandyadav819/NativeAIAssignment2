using Common.Application.Messaging;

namespace Order.Application.Commands;

public record AssignDriverCommand(Guid OrderId, Guid DriverId) : ICommand<AssignDriverResult>;

public record AssignDriverResult(bool Success, string Message);
