using BubbleShop.Application.Common.Models;
using MediatR;

namespace BubbleShop.Application.Features.Orders.Queries.GetOrderById;

public sealed record GetOrderByIdQuery(Guid OrderId) : IRequest<Result<OrderDto>>;
