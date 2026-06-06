using BubbleShop.Application.Common.Interfaces;
using BubbleShop.Application.Common.Models;
using BubbleShop.Application.Features.AIAgent.Commands.ProcessAgentTurn;
using BubbleShop.Application.Features.Customers.Commands.CreateOrUpdateCustomer;
using BubbleShop.Domain.Entities;
using BubbleShop.Domain.Enums;
using BubbleShop.Domain.Interfaces.Repositories;
using MediatR;

namespace BubbleShop.Application.Features.WhatsApp.Commands.HandleIncomingMessage;

public sealed class HandleIncomingMessageCommandHandler(
    IMediator mediator,
    IConversationRepository conversationRepository,
    IUnitOfWork unitOfWork,
    IWhatsAppService whatsAppService)
    : IRequestHandler<HandleIncomingMessageCommand, Result<string>>
{
    public async Task<Result<string>> Handle(HandleIncomingMessageCommand request, CancellationToken cancellationToken)
    {
        var customerResult = await mediator.Send(new CreateOrUpdateCustomerCommand(request.FromNumber, "Customer", null, null), cancellationToken);
        if (customerResult.IsFailure)
        {
            return Result<string>.Failure(customerResult.Error ?? "Unable to upsert customer.");
        }

        var customerId = customerResult.Value;
        var existingConversation = await conversationRepository.GetByWhatsAppNumberAsync(request.FromNumber, cancellationToken);
        var conversation = existingConversation ?? Conversation.Create(customerId, request.FromNumber);

        var history = conversation.MessageHistory;
        history.Add(new ChatMessage { Role = ChatRole.User, Content = request.MessageText, Timestamp = DateTimeOffset.UtcNow });

        var aiResult = await mediator.Send(new ProcessAgentTurnCommand(history, request.MessageText, customerId.ToString()), cancellationToken);
        if (aiResult.IsFailure || aiResult.Value is null)
        {
            return Result<string>.Failure(aiResult.Error ?? "Unable to process message.");
        }

        conversation.UpdateHistory(aiResult.Value.UpdatedHistory);

        if (existingConversation is null)
        {
            await conversationRepository.AddAsync(conversation, cancellationToken);
        }
        else
        {
            await conversationRepository.UpdateAsync(conversation, cancellationToken);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await whatsAppService.SendMessageAsync(request.FromNumber, aiResult.Value.TextReply, cancellationToken);

        return Result<string>.Success(aiResult.Value.TextReply);
    }
}
