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

        var customer = customerResult.Value;

        if (customer is null)
        {
            return Result<string>.Failure("Customer not found.");
        }
        var existingConversation = await conversationRepository.GetByWhatsAppNumberAsync(request.FromNumber, cancellationToken);
        var conversation = existingConversation ??
   new Conversation(
       request.BusinessId,
       customer.Id,
       customer.WhatsAppNumber,
       customer.Name,
       "whatsapp");
        var history = conversation.ToChatHistory();
        history.Add(new ChatMessage { Role = ChatRole.User, Content = request.MessageText, Timestamp = DateTime.UtcNow });

        var aiResult = await mediator.Send(new ProcessAgentTurnCommand(history, request.MessageText, customer.Id.ToString()), cancellationToken);
        if (aiResult.IsFailure || aiResult.Value is null)
        {
            return Result<string>.Failure(aiResult.Error ?? "Unable to process message.");
        }

        conversation.AddMessage(
    request.MessageText,
    customer.Name,
    true);

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
