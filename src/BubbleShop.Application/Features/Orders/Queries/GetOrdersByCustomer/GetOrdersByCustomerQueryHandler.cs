using BubbleShop.Application.Common.Models;
using BubbleShop.Application.Features.Orders.Queries.GetOrderById;
using BubbleShop.Domain.Interfaces.Repositories;
using MediatR;

namespace BubbleShop.Application.Features.Orders.Queries.GetOrdersByCustomer;

public sealed class GetOrdersByCustomerQueryHandler(IOrderRepository orderRepository)
    : IRequestHandler<GetOrdersByCustomerQuery, Result<IReadOnlyList<OrderDto>>>
{
    public async Task<Result<IReadOnlyList<OrderDto>>> Handle(GetOrdersByCustomerQuery request, CancellationToken cancellationToken)
    {
        var orders = await orderRepository.GetByCustomerIdAsync(request.CustomerId, cancellationToken);
        var dtos = orders.Select(order => new OrderDto(
            order.Id,
            order.CustomerId,
            order.Status,
            order.TotalAmount,
            order.CreatedAt,
            order.OrderItems.Select(x => new OrderItemDto(x.ProductId, x.Quantity, x.UnitPrice)).ToList())).ToList();

        return Result<IReadOnlyList<OrderDto>>.Success(dtos);
    }
}
