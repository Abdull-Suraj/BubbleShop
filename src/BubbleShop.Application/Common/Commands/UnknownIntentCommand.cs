
using BubbleShop.Application.AppServices;
using BubbleShop.Application.Common.Models;
using MediatR;


namespace BubbleShop.Application.Common.Commands;

public sealed record UnknownIntentCommand(
    string Message,
    string SuggestedResponse
) : IRequest<Result<MessageResponse>>;