using BubbleShop.Application.Common.Models;
using BubbleShop.Domain.Interfaces.Repositories;
using MediatR;

namespace BubbleShop.Application.Features.Deliveries.Queries.GetDeliveryStatus;

public sealed class GetDeliveryStatusQueryHandler(IOrderRepository orderRepository) : IRequestHandler<GetDeliveryStatusQuery, Result<DeliveryStatusDto>>
{
    public async Task<Result<DeliveryStatusDto>> Handle(GetDeliveryStatusQuery request, CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order?.Delivery is null)
        {
            return Result<DeliveryStatusDto>.Failure("Delivery not found.");
        }

        return Result<DeliveryStatusDto>.Success(new DeliveryStatusDto(order.Id, order.Delivery.Status, order.Delivery.TrackingNumber));
    }
}
