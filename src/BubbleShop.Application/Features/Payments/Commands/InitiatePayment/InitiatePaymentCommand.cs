// Application/Features/Payments/Commands/InitiatePayment/InitiatePaymentCommand.cs
using BubbleShop.Application.Common.Models;
using BubbleShop.Application.DTOs;
using MediatR;

namespace BubbleShop.Application.Features.Payments.Commands.InitiatePayment;

public sealed record InitiatePaymentCommand(
    Guid OrderId,
    string Provider = "flutterwave",
    string Currency = "NGN"
) : IRequest<Result<PaymentInitiationResponse>>;  // ← This is required