using BubbleShop.Domain.Entities;

namespace BubbleShop.Domain.DomainEvents;

public sealed record OrderPlacedEvent(Order Order) : IsDomainEvent;
