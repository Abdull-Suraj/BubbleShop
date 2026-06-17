using BubbleShop.Application.Common.Interfaces;
using BubbleShop.Application.Common.Models;
using BubbleShop.Domain.Entities;
using BubbleShop.Domain.Exceptions;
using BubbleShop.Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BubbleShop.Application.Features.Orders.Commands.CreateOrder;

public sealed class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, Result<Guid>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateOrderCommandHandler> _logger;

    public CreateOrderCommandHandler(
        IOrderRepository orderRepository,
        IProductRepository productRepository,
        ICustomerRepository customerRepository,
        IUnitOfWork unitOfWork,
        ILogger<CreateOrderCommandHandler> logger)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _customerRepository = customerRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Creating order for customer: {CustomerId}", request.CustomerId);

            // Validate BusinessId
            if (request.BusinessId == Guid.Empty)
            {
                return Result<Guid>.Failure("Business ID is required.", "ValidationError");
            }

            // Validate CustomerId
            if (request.CustomerId == Guid.Empty)
            {
                return Result<Guid>.Failure("Customer ID is required.", "ValidationError");
            }

            // Validate items
            if (request.Items == null || request.Items.Count == 0)
            {
                return Result<Guid>.Failure("At least one order item is required.", "ValidationError");
            }

            // Get customer
            var customer = await _customerRepository.GetByIdAsync(request.CustomerId, cancellationToken);
            if (customer is null)
            {
                return Result<Guid>.Failure($"Customer {request.CustomerId} not found.", "NotFound");
            }

            var orderItems = new List<OrderItem>();

            foreach (var item in request.Items)
            {
                var product = await _productRepository.GetByIdAsync(item.ProductId, cancellationToken);
                if (product is null)
                {
                    return Result<Guid>.Failure($"Product {item.ProductId} not found.", "NotFound");
                }

                if (product.StockQuantity < item.Quantity)
                {
                    return Result<Guid>.Failure(
                        $"Insufficient stock for {product.Name}. Available: {product.StockQuantity}",
                        "ValidationError"
                    );
                }

                // Create order item
                var orderItem = new OrderItem(
                    productId: product.Id,
                    productName: product.Name,
                    quantity: item.Quantity,
                    unitPrice: product.Price,
                    productSKU: product.SKU,
                    productImage: product.ThumbnailUrl
                );

                orderItems.Add(orderItem);

                // Reduce stock
                product.ReduceStock(item.Quantity);
                await _productRepository.UpdateAsync(product, cancellationToken);
            }

            // Create order with all required parameters
            var order = Order.Create(
                businessId: request.BusinessId,
                customerId: request.CustomerId,
                items: orderItems,
                customerName: request.CustomerName ?? customer.Name,
                customerWhatsApp: request.CustomerWhatsApp ?? customer.WhatsAppNumber
            );

            await _orderRepository.AddAsync(order, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Order {OrderId} created successfully", order.Id);

            return Result<Guid>.Success(order.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating order");
            return Result<Guid>.Failure($"Failed to create order: {ex.Message}");
        }
    }
}