using BubbleShop.Domain.Entities;

namespace BubbleShop.Application.Common.Interfaces;

public interface IDeliveryService
{
    Task<string> ArrangeDeliveryAsync(Delivery delivery, CancellationToken cancellationToken = default);
}
