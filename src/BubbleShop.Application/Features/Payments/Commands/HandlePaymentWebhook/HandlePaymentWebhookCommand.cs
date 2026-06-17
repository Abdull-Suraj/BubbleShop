// Application/Features/Payments/Commands/HandlePaymentWebhook/HandlePaymentWebhookCommand.cs
using BubbleShop.Application.Common.Models;
using MediatR;

namespace BubbleShop.Application.Features.Payments.Commands.HandlePaymentWebhook;

public sealed record HandlePaymentWebhookCommand(
    Guid OrderId,
    string TransactionId,
    string? GatewayResponse = null,
    string? Provider = "Flutterwave"
) : IRequest<Result>;