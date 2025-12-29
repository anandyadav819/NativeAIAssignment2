using Common.Application.Messaging;
using Common.Contracts.Models;
using Order.Application.DTOs;

namespace Order.Application.Queries;

public record GetCustomerOrdersQuery(
    Guid CustomerId,
    int PageNumber = 1,
    int PageSize = 10) : IQuery<PagedResult<OrderDto>>;
