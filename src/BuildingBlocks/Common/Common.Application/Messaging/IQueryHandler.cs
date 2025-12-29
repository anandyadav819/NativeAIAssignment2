using MediatR;

namespace Common.Application.Messaging;

/// <summary>
/// Handler for queries
/// </summary>
public interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, TResponse>
    where TQuery : IQuery<TResponse>
{
}
