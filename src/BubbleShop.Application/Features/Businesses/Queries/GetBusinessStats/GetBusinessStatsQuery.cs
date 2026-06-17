using BubbleShop.Application.Common.Models;
using BubbleShop.Application.DTOs;
using MediatR;

namespace BubbleShop.Application.Features.Businesses.Queries.GetBusinessStats;

public sealed record GetBusinessStatsQuery(
    Guid BusinessId,
    DateTime? FromDate = null,
    DateTime? ToDate = null
) : IRequest<Result<BusinessStatsDto>>;