using BubbleShop.Application.Common.Models;
using BubbleShop.Application.DTOs;
using MediatR;

namespace BubbleShop.Application.Features.Payments.Queries.GetPaymentHistory;

public sealed record GetPaymentHistoryQuery(
    Guid BusinessId,
    DateTime? FromDate = null,
    DateTime? ToDate = null,
    int PageNumber = 1,
    int PageSize = 20
) : IRequest<Result<PagedResult<PaymentHistoryDto>>>;