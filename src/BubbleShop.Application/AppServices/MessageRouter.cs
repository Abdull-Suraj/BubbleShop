
using BubbleShop.Application.Common.Interfaces;
using BubbleShop.Application.Common.Models;
using BubbleShop.Application.Features.Messages.Commands.ProcessCustomerMessage;
using BubbleShop.Domain.Common;
using BubbleShop.Domain.Entities;
using BubbleShop.Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace BubbleShop.Application.AppServices;

public class MessageRouter : IMessageRouter
{
    private readonly IAIIntentService _aiIntentService;
    private readonly ICommandFactory _commandFactory;
    private readonly IMediator _mediator;
    private readonly ICustomerRepository _customerRepository;
    private readonly IBusinessRepository _businessRepository;
    private readonly IConversationRepository _conversationRepository;
    private readonly ILogger<MessageRouter> _logger;

    public MessageRouter(
        IAIIntentService aiIntentService,
        ICommandFactory commandFactory,
        IMediator mediator,
        ICustomerRepository customerRepository,
        IBusinessRepository businessRepository,
        IConversationRepository conversationRepository,
        ILogger<MessageRouter> logger)
    {
        _aiIntentService = aiIntentService;
        _commandFactory = commandFactory;
        _mediator = mediator;
        _customerRepository = customerRepository;
        _businessRepository = businessRepository;
        _conversationRepository = conversationRepository;
        _logger = logger;
    }

    public async Task<MessageResponse> ProcessIncomingMessageAsync(
        string message,
        MessageContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // 1. Detect Business
            var business = await DetectBusinessAsync(context, cancellationToken);
            if (business is null)
                return MessageResponse.Error("Business not found");

            // 2. Detect/Get Customer
            var customer = await GetOrCreateCustomerAsync(context, business.Id, cancellationToken);

            // 3. Get or Create Conversation
            var conversation = await GetOrCreateConversationAsync(context, customer.Id, cancellationToken);

            // 4. Save Customer Message
            conversation.AddMessage(message, customer.Name, true);
            await _conversationRepository.UpdateAsync(conversation, cancellationToken);

            // 5. Analyze Intent with AI
            var intent = await _aiIntentService.AnalyzeIntentAsync(message, context, cancellationToken);

            // 6. Create Command
            var command = await _commandFactory.CreateCommandAsync(intent, context, cancellationToken);

            // 7. Execute Command via MediatR
            var result = await _mediator.Send(command, cancellationToken);

            // 8. Generate Response
            var responseText = await GenerateResponseAsync(intent, context, result, cancellationToken);

            // 9. Build Interactive Response
            var interactiveResponse = BuildInteractiveResponse(intent, result, responseText);

            // 10. Save Assistant Response
            conversation.AddMessage(responseText, "AI Assistant", false);
            await _conversationRepository.UpdateAsync(conversation, cancellationToken);

            return MessageResponse.Success(
                text: responseText,
                interactive: interactiveResponse,
                conversationId: conversation.Id,
                intent: intent.Intent.ToString()
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing message from {Channel}", context.Channel);
            return MessageResponse.Error("I'm sorry, I'm having trouble processing your request. Please try again later. 🙏");
        }
    }

    private async Task<Business?> DetectBusinessAsync(MessageContext context, CancellationToken cancellationToken)
    {
        var businessId = Guid.Parse(context.BusinessId);
        return await _businessRepository.GetByIdAsync(businessId, cancellationToken);
    }

    private async Task<Customer> GetOrCreateCustomerAsync(MessageContext context, Guid businessId, CancellationToken cancellationToken)
    {
        var customer = await _customerRepository.GetByWhatsAppNumberAsync(
            context.ChannelUserId,
            businessId,
            cancellationToken);

        if (customer is not null)
            return customer;

        var customerName = context.Metadata?.GetValueOrDefault("CustomerName") ?? "Valued Customer";
        var firstName = customerName.Split(' ').FirstOrDefault() ?? "Valued";
        var lastName = customerName.Split(' ').Skip(1).FirstOrDefault() ?? "Customer";

        var newCustomer = new Customer(
            businessId: businessId,
            name: customerName,
            whatsappNumber: context.ChannelUserId,
            phoneNumber: context.ChannelUserId,
            email: null
        );

        await _customerRepository.AddAsync(newCustomer, cancellationToken);
        return newCustomer;
    }

    private async Task<Conversation> GetOrCreateConversationAsync(MessageContext context, Guid customerId, CancellationToken cancellationToken)
    {
        var conversation = await _conversationRepository.GetByCustomerAndChannelAsync(
            context.ChannelUserId,
            Guid.Parse(context.BusinessId),
            context.Channel.ToString(),
            cancellationToken);

        if (conversation is not null)
            return conversation;

        var newConversation = new Conversation(
            businessId: Guid.Parse(context.BusinessId),
            whatsAppNumber: context.ChannelUserId,
            customerName: context.Metadata?.GetValueOrDefault("CustomerName") ?? "Customer",
            channel: context.Channel.ToString()
        );

        await _conversationRepository.AddAsync(newConversation, cancellationToken);
        return newConversation;
    }

    private async Task<string> GenerateResponseAsync(
        IntentResult intent,
        MessageContext context,
        Result<MessageResponse> result,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrEmpty(intent.ResponseMessage))
            return intent.ResponseMessage;

        return intent.Intent switch
        {
            Intent.CreateOrder => "Great! Let me help you create that order. What would you like to buy? 🛒",
            Intent.SearchProduct => "Let me search for products for you! What are you looking for? 🔍",
            Intent.GetProductPrice => "Let me check the price for you. Which product are you interested in? 💰",
            Intent.TrackOrder => "I'll help you track your order. Please provide your order number. 🚚",
            Intent.GetHelp => "I can help you with orders, prices, products, and tracking. What would you like to do? 😊",
            _ => "How can I help you today? 😊"
        };
    }

    private InteractiveMessage? BuildInteractiveResponse(IntentResult intent, IActionResult result, string text)
    {
        if (intent.SuggestedResponses.Any())
        {
            return new InteractiveMessage
            {
                Text = text,
                QuickReplies = intent.SuggestedResponses.Take(5).ToList()
            };
        }

        return null;
    }
}
