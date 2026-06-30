//using BubbleShop.Application.Common.Interfaces;
//using DomainDelivery = BubbleShop.Domain.Entities.Delivery;
//using Microsoft.Extensions.Logging;

//namespace BubbleShop.Application.AppServices;

//public sealed class DeliveryService(ILogger<DeliveryService> logger) : IDeliveryService
//{
//    public Task<string> ArrangeDeliveryAsync(DomainDelivery delivery, CancellationToken cancellationToken = default)
//    {
//        logger.LogInformation("Arranging delivery for order {OrderId}", delivery.OrderId);
//        // TODO: integrate with real provider API.
//        return Task.FromResult($"TRK-{delivery.OrderId:N}"[..16]);
//    }
//}
