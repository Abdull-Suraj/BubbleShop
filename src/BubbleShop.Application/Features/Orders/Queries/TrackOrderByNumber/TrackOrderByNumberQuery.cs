
using BubbleShop.Application.Common.Models;
using BubbleShop.Application.DTOs;
using MediatR;

namespace BubbleShop.Application.Features.Orders.Queries.TrackOrderByNumber;

public sealed record TrackOrderByNumberQuery(
    string OrderNumber,
    string? Email = null
) : IRequest<Result<TrackingInfoDto>>;