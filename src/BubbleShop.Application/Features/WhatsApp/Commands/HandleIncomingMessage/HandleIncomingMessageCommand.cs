using BubbleShop.Application.Common.Models;
using MediatR;

namespace BubbleShop.Application.Features.WhatsApp.Commands.HandleIncomingMessage;

public sealed record HandleIncomingMessageCommand(
    Guid BusinessId,
    string FromNumber,
    string MessageText
) : IRequest<Result<string>>;