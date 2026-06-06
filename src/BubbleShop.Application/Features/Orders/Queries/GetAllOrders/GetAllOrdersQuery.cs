using BubbleShop.Application.Common.Models;
using BubbleShop.Application.Features.Orders.Queries.GetOrderById;
using MediatR;

namespace BubbleShop.Application.Features.Orders.Queries.GetAllOrders;

public sealed record GetAllOrdersQuery : IRequest<Result<IReadOnlyList<OrderDto>>>;
