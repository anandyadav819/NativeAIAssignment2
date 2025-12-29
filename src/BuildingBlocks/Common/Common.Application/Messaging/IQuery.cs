using MediatR;

namespace Common.Application.Messaging;

/// <summary>
/// Marker interface for queries (operations that read data)
/// </summary>
public interface IQuery<out TResponse> : IRequest<TResponse>
{
}
