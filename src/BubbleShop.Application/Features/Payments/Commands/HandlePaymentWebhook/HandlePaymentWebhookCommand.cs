using BubbleShop.Application.Common.Models;
using MediatR;

namespace BubbleShop.Application.Features.Payments.Commands.HandlePaymentWebhook;

public sealed record HandlePaymentWebhookCommand(Guid OrderId, string TransactionId) : IRequest<Result>;
