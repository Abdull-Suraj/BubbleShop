using BubbleShop.Application.Common.Interfaces;
using BubbleShop.Application.Common.Models;
using BubbleShop.Application.DTOs;
using BubbleShop.Application.Features.Orders.Queries.GetOrderById;
using BubbleShop.Domain.Common;
using BubbleShop.Domain.Entities;
using BubbleShop.Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Text;


namespace BubbleShop.Application.AppServices
{


    public class MessageRouter : IMessageRouter
    {
        private readonly IAIIntentService _aiIntentService;
        private readonly ICommandFactory _commandFactory;
        private readonly IMediator _mediator;
        private readonly ICustomerRepository _customerRepository;
        private readonly IBusinessRepository _businessRepository;
        private readonly IConversationRepository _conversationRepository;
        private readonly ILogger<MessageRouter> _logger;
        private readonly IUnitOfWork _unitOfWork;


        public MessageRouter(
            IAIIntentService aiIntentService,
            ICommandFactory commandFactory,
            IMediator mediator,
            ICustomerRepository customerRepository,
            IBusinessRepository businessRepository,
            IConversationRepository conversationRepository,
            IUnitOfWork unitOfWork,
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

        /// <summary>
        /// Process an incoming message from any channel (WhatsApp, Telegram, WebChat, etc.)
        /// </summary>
        /// <param name="message">The customer's message</param>
        /// <param name="context">Message context containing channel, user, and business information</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The response message to send back to the customer</returns>
        public async Task<string> ProcessIncomingMessageAsync(
            string message,
            MessageContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Processing message from {Channel} user {UserId}: {Message}",
                    context.Channel, context.ChannelUserId, message);

                // Step 1: Get or create customer
                var customer = await GetOrCreateCustomerAsync(context, cancellationToken);
                context.CustomerId = customer.Id;

                // Step 2: Get or create conversation
                var conversation = await GetOrCreateConversationAsync(context, customer.Id, cancellationToken);
                context.ConversationId = conversation.Id.ToString();

                // Step 3: Save customer message to conversation
                conversation.AddCustomerMessage(message);
                await _conversationRepository.UpdateAsync(conversation, cancellationToken);

                // Step 4: Analyze intent using AI service
                var intentResult = await _aiIntentService.AnalyzeIntentAsync(message, context, cancellationToken);

                var command = await _commandFactory.CreateCommandAsync(intentResult, context, cancellationToken);

                object result = await _mediator.Send(command, cancellationToken);

                // Step 7: Generate natural language response
                var response = await GenerateResponseAsync(intentResult, context, result, cancellationToken);

                // Step 8: Save assistant response to conversation
                conversation.AddAssistantMessage(response);
                await _conversationRepository.UpdateAsync(conversation, cancellationToken);

                _logger.LogInformation("Response sent to {Channel} user {UserId}: {Response}",
                    context.Channel, context.ChannelUserId, response);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message from {Channel} user {UserId}",
                    context.Channel, context.ChannelUserId);

                return "I'm sorry, I'm having trouble processing your request right now. Please try again later or contact our support team. 🙏";
            }
        }
        private string BuildOrderResponse(object result)
        {
            if (result is Result<OrderDto> orderResult &&
                orderResult.IsSuccess)
            {
                var order = orderResult.Value;

                return
                    $"✅ Order Created Successfully!\n\n" +
                    $"📦 Order ID: {order.Id}\n" +
                    $"💰 Total: ₦{order.TotalAmount:N2}\n" +
                    $"📍 Status: {order.Status}";
            }

            return "❌ Unable to create order.";
        }

        private string BuildSearchResponse(object result)
        {
            if (result is Result<PagedResult<ProductDto>> searchResult &&
                searchResult.IsSuccess)
            {
                var products = searchResult.Value.Items.ToList();

                if (!products.Any())
                    return "No products found.";

                var response = "📋 Products Found\n\n";

                foreach (var product in products.Take(5))
                {
                    response +=
                        $"• {product.Name}\n" +
                        $"Price: ₦{product.Price:N2}\n" +
                        $"Stock: {product.StockQuantity}\n\n";
                }

                return response;
            }

            return "No products found.";
        }

        private string BuildPriceResponse(object result)
        {
            if (result is Result<ProductDto> productResult &&
                productResult.IsSuccess)
            {
                var product = productResult.Value;

                return
                    $"💰 {product.Name}\n\n" +
                    $"Price: ₦{product.Price:N2}\n" +
                    $"Available Stock: {product.StockQuantity}";
            }

            return "Product not found.";
        }

        private string BuildStockResponse(object result)
        {
            if (result is Result<ProductDto> productResult &&
                productResult.IsSuccess)
            {
                var product = productResult.Value;

                var status =
                    product.StockQuantity > 10 ? "✅ In Stock" :
                    product.StockQuantity > 0 ? "⚠️ Low Stock" :
                    "❌ Out of Stock";

                return
                    $"📦 {product.Name}\n" +
                    $"Status: {status}\n" +
                    $"Quantity Available: {product.StockQuantity}";
            }

            return "Unable to check stock.";
        }

        private string BuildTrackOrderResponse(object result)
        {
            if (result is Result<OrderDto> orderResult &&
                orderResult.IsSuccess)
            {
                var order = orderResult.Value;

                return
                    $"🚚 Order Status\n\n" +
                    $"Order: {order.Id}\n" +
                    $"Status: {order.Status}\n" +
                    $"Created: {order.CreatedAt:g}";
            }

            return "Order not found.";
        }
        private string BuildCartResponse(object result)
        {
            if (result is OkObjectResult ok)
            {
                dynamic data = ok.Value!;

                var productName = data.ProductName ?? "Product";
                var quantity = data.Quantity ?? 1;
                var totalItems = data.TotalItems ?? quantity;
                var total = data.Total ?? data.TotalPrice ?? 0m;

                return
        $"""
                🛒 *Item Added to Cart*

                ✅ {quantity} × {productName} added successfully.

                🛍 Total Items: {totalItems}
                💰 Cart Total: ₦{total:N2}

                You can now:
                • View Cart
                • Continue Shopping
                • Checkout
                """;
                            }

                            if (result is BadRequestObjectResult bad)
                            {
                                dynamic error = bad.Value!;

                                return
                        $"""
                ❌ Unable to add item to your cart.

                {error?.message ?? error?.error ?? "Please try again."}
                """;
                            }

                            if (result is NotFoundObjectResult)
                            {
                                return
                        """
                ❌ Product not found.

                Please check the product name and try again.
                """;
                            }

                            return
                        """
                ✅ Item added to your cart successfully.
                """;
        }
        private async Task<string> GenerateResponseAsync(
            IntentResult intent,
            MessageContext context,
            object result,
            CancellationToken cancellationToken)
        {
            if (!string.IsNullOrWhiteSpace(intent.ResponseMessage))
                return intent.ResponseMessage;

            return intent.Intent switch
            {
                Intent.CreateOrder => BuildOrderResponse(result),
                Intent.SearchProduct => BuildSearchResponse(result),
                Intent.GetProductPrice => BuildPriceResponse(result),
                Intent.CheckStock => BuildStockResponse(result),
                Intent.TrackOrder => BuildTrackOrderResponse(result),
                Intent.ViewCart => BuildCartResponse(result),
                Intent.GetHelp => GetHelpResponse(),
                Intent.JustChatting => GetChattingResponse(intent),
                _ => GetDefaultResponse()
            };
        }

        #region Response Handlers


        private string GetHelpResponse()
        {
            return "🤖 **How can I help you?**\n\n" +
                   "Here's what I can do:\n\n" +
                   "🛒 **Place an order** - Say `I want to buy [product]`\n" +
                   "💰 **Check prices** - Ask `How much is [product]?`\n" +
                   "🔍 **Search products** - Say `Show me [product]`\n" +
                   "📊 **Check stock** - Ask `Do you have [product]?`\n" +
                   "🚚 **Track order** - Say `Track my order`\n\n" +
                   "What would you like to do today? 😊";
        }

        private string GetChattingResponse(IntentResult intent)
        {
            var responses = new[]
            {
            "Hello! 👋 How can I make your day better today?",
            "Hi there! 😊 Welcome to Bubble Shop! What can I help you with?",
            "Hey! 🎉 Great to see you! Looking for something special today?",
            "Hello! 🌟 How are you doing today? Need any help with shopping?"
        };
            return responses[new Random().Next(responses.Length)];
        }

        private string GetDefaultResponse()
        {
            return "I'm here to help! 😊 You can ask me about:\n" +
                   "• Placing orders\n" +
                   "• Checking prices\n" +
                   "• Finding products\n" +
                   "• Tracking orders\n\n" +
                   "What would you like to do?";
        }

        #endregion

        #region Helper Methods

        private async Task<Customer> GetOrCreateCustomerAsync(MessageContext context, CancellationToken cancellationToken)
        {
            
            var customer = await _customerRepository.GetByWhatsAppNumberAsync(
                context.ChannelUserId,
                Guid.Parse(context.BusinessId),
                cancellationToken);
            
            if (customer != null)
                return customer;

        
            var customerName = context.Metadata?.GetValueOrDefault("CustomerName") ?? "Valued Customer";
            var newCustomer = new Customer(
                whatsappNumber: context.ChannelUserId,
                name: customerName,
                email: null,
                phoneNumber: context.ChannelUserId,
                businessId: Guid.Parse(context.BusinessId)
            );

            await _customerRepository.AddAsync(newCustomer, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return newCustomer;
        }

        private async Task<Conversation> GetOrCreateConversationAsync(MessageContext context, Guid customerId, CancellationToken cancellationToken)
        {
            // Try to get existing conversation
            if (!string.IsNullOrEmpty(context.ConversationId))
            {
                var existing = await _conversationRepository.GetByIdAsync(Guid.Parse(context.ConversationId), cancellationToken);
                if (existing != null)
                    return existing;
            }

            var conversation = new Conversation(
                businessId: Guid.Parse(context.BusinessId),
                customerId: customerId,
                whatsAppNumber: context.ChannelUserId,
                customerName: context.Metadata?.GetValueOrDefault("CustomerName") ?? "Customer"
            );

            await _conversationRepository.AddAsync(conversation, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return conversation;
        }

        #endregion
    }
}
