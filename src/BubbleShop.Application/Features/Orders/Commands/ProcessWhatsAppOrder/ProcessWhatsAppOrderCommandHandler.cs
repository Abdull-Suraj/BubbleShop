// Application/Features/Orders/Commands/ProcessWhatsAppOrder/ProcessWhatsAppOrderCommandHandler.cs
using BubbleShop.Application.Common.Interfaces;
using BubbleShop.Application.Common.Models;
using BubbleShop.Domain.Entities;
using BubbleShop.Domain.Enums;
using BubbleShop.Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BubbleShop.Application.Features.Orders.Commands.ProcessWhatsAppOrder;

public sealed class ProcessWhatsAppOrderCommandHandler : IRequestHandler<ProcessWhatsAppOrderCommand, Result<WhatsAppOrderResponse>>
{
    private readonly IBusinessRepository _businessRepository;
    private readonly IProductRepository _productRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ProcessWhatsAppOrderCommandHandler> _logger;

    public ProcessWhatsAppOrderCommandHandler(
        IBusinessRepository businessRepository,
        IProductRepository productRepository,
        ICustomerRepository customerRepository,
        IOrderRepository orderRepository,
        IUnitOfWork unitOfWork,
        ILogger<ProcessWhatsAppOrderCommandHandler> logger)
    {
        _businessRepository = businessRepository;
        _productRepository = productRepository;
        _customerRepository = customerRepository;
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<WhatsAppOrderResponse>> Handle(ProcessWhatsAppOrderCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Processing WhatsApp order from {Customer} to {Business}",
                request.CustomerWhatsApp, request.BusinessWhatsApp);

            // 1. Find business by WhatsApp number
            var business = await _businessRepository.GetByWhatsAppNumberAsync(request.BusinessWhatsApp, cancellationToken);
            if (business is null)
                return Result<WhatsAppOrderResponse>.Failure($"Business with WhatsApp {request.BusinessWhatsApp} not found", "NotFound");

            // 2. Find or create customer
            var customer = await GetOrCreateCustomer(request, business.Id, cancellationToken);

            // 3. Parse message to extract product and quantity
            var (productName, quantity) = ParseOrderMessage(request.Message);

            if (string.IsNullOrEmpty(productName))
                return Result<WhatsAppOrderResponse>.Failure("Could not understand your order. Please specify product name and quantity.", "ValidationError");

            // 4. Find product by name
            var product = await _productRepository.GetByNameAsync(productName, business.Id, cancellationToken);
            if (product is null)
                return Result<WhatsAppOrderResponse>.Failure($"Product '{productName}' not found. Please check the name and try again.", "NotFound");

            if (product.StockQuantity < quantity)
                return Result<WhatsAppOrderResponse>.Failure($"Only {product.StockQuantity} units of {product.Name} available. Would you like to order {product.StockQuantity} instead?", "ValidationError");

            // 5. Create order
            var order = await CreateOrder(customer, product, quantity, business.Id, cancellationToken);

            // 6. Reduce stock
            product.ReduceStock(quantity);
            await _productRepository.UpdateAsync(product, cancellationToken);

            // 7. Generate response
            var responseMessage = GenerateOrderResponse(order, product, quantity);

            return Result<WhatsAppOrderResponse>.Success(new WhatsAppOrderResponse(
                ResponseMessage: responseMessage,
                OrderId: order.Id,
                OrderNumber: order.OrderNumber,
                TotalAmount: order.TotalAmount,
                ProductName: product.Name,
                Quantity: quantity
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing WhatsApp order");
            return Result<WhatsAppOrderResponse>.Failure($"Failed to process order: {ex.Message}");
        }
    }

    private async Task<Customer> GetOrCreateCustomer(ProcessWhatsAppOrderCommand request, Guid businessId, CancellationToken cancellationToken)
    {
        var existingCustomer = await _customerRepository.GetByWhatsAppNumberAsync(
            request.CustomerWhatsApp,
            businessId,
            cancellationToken);

        if (existingCustomer is not null)
            return existingCustomer;

        var customer = new Customer(
            businessId: businessId,
            name: request.CustomerName?.Split(' ').FirstOrDefault() ?? "Valued",
            whatsappNumber: request.CustomerWhatsApp,
            phoneNumber: request.CustomerWhatsApp,
            email: null
        );

        await _customerRepository.AddAsync(customer, cancellationToken);
        return customer;
    }

    private (string productName, int quantity) ParseOrderMessage(string message)
    {
        var lowerMessage = message.ToLower();
        var words = lowerMessage.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        int quantity = 1;
        string productName = string.Empty;

        // Try to extract quantity (number before product name)
        for (int i = 0; i < words.Length; i++)
        {
            if (int.TryParse(words[i], out int qty))
            {
                quantity = qty;
                if (i + 1 < words.Length)
                {
                    productName = words[i + 1];
                    break;
                }
            }
            else if (i + 1 < words.Length)
            {
                // If product is multiple words, capture them
                if (words[i] == "want" || words[i] == "buy" || words[i] == "get")
                {
                    productName = string.Join(" ", words.Skip(i + 1));
                    break;
                }
            }
        }

        // Remove common words from product name
        var removeWords = new[] { "of", "bag", "bags", "kg", "piece", "pieces", "pack", "packs" };
        foreach (var word in removeWords)
        {
            productName = productName.Replace(word, "").Trim();
        }

        return (productName, quantity);
    }

    private async Task<Order> CreateOrder(Customer customer, Product product, int quantity, Guid businessId, CancellationToken cancellationToken)
    {
        var orderItem = new OrderItem(
            productId: product.Id,
            productName: product.Name,
            quantity: quantity,
            unitPrice: product.Price,
            productSKU: product.SKU,
            productImage: product.ThumbnailUrl
        );

        var order = Order.Create(
            businessId: businessId,
            customerId: customer.Id,
            items: new List<OrderItem> { orderItem },
            customerName: customer.Name,
            customerWhatsApp: customer.WhatsAppNumber
        );

        await _orderRepository.AddAsync(order, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return order;
    }

    private string GenerateOrderResponse(Order order, Product product, int quantity)
    {
        return $"✅ **Order Created Successfully!** 🎉\n\n" +
               $"📦 **Order Number:** {order.OrderNumber}\n" +
               $"🛍️ **Product:** {product.Name}\n" +
               $"🔢 **Quantity:** {quantity}\n" +
               $"💰 **Total Amount:** ${order.TotalAmount:F2}\n\n" +
               $"📅 **Estimated Delivery:** {DateTime.UtcNow.AddDays(2):dddd, MMMM d, yyyy}\n\n" +
               $"We'll notify you when your order is confirmed.\n" +
               $"Reply `TRACK {order.OrderNumber}` to track your order.\n\n" +
               $"Thank you for shopping with us! 🙏";
    }
}