// Application/Features/Payments/Queries/GetCustomerPayments/GetCustomerPaymentsQuery.cs
using BubbleShop.Application.Common.Models;
using BubbleShop.Application.DTOs;
using MediatR;

namespace BubbleShop.Application.Features.Payments.Queries.GetCustomerPayments;

public sealed record GetCustomerPaymentsQuery(
    Guid CustomerId,
    int PageNumber = 1,
    int PageSize = 20
) : IRequest<Result<PagedResult<PaymentHistoryDto>>>;