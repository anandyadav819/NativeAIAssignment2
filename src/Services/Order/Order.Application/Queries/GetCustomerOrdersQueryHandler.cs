using Common.Application.Messaging;
using Common.Contracts.Models;
using Order.Application.DTOs;
using Order.Domain.Repositories;

namespace Order.Application.Queries;

public class GetCustomerOrdersQueryHandler : IQueryHandler<GetCustomerOrdersQuery, PagedResult<OrderDto>>
{
    private readonly IOrderRepository _orderRepository;

    public GetCustomerOrdersQueryHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<PagedResult<OrderDto>> Handle(GetCustomerOrdersQuery request, CancellationToken cancellationToken)
    {
        var orders = await _orderRepository.GetByCustomerIdAsync(
            request.CustomerId,
            request.PageNumber,
            request.PageSize,
            cancellationToken);

        var totalCount = await _orderRepository.GetCustomerOrderCountAsync(
            request.CustomerId,
            cancellationToken);

        var orderDtos = orders.Select(order => new OrderDto
        {
            Id = order.Id,
            CustomerId = order.CustomerId,
            RestaurantId = order.RestaurantId,
            Status = order.Status,
            TotalAmount = order.TotalAmount,
            DeliveryAddress = order.DeliveryAddress,
            DriverId = order.DriverId,
            CreatedAt = order.CreatedAt,
            Items = order.Items.Select(i => new OrderItemDto
            {
                Id = i.Id,
                MenuItemId = i.MenuItemId,
                Name = i.Name,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice
            }).ToList()
        }).ToList();

        return new PagedResult<OrderDto>(orderDtos, totalCount, request.PageNumber, request.PageSize);
    }
}
