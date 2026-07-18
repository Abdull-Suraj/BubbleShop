
using BubbleShop.Application.AppServices;
using BubbleShop.Application.Common.Models;
using MediatR;

namespace BubbleShop.Application.Features.Unknown.Commands.Unknown;

public sealed record UnknownCommand(
    string Channel,
    Guid CustomerId,
    Guid BusinessId,
    string Message,
    string SuggestedResponse
) : IRequest<Result<MessageResponse>>;