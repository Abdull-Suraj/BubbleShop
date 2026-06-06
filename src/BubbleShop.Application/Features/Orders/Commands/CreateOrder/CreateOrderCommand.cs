using BubbleShop.Application.Common.Models;
using MediatR;

namespace BubbleShop.Application.Features.Orders.Commands.CreateOrder;

public sealed record CreateOrderItemRequest(Guid ProductId, int Quantity);
public sealed record CreateOrderCommand(Guid CustomerId, IReadOnlyList<CreateOrderItemRequest> Items) : IRequest<Result<Guid>>;
