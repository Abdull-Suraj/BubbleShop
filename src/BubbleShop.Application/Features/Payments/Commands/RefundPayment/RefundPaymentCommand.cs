using BubbleShop.Application.Common.Models;
using BubbleShop.Application.DTOs;
using MediatR;

namespace BubbleShop.Application.Features.Payments.Commands.RefundPayment;

public sealed record RefundPaymentCommand(
    Guid PaymentId,
    decimal Amount,
    string? Reason = null
) : IRequest<Result<RefundResponseDto>>;