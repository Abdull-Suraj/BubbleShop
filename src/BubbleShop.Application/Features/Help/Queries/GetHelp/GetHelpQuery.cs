
using BubbleShop.Application.AppServices;
using BubbleShop.Application.Common.Models;
using MediatR;

namespace BubbleShop.Application.Features.Help.Queries.GetHelp;

public sealed record GetHelpQuery(
    string Channel,
    Guid CustomerId,
    Guid BusinessId,
    string Topic,
    string Message
) : IRequest<Result<MessageResponse>>;