// Application/Features/Orders/Queries/GetOrdersByBusiness/GetOrdersByBusinessQuery.cs
using BubbleShop.Application.Common.Models;
using BubbleShop.Application.DTOs;
using BubbleShop.Domain.Enums;
using MediatR;

namespace BubbleShop.Application.Features.Orders.Queries.GetOrdersByBusiness;

public sealed record GetOrdersByBusinessQuery(
    Guid BusinessId,
    int PageNumber = 1,
    int PageSize = 20,
    string? Status = null
) : IRequest<Result<PagedResult<OrderSummaryDto>>>;