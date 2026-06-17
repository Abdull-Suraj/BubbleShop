
using MediatR;
using BubbleShop.Domain.Entities;

namespace BubbleShop.Domain.DomainEvents;

public record OrderPlacedEvent : INotification
{
    public Order Order { get; }
    public DateTime OccurredOn { get; }

    public OrderPlacedEvent(Order order)
    {
        Order = order;
        OccurredOn = DateTime.UtcNow;
    }
}