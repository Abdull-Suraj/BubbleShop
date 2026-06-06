using BubbleShop.Application.Common.Models;
using BubbleShop.Application.Features.Orders.Queries.GetOrderById;
using MediatR;

namespace BubbleShop.Application.Features.Orders.Queries.GetOrdersByCustomer;

public sealed record GetOrdersByCustomerQuery(Guid CustomerId) : IRequest<Result<IReadOnlyList<OrderDto>>>;
