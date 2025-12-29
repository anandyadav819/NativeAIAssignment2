using MediatR;

namespace Common.Application.Messaging;

/// <summary>
/// Marker interface for commands (operations that change state)
/// </summary>
public interface ICommand : IRequest
{
}

/// <summary>
/// Marker interface for commands that return a result
/// </summary>
public interface ICommand<out TResponse> : IRequest<TResponse>
{
}
