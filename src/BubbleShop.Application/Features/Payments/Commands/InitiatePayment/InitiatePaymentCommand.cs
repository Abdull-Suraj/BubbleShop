using BubbleShop.Application.Common.Models;
using MediatR;

namespace BubbleShop.Application.Features.Payments.Commands.InitiatePayment;

public sealed record InitiatePaymentCommand(Guid OrderId) : IRequest<Result<string>>;
