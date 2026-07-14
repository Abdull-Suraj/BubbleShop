using BubbleShop.Application.Common.Interfaces;
using BubbleShop.Application.Common.Models;
using BubbleShop.Application.Features.Messages.Commands.ProcessCustomerMessage;
using BubbleShop.Application.Features.Orders.Commands.CreateOrder;
using BubbleShop.Application.Features.Products.Commands.GetProductPrice;
using BubbleShop.Application.Features.Products.Commands.SearchProduct;
using BubbleShop.Application.Features.Orders.Commands.TrackOrder;
using BubbleShop.Application.Features.Orders.Commands.CancelOrder;
using BubbleShop.Application.Features.Orders.Commands.Checkout;
using BubbleShop.Application.Features.Customers.Commands.CreateOrUpdateCustomer;
using BubbleShop.Application.Features.Cart.Commands.AddToCart;
using BubbleShop.Application.Features.Cart.Commands.RemoveFromCart;
using BubbleShop.Application.Features.Cart.Queries.GetCart;
using BubbleShop.Application.Features.Store.Queries.GetStoreHours;
using BubbleShop.Application.Features.Help.Queries.GetHelp;
using BubbleShop.Application.Features.Feedback.Commands.ProvideFeedback;
using BubbleShop.Domain.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BubbleShop.Application.AppServices;

public class CommandFactory : ICommandFactory
{
    private readonly ILogger<CommandFactory> _logger;

    public CommandFactory(ILogger<CommandFactory> logger)
    {
        _logger = logger;
    }

    public Task<IRequest<Result<MessageResponse>>> CreateCommandAsync(
        IntentResult intent,
        MessageContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Creating command for intent: {Intent}", intent.Intent);

            IRequest<Result<MessageResponse>> command = intent.Intent switch
            {
                // Order Commands
                Intent.CreateOrder => CreateOrderCommand(intent, context),

                // Product Commands
                Intent.SearchProduct => CreateSearchProductCommand(intent, context),
                Intent.GetProductPrice => CreateGetProductPriceCommand(intent, context),
                Intent.CheckStock => CreateCheckStockCommand(intent, context),

                // Order Management Commands
                Intent.TrackOrder => CreateTrackOrderCommand(intent, context),
                Intent.CancelOrder => CreateCancelOrderCommand(intent, context),
                Intent.Checkout => CreateCheckoutCommand(intent, context),

                // Cart Commands
                Intent.ViewCart => CreateViewCartCommand(intent, context),
                Intent.AddToCart => CreateAddToCartCommand(intent, context),
                Intent.RemoveFromCart => CreateRemoveFromCartCommand(intent, context),

                // Customer Commands
                Intent.ContactSupport => CreateContactSupportCommand(intent, context),

                // Store Commands
                Intent.GetStoreHours => CreateGetStoreHoursCommand(intent, context),

                // Help Commands
                Intent.GetHelp => CreateGetHelpCommand(intent, context),

                // Feedback Commands
                Intent.ProvideFeedback => CreateProvideFeedbackCommand(intent, context),

                // Social/Casual
                Intent.JustChatting => CreateJustChattingCommand(intent, context),

                // Default - Unknown
                _ => CreateUnknownCommand(intent, context)
            };

            return Task.FromResult(command);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed creating command for intent {Intent}", intent.Intent);
            throw;
        }
    }

    #region Order Commands

    private IRequest<Result<MessageResponse>> CreateOrderCommand(IntentResult intent, MessageContext context)
    {
        var productName = intent.Parameters.GetValueOrDefault("ProductName")?.ToString() ?? string.Empty;
        var quantity = intent.Parameters.GetValueOrDefault("Quantity") is int qty ? qty : 1;
        var customerName = GetCustomerName(context);

        return new CreateOrderCommand(
            Channel: context.Channel.ToString(),
            CustomerId: context.ChannelUserId,
            CustomerName: customerName,
            CustomerWhatsApp: context.ChannelUserId,
            BusinessId: Guid.Parse(context.BusinessId),
            ProductName: productName,
            Quantity: quantity,
            Message: intent.RawMessage
        );
    }

    private IRequest<Result<MessageResponse>> CreateTrackOrderCommand(IntentResult intent, MessageContext context)
    {
        var orderNumber = intent.Parameters.GetValueOrDefault("OrderNumber")?.ToString() ?? string.Empty;

        return new TrackOrderCommand(
            Channel: context.Channel.ToString(),
            CustomerId: context.ChannelUserId,
            BusinessId: Guid.Parse(context.BusinessId),
            OrderNumber: orderNumber,
            Message: intent.RawMessage
        );
    }

    private IRequest<Result<MessageResponse>> CreateCancelOrderCommand(IntentResult intent, MessageContext context)
    {
        var orderNumber = intent.Parameters.GetValueOrDefault("OrderNumber")?.ToString() ?? string.Empty;
        var reason = intent.Parameters.GetValueOrDefault("Reason")?.ToString() ?? "Customer requested cancellation";

        return new CancelOrderCommand(
            Channel: context.Channel.ToString(),
            CustomerId: context.ChannelUserId,
            BusinessId: Guid.Parse(context.BusinessId),
            OrderNumber: orderNumber,
            Reason: reason,
            Message: intent.RawMessage
        );
    }

    private IRequest<Result<MessageResponse>> CreateCheckoutCommand(IntentResult intent, MessageContext context)
    {
        return new CheckoutCommand(
            Channel: context.Channel.ToString(),
            CustomerId: context.ChannelUserId,
            BusinessId: Guid.Parse(context.BusinessId),
            Message: intent.RawMessage
        );
    }

    #endregion

    #region Product Commands

    private IRequest<Result<MessageResponse>> CreateSearchProductCommand(IntentResult intent, MessageContext context)
    {
        var searchTerm = intent.Parameters.GetValueOrDefault("SearchTerm")?.ToString()
                         ?? intent.Parameters.GetValueOrDefault("ProductName")?.ToString()
                         ?? intent.RawMessage;

        return new SearchProductCommand(
            Channel: context.Channel.ToString(),
            CustomerId: context.ChannelUserId,
            BusinessId: Guid.Parse(context.BusinessId),
            SearchTerm: searchTerm,
            Message: intent.RawMessage
        );
    }

    private IRequest<Result<MessageResponse>> CreateGetProductPriceCommand(IntentResult intent, MessageContext context)
    {
        var productName = intent.Parameters.GetValueOrDefault("ProductName")?.ToString() ?? intent.RawMessage;

        return new GetProductPriceCommand(
            Channel: context.Channel.ToString(),
            CustomerId: context.ChannelUserId,
            BusinessId: Guid.Parse(context.BusinessId),
            ProductName: productName,
            Message: intent.RawMessage
        );
    }

    private IRequest<Result<MessageResponse>> CreateCheckStockCommand(IntentResult intent, MessageContext context)
    {
        var productName = intent.Parameters.GetValueOrDefault("ProductName")?.ToString() ?? intent.RawMessage;

        return new CheckStockCommand(
            Channel: context.Channel.ToString(),
            CustomerId: context.ChannelUserId,
            BusinessId: Guid.Parse(context.BusinessId),
            ProductName: productName,
            Message: intent.RawMessage
        );
    }

    #endregion

    #region Cart Commands

    private IRequest<Result<MessageResponse>> CreateViewCartCommand(IntentResult intent, MessageContext context)
    {
        return new GetCartQuery(
            Channel: context.Channel.ToString(),
            CustomerId: context.ChannelUserId,
            BusinessId: Guid.Parse(context.BusinessId),
            Message: intent.RawMessage
        );
    }

    private IRequest<Result<MessageResponse>> CreateAddToCartCommand(IntentResult intent, MessageContext context)
    {
        var productName = intent.Parameters.GetValueOrDefault("ProductName")?.ToString() ?? string.Empty;
        var quantity = intent.Parameters.GetValueOrDefault("Quantity") is int qty ? qty : 1;

        return new AddToCartCommand(
            Channel: context.Channel.ToString(),
            CustomerId: context.ChannelUserId,
            BusinessId: Guid.Parse(context.BusinessId),
            ProductName: productName,
            Quantity: quantity,
            Message: intent.RawMessage
        );
    }

    private IRequest<Result<MessageResponse>> CreateRemoveFromCartCommand(IntentResult intent, MessageContext context)
    {
        var productName = intent.Parameters.GetValueOrDefault("ProductName")?.ToString() ?? string.Empty;

        return new RemoveFromCartCommand(
            Channel: context.Channel.ToString(),
            CustomerId: context.ChannelUserId,
            BusinessId: Guid.Parse(context.BusinessId),
            ProductName: productName,
            Message: intent.RawMessage
        );
    }

    #endregion

    #region Store Commands

    private IRequest<Result<MessageResponse>> CreateGetStoreHoursCommand(IntentResult intent, MessageContext context)
    {
        return new GetStoreHoursQuery(
            Channel: context.Channel.ToString(),
            CustomerId: context.ChannelUserId,
            BusinessId: Guid.Parse(context.BusinessId),
            Message: intent.RawMessage
        );
    }

    #endregion

    #region Customer Commands

    private IRequest<Result<MessageResponse>> CreateContactSupportCommand(IntentResult intent, MessageContext context)
    {
        return new ContactSupportCommand(
            Channel: context.Channel.ToString(),
            CustomerId: context.ChannelUserId,
            CustomerName: GetCustomerName(context),
            BusinessId: Guid.Parse(context.BusinessId),
            Message: intent.RawMessage
        );
    }

    #endregion

    #region Help Commands

    private IRequest<Result<MessageResponse>> CreateGetHelpCommand(IntentResult intent, MessageContext context)
    {
        var topic = intent.Parameters.GetValueOrDefault("Topic")?.ToString() ?? "general";

        return new GetHelpQuery(
            Channel: context.Channel.ToString(),
            CustomerId: context.ChannelUserId,
            BusinessId: Guid.Parse(context.BusinessId),
            Topic: topic,
            Message: intent.RawMessage
        );
    }

    #endregion

    #region Feedback Commands

    private IRequest<Result<MessageResponse>> CreateProvideFeedbackCommand(IntentResult intent, MessageContext context)
    {
        var rating = intent.Parameters.GetValueOrDefault("Rating") is int r ? r : 0;
        var feedback = intent.Parameters.GetValueOrDefault("Feedback")?.ToString() ?? intent.RawMessage;

        return new ProvideFeedbackCommand(
            Channel: context.Channel.ToString(),
            CustomerId: context.ChannelUserId,
            BusinessId: Guid.Parse(context.BusinessId),
            Rating: rating,
            Feedback: feedback,
            Message: intent.RawMessage
        );
    }

    #endregion

    #region Social/Casual Commands

    private IRequest<Result<MessageResponse>> CreateJustChattingCommand(IntentResult intent, MessageContext context)
    {
        return new JustChattingCommand(
            Channel: context.Channel.ToString(),
            CustomerId: context.ChannelUserId,
            BusinessId: Guid.Parse(context.BusinessId),
            Message: intent.RawMessage,
            Response: intent.ResponseMessage ?? "Hello! How can I help you today? 😊"
        );
    }

    #endregion

    #region Unknown Command

    private IRequest<Result<MessageResponse>> CreateUnknownCommand(IntentResult intent, MessageContext context)
    {
        return new UnknownCommand(
            Channel: context.Channel.ToString(),
            CustomerId: context.ChannelUserId,
            BusinessId: Guid.Parse(context.BusinessId),
            Message: intent.RawMessage,
            SuggestedResponse: "I'm not sure how to help with that. You can ask me about:\n" +
                              "• Placing orders (e.g., 'I want to buy rice')\n" +
                              "• Checking prices (e.g., 'How much is rice?')\n" +
                              "• Finding products (e.g., 'Show me rice')\n" +
                              "• Tracking orders (e.g., 'Where is my order?')\n\n" +
                              "How can I assist you today?"
        );
    }

    #endregion

    #region Helper Methods

    private static string GetCustomerName(MessageContext context)
    {
        if (context.Metadata is not null &&
            context.Metadata.TryGetValue("CustomerName", out var name))
        {
            return name;
        }

        return "Customer";
    }

    private static string GetProductNameFromIntent(IntentResult intent)
    {
        if (intent.Parameters.TryGetValue("ProductName", out var productName))
        {
            return productName?.ToString() ?? string.Empty;
        }

        // Try to extract from raw message
        var words = intent.RawMessage.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        foreach (var word in words)
        {
            if (word.Length > 2 && !IsCommonWord(word))
            {
                return word;
            }
        }

        return string.Empty;
    }

    private static bool IsCommonWord(string word)
    {
        var commonWords = new[] { "want", "buy", "get", "show", "find", "search", "how", "much", "what", "where", "when", "why" };
        return commonWords.Contains(word.ToLower());
    }

    #endregion
}