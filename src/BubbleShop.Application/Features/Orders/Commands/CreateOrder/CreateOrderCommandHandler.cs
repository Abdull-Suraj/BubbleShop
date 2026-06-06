using BubbleShop.Application.Common.Interfaces;
using BubbleShop.Application.Common.Models;
using BubbleShop.Domain.Entities;
using BubbleShop.Domain.Exceptions;
using BubbleShop.Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BubbleShop.Application.Features.Orders.Commands.CreateOrder;

public sealed class CreateOrderCommandHandler(
    IOrderRepository orderRepository,
    IProductRepository productRepository,
    IUnitOfWork unitOfWork,
    ILogger<CreateOrderCommandHandler> logger)
    : IRequestHandler<CreateOrderCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        if (request.CustomerId == Guid.Empty)
        {
            throw new DomainException("Customer is required.");
        }

        if (request.Items.Count == 0)
        {
            throw new DomainException("At least one order item is required.");
        }

        var orderItems = new List<OrderItem>();
        foreach (var item in request.Items)
        {
            var product = await productRepository.GetByIdAsync(item.ProductId, cancellationToken);
            if (product is null)
            {
                return Result<Guid>.Failure($"Product {item.ProductId} not found.");
            }

            product.ReduceStock(item.Quantity);
            await productRepository.UpdateAsync(product, cancellationToken);
            orderItems.Add(OrderItem.Create(Guid.Empty, product.Id, item.Quantity, product.Price));
        }

        var order = Order.Create(request.CustomerId, orderItems.Select(i => OrderItem.Create(Guid.Empty, i.ProductId, i.Quantity, i.UnitPrice)));
        await orderRepository.AddAsync(order, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Order {OrderId} created", order.Id);
        return Result<Guid>.Success(order.Id);
    }
}
