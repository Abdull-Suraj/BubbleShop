using BubbleShop.Domain.Entities;

namespace BubbleShop.Domain.DomainEvents;

public sealed record PaymentCompletedEvent(Payment Payment) : IsDomainEvent;
