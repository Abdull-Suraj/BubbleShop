// Application/Features/Orders/Queries/GetAllOrders/GetAllOrdersQuery.cs
using BubbleShop.Application.Common.Models;
using BubbleShop.Application.Features.Orders.Queries.GetOrderById;
using MediatR;

namespace BubbleShop.Application.Features.Orders.Queries.GetAllOrders;

public sealed record GetAllOrdersQuery(
    int PageNumber = 1,
    int PageSize = 20,
    Guid? BusinessId = null,
    string? Status = null,
    DateTime? FromDate = null,
    DateTime? ToDate = null
) : IRequest<Result<IReadOnlyList<OrderDto>>>;