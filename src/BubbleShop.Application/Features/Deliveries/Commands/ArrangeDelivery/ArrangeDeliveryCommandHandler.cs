using BubbleShop.Application.Common.Interfaces;
using BubbleShop.Application.Common.Models;
using BubbleShop.Domain.Entities;
using BubbleShop.Domain.Interfaces.Repositories;
using MediatR;

namespace BubbleShop.Application.Features.Deliveries.Commands.ArrangeDelivery;

public sealed class ArrangeDeliveryCommandHandler(IOrderRepository orderRepository, IDeliveryService deliveryService, IUnitOfWork unitOfWork)
    : IRequestHandler<ArrangeDeliveryCommand, Result<string>>
{
    public async Task<Result<string>> Handle(ArrangeDeliveryCommand request, CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order is null)
        {
            return Result<string>.Failure("Order not found.");
        }

        var delivery = Delivery.Create(order.Id, request.RecipientName, request.AddressLine1, request.AddressLine2, request.City, request.Postcode, request.Country);
        var trackingNumber = await deliveryService.ArrangeDeliveryAsync(delivery, cancellationToken);
        delivery.Arrange("Default", trackingNumber);
        order.AttachDelivery(delivery);

        await orderRepository.UpdateAsync(order, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<string>.Success(trackingNumber);
    }
}
