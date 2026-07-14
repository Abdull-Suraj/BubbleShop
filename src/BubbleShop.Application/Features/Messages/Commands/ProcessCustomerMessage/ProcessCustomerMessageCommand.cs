
using BubbleShop.Application.AppServices;
using BubbleShop.Application.Common.Models;
using MediatR;

namespace BubbleShop.Application.Features.Messages.Commands.ProcessCustomerMessage;

public sealed record ProcessCustomerMessageCommand(
    string Channel,
    string CustomerId,
    string? CustomerName,
    string? CustomerWhatsApp,
    Guid BusinessId,
    string Message,
    Guid? ConversationId = null
) : IRequest<Result<MessageResponse>>;

