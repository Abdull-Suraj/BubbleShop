
using BubbleShop.Application.Common.Models;
using BubbleShop.Application.DTOs;
using MediatR;

namespace BubbleShop.Application.Features.Messages.Queries.GetConversationHistory;

public sealed record GetConversationHistoryQuery(
    string CustomerId,
    Guid BusinessId,
    string Channel
) : IRequest<Result<ConversationHistoryDto>>;