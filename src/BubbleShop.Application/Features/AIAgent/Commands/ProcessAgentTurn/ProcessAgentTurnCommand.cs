using BubbleShop.Application.Common.Models;
using BubbleShop.Domain.Entities;
using MediatR;

namespace BubbleShop.Application.Features.AIAgent.Commands.ProcessAgentTurn;

public sealed record ProcessAgentTurnCommand(List<ChatMessage> History, string NewMessage, string CustomerId)
    : IRequest<Result<ProcessAgentTurnResult>>;

public sealed record ProcessAgentTurnResult(string TextReply, List<ChatMessage> UpdatedHistory);
