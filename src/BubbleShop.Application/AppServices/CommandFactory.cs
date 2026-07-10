using BubbleShop.Application.Common.Interfaces;
using BubbleShop.Application.Common.Models;
using BubbleShop.Application.Common.Commands;
using BubbleShop.Application.Features.Orders.Commands.CreateOrder;
//using BubbleShop.Application.Features.Orders.Queries;
using BubbleShop.Application.Features.Products.Queries;
using BubbleShop.Application.Features.Products.Queries.SearchProducts;
using BubbleShop.Domain.Common;
using BubbleShop.Domain.Entities;
using BubbleShop.Domain.Exceptions;
using BubbleShop.Domain.Interfaces.Repositories;
using MediatR;
//using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using BubbleShop.Application.Features.Cart.Commands.AddToCart;
using BubbleShop.Application.Features.Cart.Commands.RemoveFromCart;
using BubbleShop.Application.Features.Orders.Commands.Checkout;
using BubbleShop.Application.Features.Cart.Queries.GetCart;


namespace BubbleShop.Application.AppServices;

public class CommandFactory : ICommandFactory
{
  
    private readonly IProductRepository _productRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly ILogger<CommandFactory> _logger;

    public CommandFactory(
        IMediator mediator,
        IProductRepository productRepository,
        ICustomerRepository customerRepository,
        ILogger<CommandFactory> logger)
    {
       
        _productRepository = productRepository;
        _customerRepository = customerRepository;
        _logger = logger;
    }

    private static Guid GetBusinessId(MessageContext context)
    {
        if (!Guid.TryParse(context.BusinessId, out var businessId))
            throw new DomainException("Invalid business id.");

        return businessId;
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
                Intent.RemoveFromCart =>
    await CreateRemoveFromCartCommand(intent, context, cancellationToken),
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
        if (!intent.Parameters.TryGetValue("ProductName", out var productNameValue))
            throw new DomainException("Product name is required");

        var productName = productNameValue?.ToString();

        if (string.IsNullOrWhiteSpace(productName))
            throw new DomainException("Product name is required.");

        var quantity = 1;

        if (intent.Parameters.TryGetValue("Quantity", out var quantityValue))
        {
            quantity = quantityValue switch
            {
                int i => i,
                long l => (int)l,
                string s when int.TryParse(s, out var q) => q,
                _ => 1
            };
        }
        var businessId = GetBusinessId(context);
        var product = await _productRepository.GetByNameAsync(
            productName,
            businessId,
            cancellationToken);
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

        intent.Parameters.TryGetValue("ShippingAddress", out var shipping);
        //ShippingAddress = shipping?.ToString();

        return new CreateOrderCommand(
            BusinessId: businessId,
            CustomerId: customerId,
            Items: items,
            CustomerName: customerName,
            CustomerWhatsApp: context.ChannelUserId,
            CustomerEmail: customer?.Email,
            CustomerPhone: customer?.PhoneNumber,
            ShippingAddress: shipping?.ToString(),
            Channel: context.Channel.ToString()
        );
    }

    private SearchProductsQuery CreateSearchProductCommand(IntentResult intent, MessageContext context)
    {
        var searchTerm =
            intent.Parameters.TryGetValue("SearchTerm", out var value)
                ? value?.ToString()
                : intent.RawMessage;

        searchTerm ??= "";
        if (!Guid.TryParse(context.BusinessId, out var businessId))
            throw new DomainException("Invalid business id.");
        return new SearchProductsQuery
        {
            Keyword = searchTerm,
            BusinessId = businessId,
            PageNumber = 1,
            PageSize = 10
        };
    }

    private GetProductPriceQuery CreateGetProductPriceCommand(IntentResult intent, MessageContext context)
    {
        var productName =
     intent.Parameters.TryGetValue("ProductName", out var value)
         ? value?.ToString()
         : intent.RawMessage;

        productName ??= string.Empty;
        var businessId = GetBusinessId(context);

        return new GetProductPriceQuery
        {
            ProductName = productName,
            BusinessId = businessId
        };
    }

    private CheckStockQuery CreateCheckStockCommand(IntentResult intent, MessageContext context)
    {
        var productName =
     intent.Parameters.TryGetValue("ProductName", out var value)
         ? value?.ToString()
         : intent.RawMessage;

        productName ??= string.Empty;
        var businessId = GetBusinessId(context);
        return new CheckStockQuery
        {
            ProductName = productName,
            BusinessId = businessId
        };
    }



    private async Task<GetCartQuery> CreateViewCartCommand(MessageContext context, CancellationToken cancellationToken)
    {
        var customerId = await GetOrCreateCustomerId(context, cancellationToken);
        var businessId = GetBusinessId(context);
        return new GetCartQuery
        {
            CustomerId = customerId,
            BusinessId = businessId
        };
    }

    private async Task<AddToCartCommand> CreateAddToCartCommand(
     IntentResult intent,
     MessageContext context,
     CancellationToken cancellationToken)
    {
        if (!intent.Parameters.ContainsKey("ProductId") &&
            !intent.Parameters.ContainsKey("ProductName"))
        {
            throw new DomainException("Product information is required to add to cart.");
        }

        var businessId = GetBusinessId(context);

        var customerId = await GetOrCreateCustomerId(context, cancellationToken);

        Guid productId;

        if (intent.Parameters.TryGetValue("ProductId", out var productIdValue))
        {
            if (!Guid.TryParse(productIdValue?.ToString(), out productId))
                throw new DomainException("Invalid product id.");
        }
        else
        {
            var productName = intent.Parameters["ProductName"]?.ToString();

            if (string.IsNullOrWhiteSpace(productName))
                throw new DomainException("Product name is required.");

            var product = await _productRepository.GetByNameAsync(
                productName,
                businessId,
                cancellationToken);

            if (product is null)
                throw new DomainException($"Product '{productName}' not found.");

            productId = product.Id;
        }

        var quantity = 1;

        if (intent.Parameters.TryGetValue("Quantity", out var quantityValue))
        {
            quantity = quantityValue switch
            {
                int i => i,
                long l => (int)l,
                string s when int.TryParse(s, out var q) => q,
                _ => 1
            };
            quantity = Math.Max(quantity, 1);
        }

        return new AddToCartCommand
        {
            CustomerId = customerId,
            ProductId = productId,
            Quantity = quantity
        };
    }
    private async Task<RemoveFromCartCommand> CreateRemoveFromCartCommand(
        IntentResult intent,
        MessageContext context,
        CancellationToken cancellationToken)
    {
        if (!intent.Parameters.TryGetValue("CartItemId", out var cartItemValue))
            throw new DomainException("Cart item id is required.");

        if (!Guid.TryParse(cartItemValue?.ToString(), out var cartItemId))
            throw new DomainException("Invalid cart item id.");
        var customerId = await GetOrCreateCustomerId(context, cancellationToken);

        return new RemoveFromCartCommand
        {
            CustomerId = customerId,
            CartItemId = cartItemId
        };
    }

    private async Task<CheckoutCommand> CreateCheckoutCommand(MessageContext context, CancellationToken cancellationToken)
    {
        var customerId = await GetOrCreateCustomerId(context, cancellationToken);
        var businessId = GetBusinessId(context);
        return new CheckoutCommand
        {
            CustomerId = customerId,
            BusinessId = businessId,
            Channel = context.Channel.ToString()
        };
    }

 
    private GetStoreHoursQuery CreateGetStoreHoursCommand(MessageContext context)
    {
        var businessId = GetBusinessId(context);
        return new GetStoreHoursQuery
        {
            BusinessId = businessId
        };
    }



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
        var businessId = GetBusinessId(context);
        var existingCustomer = await _customerRepository.GetByWhatsAppNumberAsync(
            context.ChannelUserId,
            businessId,
            cancellationToken);

        if (existingCustomer != null)
        {
            return existingCustomer.Id;
        }

        var customerName = context.Metadata?.GetValueOrDefault("CustomerName") ?? "Valued Customer";


        var newCustomer = new Customer(
            
            businessId: businessId,
            name: customerName,
            whatsappNumber: context.ChannelUserId,  
            phoneNumber: context.ChannelUserId,
            email: null
        );

        var addedCustomer = await _customerRepository.AddAsync(newCustomer, cancellationToken);
        return addedCustomer.Id;
    }
}