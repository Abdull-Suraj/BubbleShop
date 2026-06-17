using BubbleShop.Application.Common.Models;
using BubbleShop.Application.DTOs;
using MediatR;

namespace BubbleShop.Application.Features.Payments.Queries.GetPaymentStatus;

public sealed record GetPaymentStatusQuery(string TransactionReference) : IRequest<Result<PaymentStatusDto>>;