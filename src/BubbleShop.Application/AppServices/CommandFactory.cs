using BubbleShop.Application.Common.Interfaces;
using BubbleShop.Application.Common.Models;
using BubbleShop.Application.Common.Commands;
using BubbleShop.Application.Features.Orders.Commands.CreateOrder;
using BubbleShop.Application.Features.Orders.Queries;
using BubbleShop.Application.Features.Products.Queries;
using BubbleShop.Application.Features.Products.Queries.SearchProducts;
using BubbleShop.Domain.Common;
using BubbleShop.Domain.Entities;
using BubbleShop.Domain.Exceptions;
using BubbleShop.Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using BubbleShop.Application.Features.Cart.Commands.AddToCart;
using BubbleShop.Application.Features.Cart.Commands.RemoveFromCart;
using BubbleShop.Application.Features.Orders.Commands.Checkout;
using BubbleShop.Application.Features.Cart.Queries.GetCart;


namespace BubbleShop.Application.AppServices;

public class CommandFactory : ICommandFactory
{
    private readonly IMediator _mediator;
    private readonly IProductRepository _productRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly ILogger<CommandFactory> _logger;

    public CommandFactory(
        IMediator mediator,
        IProductRepository productRepository,
        ICustomerRepository customerRepository,
        ILogger<CommandFactory> logger)
    {
        _mediator = mediator;
        _productRepository = productRepository;
        _customerRepository = customerRepository;
        _logger = logger;
    }

    public async Task<IBaseRequest> CreateCommandAsync(
        IntentResult intent,
        MessageContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Creating command for intent: {Intent} from channel: {Channel}",
                intent.Intent, context.Channel);

            return intent.Intent switch
            {
                Intent.CreateOrder => await CreateOrderCommand(intent, context, cancellationToken),
                Intent.SearchProduct => CreateSearchProductCommand(intent, context),
                Intent.GetProductPrice => CreateGetProductPriceCommand(intent, context),
                Intent.CheckStock => CreateCheckStockCommand(intent, context),
                Intent.ViewCart => await CreateViewCartCommand(context, cancellationToken),
                Intent.AddToCart => await CreateAddToCartCommand(intent, context, cancellationToken),
                Intent.RemoveFromCart => CreateRemoveFromCartCommand(intent, context),
                Intent.Checkout => await CreateCheckoutCommand(context, cancellationToken),
                Intent.GetStoreHours => CreateGetStoreHoursCommand(context),
                _ => CreateUnknownIntentCommand(intent)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating command for intent: {Intent}", intent.Intent);
            return CreateUnknownIntentCommand(intent);
        }
    }

    private GetStoreHoursQuery CreateContactSupportCommand(IntentResult intent, MessageContext context)
    {
        throw new NotImplementedException();
    }

    private async Task<CreateOrderCommand> CreateOrderCommand(IntentResult intent, MessageContext context, CancellationToken cancellationToken)
    {
        if (!intent.Parameters.ContainsKey("ProductName"))
            throw new DomainException("Product name is required for order creation");

        var productName = intent.Parameters["ProductName"].ToString();
        var quantity = intent.Parameters.ContainsKey("Quantity") ? (int)intent.Parameters["Quantity"] : 1;

        var product = await _productRepository.GetByNameAsync(productName, Guid.Parse(context.BusinessId), cancellationToken);

        if (product == null)
        {
            throw new DomainException($"Sorry, we don't have '{productName}' in our store.");
        }

        var customerId = await GetOrCreateCustomerId(context, cancellationToken);
        var customer = await _customerRepository.GetByIdAsync(customerId, cancellationToken);

        var customerName = customer?.Name ?? intent.Parameters.GetValueOrDefault("CustomerName")?.ToString() ?? "Valued Customer";

        var items = new List<OrderItemInput>
        {
            new OrderItemInput(product.Id, quantity)
        };

        return new CreateOrderCommand(
            BusinessId: Guid.Parse(context.BusinessId),
            CustomerId: customerId,
            Items: items,
            CustomerName: customerName,
            CustomerWhatsApp: context.ChannelUserId,
            CustomerEmail: customer?.Email,
            CustomerPhone: customer?.PhoneNumber,
            ShippingAddress: intent.Parameters.ContainsKey("ShippingAddress")
                ? intent.Parameters["ShippingAddress"].ToString()
                : null,
            Channel: context.Channel.ToString()
        );
    }

    private SearchProductsQuery CreateSearchProductCommand(IntentResult intent, MessageContext context)
    {
        var searchTerm = intent.Parameters.ContainsKey("SearchTerm")
            ? intent.Parameters["SearchTerm"].ToString()
            : intent.RawMessage;

        return new SearchProductsQuery
        {
            Keyword = searchTerm,
            BusinessId = Guid.Parse(context.BusinessId),
            PageNumber = 1,
            PageSize = 10
        };
    }

    private GetProductPriceQuery CreateGetProductPriceCommand(IntentResult intent, MessageContext context)
    {
        var productName = intent.Parameters.ContainsKey("ProductName")
            ? intent.Parameters["ProductName"].ToString()
            : intent.RawMessage;

        return new GetProductPriceQuery
        {
            ProductName = productName,
            BusinessId = Guid.Parse(context.BusinessId)
        };
    }

    private CheckStockQuery CreateCheckStockCommand(IntentResult intent, MessageContext context)
    {
        var productName = intent.Parameters.ContainsKey("ProductName")
            ? intent.Parameters["ProductName"].ToString()
            : intent.RawMessage;

        return new CheckStockQuery
        {
            ProductName = productName,
            BusinessId = Guid.Parse(context.BusinessId)
        };
    }

    //private TrackOrderQuery CreateTrackOrderCommand(IntentResult intent, MessageContext context)
    //{
    //    var orderNumber = intent.Parameters.ContainsKey("OrderNumber")
    //        ? intent.Parameters["OrderNumber"].ToString()
    //        : null;

    //    return new TrackOrderQuery
    //    {
    //        OrderNumber = orderNumber,
    //        CustomerWhatsApp = context.ChannelUserId,
    //        CustomerId = null
    //    };
    //}

    private async Task<GetCartQuery> CreateViewCartCommand(MessageContext context, CancellationToken cancellationToken)
    {
        var customerId = await GetOrCreateCustomerId(context, cancellationToken);
        return new GetCartQuery
        {
            CustomerId = customerId,
            BusinessId = Guid.Parse(context.BusinessId)
        };
    }

    private async Task<AddToCartCommand> CreateAddToCartCommand(IntentResult intent, MessageContext context, CancellationToken cancellationToken)
    {
        if (!intent.Parameters.ContainsKey("ProductId") && !intent.Parameters.ContainsKey("ProductName"))
            throw new DomainException("Product information is required to add to cart");

        var customerId = await GetOrCreateCustomerId(context, cancellationToken);
        var productId = Guid.Empty;

        if (intent.Parameters.ContainsKey("ProductId"))
        {
            productId = Guid.Parse(intent.Parameters["ProductId"].ToString());
        }
        else if (intent.Parameters.ContainsKey("ProductName"))
        {
            var productName = intent.Parameters["ProductName"].ToString();
            var product = await _productRepository.GetByNameAsync(productName, Guid.Parse(context.BusinessId), cancellationToken);
            if (product == null)
                throw new DomainException($"Product '{productName}' not found");
            productId = product.Id;
        }

        var quantity = intent.Parameters.ContainsKey("Quantity") ? (int)intent.Parameters["Quantity"] : 1;

        return new AddToCartCommand
        {
            CustomerId = customerId,
            ProductId = productId,
            Quantity = quantity
        };
    }

    private RemoveFromCartCommand CreateRemoveFromCartCommand(IntentResult intent, MessageContext context)
    {
        if (!intent.Parameters.ContainsKey("CartItemId"))
            throw new DomainException("Cart item ID is required to remove from cart");

        return new RemoveFromCartCommand
        {
            CustomerId = Guid.NewGuid(),
            CartItemId = Guid.Parse(intent.Parameters["CartItemId"].ToString())
        };
    }

    private async Task<CheckoutCommand> CreateCheckoutCommand(MessageContext context, CancellationToken cancellationToken)
    {
        var customerId = await GetOrCreateCustomerId(context, cancellationToken);
        return new CheckoutCommand
        {
            CustomerId = customerId,
            BusinessId = Guid.Parse(context.BusinessId),
            Channel = context.Channel.ToString()
        };
    }

    //private ApplyCouponCommand CreateApplyCouponCommand(IntentResult intent, MessageContext context)
    //{
    //    if (!intent.Parameters.ContainsKey("CouponCode"))
    //        throw new DomainException("Coupon code is required");

    //    return new ApplyCouponCommand
    //    {
    //        CustomerId = Guid.NewGuid(),
    //        CouponCode = intent.Parameters["CouponCode"].ToString()
    //    };
    //}

    private GetStoreHoursQuery CreateGetStoreHoursCommand(MessageContext context)
    {
        return new GetStoreHoursQuery
        {
            BusinessId = Guid.Parse(context.BusinessId)
        };
    }

    //private CreateSupportTicketCommand CreateContactSupportCommand(IntentResult intent, MessageContext context)
    //{
    //    return new CreateSupportTicketCommand
    //    {
    //        CustomerId = null,
    //        CustomerWhatsApp = context.ChannelUserId,
    //        CustomerName = intent.Parameters.ContainsKey("CustomerName") ? intent.Parameters["CustomerName"].ToString() : null,
    //        Message = intent.RawMessage,
    //        Channel = context.Channel.ToString(),
    //        Priority = "Normal"
    //    };
    //}

    //private GetHelpQuery CreateGetHelpCommand(IntentResult intent, MessageContext context)
    //{
    //    return new GetHelpQuery
    //    {
    //        BusinessId = Guid.Parse(context.BusinessId),
    //        CustomerId = null,
    //        Topic = intent.Parameters.ContainsKey("Topic") ? intent.Parameters["Topic"].ToString() : "General"
    //    };
    //}

    //private JustChattingCommand CreateChattingCommand(IntentResult intent, MessageContext context)
    //{
    //    return new JustChattingCommand
    //    {
    //        Message = intent.RawMessage,
    //        Response = intent.ResponseMessage
    //    };
    //}

    private UnknownIntentCommand CreateUnknownIntentCommand(IntentResult intent)
    {
        return new UnknownIntentCommand
        {
            Message = intent.RawMessage,
            SuggestedResponse = "I'm not sure how to help with that. You can ask me about:\n" +
                               "• Placing orders (e.g., 'I want to buy rice')\n" +
                               "• Checking prices (e.g., 'How much is rice?')\n" +
                               "• Finding products (e.g., 'Show me rice')\n" +
                               "• Tracking orders (e.g., 'Where is my order?')\n\n" +
                               "How can I assist you today?"
        };
    }

    private async Task<Guid> GetOrCreateCustomerId(MessageContext context, CancellationToken cancellationToken)
    {
        if (context.CustomerId.HasValue && context.CustomerId.Value != Guid.Empty)
        {
            return context.CustomerId.Value;
        }

        var existingCustomer = await _customerRepository.GetByWhatsAppNumberAsync(
            context.ChannelUserId,
            Guid.Parse(context.BusinessId),
            cancellationToken);

        if (existingCustomer != null)
        {
            return existingCustomer.Id;
        }

        var customerName = context.Metadata?.GetValueOrDefault("CustomerName") ?? "Valued Customer";


        var newCustomer = new Customer(
            
            businessId: Guid.Parse(context.BusinessId),
            name: customerName,
            whatsappNumber: context.ChannelUserId,  
            phoneNumber: context.ChannelUserId,
            email: null
        );

        var addedCustomer = await _customerRepository.AddAsync(newCustomer, cancellationToken);
        return addedCustomer.Id;
    }
}