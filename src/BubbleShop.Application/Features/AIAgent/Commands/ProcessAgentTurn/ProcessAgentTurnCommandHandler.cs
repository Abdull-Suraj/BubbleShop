using BubbleShop.Application.Common.Interfaces;
using BubbleShop.Application.Common.Models;
using MediatR;

namespace BubbleShop.Application.Features.AIAgent.Commands.ProcessAgentTurn;

public sealed class ProcessAgentTurnCommandHandler(IAIAgentService aiAgentService)
    : IRequestHandler<ProcessAgentTurnCommand, Result<ProcessAgentTurnResult>>
{
    public async Task<Result<ProcessAgentTurnResult>> Handle(ProcessAgentTurnCommand request, CancellationToken cancellationToken)
    {
        var response = await aiAgentService.ProcessAsync(request.History, request.NewMessage, request.CustomerId, cancellationToken);
        return Result<ProcessAgentTurnResult>.Success(new ProcessAgentTurnResult(response.TextReply, response.UpdatedHistory));
    }
}
