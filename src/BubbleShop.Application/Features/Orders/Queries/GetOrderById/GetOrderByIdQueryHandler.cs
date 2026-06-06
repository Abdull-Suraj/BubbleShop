using BubbleShop.Application.Common.Models;
using BubbleShop.Domain.Interfaces.Repositories;
using MediatR;

namespace BubbleShop.Application.Features.Orders.Queries.GetOrderById;

public sealed class GetOrderByIdQueryHandler(IOrderRepository orderRepository) : IRequestHandler<GetOrderByIdQuery, Result<OrderDto>>
{
    public async Task<Result<OrderDto>> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetWithItemsAsync(request.OrderId, cancellationToken);
        if (order is null)
        {
            return Result<OrderDto>.Failure("Order not found.");
        }

        return Result<OrderDto>.Success(new OrderDto(
            order.Id,
            order.CustomerId,
            order.Status,
            order.TotalAmount,
            order.CreatedAt,
            order.OrderItems.Select(x => new OrderItemDto(x.ProductId, x.Quantity, x.UnitPrice)).ToList()));
    }
}
