using BubbleShop.Application.Common.Interfaces;
using BubbleShop.Application.Common.Models;
using BubbleShop.Application.Common.Commands;
using BubbleShop.Application.Features.Cart.Commands.AddToCart;
using BubbleShop.Application.Features.Cart.Commands.RemoveFromCart;
using BubbleShop.Application.Features.Cart.Queries.GetCart;
using BubbleShop.Application.Features.Orders.Commands.Checkout;
using BubbleShop.Application.Features.Orders.Commands.CreateOrder;
using BubbleShop.Application.Features.Products.Queries;
using BubbleShop.Application.Features.Products.Queries.SearchProducts;
using BubbleShop.Domain.Common;
using BubbleShop.Domain.Entities;
using BubbleShop.Domain.Exceptions;
using BubbleShop.Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BubbleShop.Application.AppServices;

public sealed class CommandFactory : ICommandFactory
{
    private readonly IProductRepository _productRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly ILogger<CommandFactory> _logger;

    public CommandFactory(
        IProductRepository productRepository,
        ICustomerRepository customerRepository,
        ILogger<CommandFactory> logger)
    {
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
            _logger.LogInformation(
                "Creating command for {Intent}",
                intent.Intent);

            return intent.Intent switch
            {
                Intent.CreateOrder =>
                    await CreateOrderCommand(intent, context, cancellationToken),

                Intent.SearchProduct =>
                    CreateSearchProductCommand(intent, context),

                Intent.GetProductPrice =>
                    CreateGetProductPriceCommand(intent, context),

                Intent.CheckStock =>
                    CreateCheckStockCommand(intent, context),

                Intent.ViewCart =>
                    await CreateViewCartCommand(context, cancellationToken),

                Intent.AddToCart =>
                    await CreateAddToCartCommand(intent, context, cancellationToken),

                Intent.RemoveFromCart =>
                    await CreateRemoveFromCartCommand(intent, context, cancellationToken),

                Intent.Checkout =>
                    await CreateCheckoutCommand(context, cancellationToken),

                Intent.GetStoreHours =>
                    CreateGetStoreHoursCommand(context),

                _ =>
                    CreateUnknownIntentCommand(intent)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed creating command");

            return CreateUnknownIntentCommand(intent);
        }
    }

    private static Guid GetBusinessId(MessageContext context)
    {
        if (!Guid.TryParse(context.BusinessId, out var id))
            throw new DomainException("Invalid business id.");

        return id;
    }

    private async Task<Guid> GetOrCreateCustomerId(
        MessageContext context,
        CancellationToken cancellationToken)
    {
        if (context.CustomerId.HasValue &&
            context.CustomerId.Value != Guid.Empty)
        {
            return context.CustomerId.Value;
        }

        var businessId = GetBusinessId(context);

        var existing = await _customerRepository.GetByWhatsAppNumberAsync(
            context.ChannelUserId,
            businessId,
            cancellationToken);

        if (existing != null)
            return existing.Id;

        var customer = new Customer(
            businessId: businessId,
            whatsappNumber: context.ChannelUserId,
            name: context.Metadata.GetValueOrDefault("CustomerName") ?? "Customer",
            email: null,
            phoneNumber: context.ChannelUserId
            );

        customer = await _customerRepository.AddAsync(
            customer,
            cancellationToken);

        return customer.Id;
    }

    // -----------------------------
    // CREATE ORDER
    // -----------------------------

    private async Task<CreateOrderCommand> CreateOrderCommand(
        IntentResult intent,
        MessageContext context,
        CancellationToken cancellationToken)
    {
        var businessId = GetBusinessId(context);

        var customerId =
            await GetOrCreateCustomerId(
                context,
                cancellationToken);

        var productName =
            intent.Parameters["ProductName"].ToString()!;

        var quantity =
            intent.Parameters.TryGetValue("Quantity", out var qty)
                ? Convert.ToInt32(qty)
                : 1;

        var product =
            await _productRepository.GetByNameAsync(
                productName,
                businessId,
                cancellationToken);

        if (product == null)
            throw new DomainException($"'{productName}' not found.");

        return new CreateOrderCommand(
            businessId,
            customerId,
            new()
            {
                new OrderItemInput(product.Id, quantity)
            },
            "Customer",
            context.ChannelUserId,
            null,
            context.ChannelUserId,
            null,
            context.Channel.ToString());
    }

    private SearchProductsQuery CreateSearchProductCommand(
        IntentResult intent,
        MessageContext context)
    {
        return new SearchProductsQuery
        {
            BusinessId = GetBusinessId(context),
            Keyword = intent.Parameters.GetValueOrDefault("SearchTerm")?.ToString() ?? "",
            PageNumber = 1,
            PageSize = 10
        };
    }

    private GetProductPriceQuery CreateGetProductPriceCommand(
        IntentResult intent,
        MessageContext context)
    {
        return new GetProductPriceQuery
        {
            BusinessId = GetBusinessId(context),
            ProductName =
                intent.Parameters.GetValueOrDefault("ProductName")?.ToString() ?? ""
        };
    }

    private CheckStockQuery CreateCheckStockCommand(
        IntentResult intent,
        MessageContext context)
    {
        return new CheckStockQuery
        {
            BusinessId = GetBusinessId(context),
            ProductName =
                intent.Parameters.GetValueOrDefault("ProductName")?.ToString() ?? ""
        };
    }

    private async Task<GetCartQuery> CreateViewCartCommand(
        MessageContext context,
        CancellationToken cancellationToken)
    {
        var customerId = await GetOrCreateCustomerId(context, cancellationToken);
        var businessId = GetBusinessId(context);

        return new GetCartQuery(
            Channel: context.Channel.ToString(),
            CustomerId: customerId,
            BusinessId: businessId,
            Message: "View Cart"
        );
    }

    private async Task<AddToCartCommand> CreateAddToCartCommand(
        IntentResult intent,
        MessageContext context,
        CancellationToken cancellationToken)
    {
        if (!intent.Parameters.ContainsKey("ProductId") &&
            !intent.Parameters.ContainsKey("ProductName"))
        {
            throw new DomainException("Product information is required.");
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

        return new AddToCartCommand(
            Channel: context.Channel.ToString(),
            CustomerId: customerId,
            BusinessId: businessId,
            ProductId: productId,
            Quantity: quantity,
            Message: intent.RawMessage
        );
    }

    private async Task<RemoveFromCartCommand> CreateRemoveFromCartCommand(
        IntentResult intent,
        MessageContext context,
        CancellationToken cancellationToken)
    {
        if (!intent.Parameters.TryGetValue("CartItemId", out var value))
            throw new DomainException("Cart item id is required.");

        if (!Guid.TryParse(value?.ToString(), out var cartItemId))
            throw new DomainException("Invalid cart item id.");

        var customerId = await GetOrCreateCustomerId(context, cancellationToken);
        var businessId = GetBusinessId(context);

        return new RemoveFromCartCommand(
            Channel: context.Channel.ToString(),
            CustomerId: customerId,
            BusinessId: businessId,
            CartItemId: cartItemId,
            Message: intent.RawMessage
        );
    }

    private async Task<CheckoutCommand> CreateCheckoutCommand(
        MessageContext context,
        CancellationToken cancellationToken)
    {
        return new CheckoutCommand
        {
            CustomerId =
                await GetOrCreateCustomerId(context, cancellationToken),

            BusinessId =
                GetBusinessId(context),

            Channel =
                context.Channel.ToString()
        };
    }

    private GetStoreHoursQuery CreateGetStoreHoursCommand(
        MessageContext context)
    {
        return new GetStoreHoursQuery
        {
            BusinessId = GetBusinessId(context)
        };
    }

    private UnknownIntentCommand CreateUnknownIntentCommand(
        IntentResult intent)
    {
        return new UnknownIntentCommand
        {
            Message = intent.RawMessage,
            SuggestedResponse =
                "Sorry, I didn't understand that request."
        };
    }
}