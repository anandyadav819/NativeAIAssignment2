using Common.Application.Messaging;
using Order.Application.DTOs;

namespace Order.Application.Queries;

public record GetOrderByIdQuery(Guid OrderId) : IQuery<OrderDto?>;
