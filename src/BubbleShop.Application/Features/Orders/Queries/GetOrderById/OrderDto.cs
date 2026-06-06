using BubbleShop.Domain.Enums;

namespace BubbleShop.Application.Features.Orders.Queries.GetOrderById;

public sealed record OrderItemDto(Guid ProductId, int Quantity, decimal UnitPrice);
public sealed record OrderDto(Guid Id, Guid CustomerId, OrderStatus Status, decimal TotalAmount, DateTimeOffset CreatedAt, IReadOnlyList<OrderItemDto> Items);
