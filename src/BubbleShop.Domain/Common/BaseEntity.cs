// Domain/Common/BaseEntity.cs
using MediatR;

namespace BubbleShop.Domain.Common;

public abstract class BaseEntity
{
    public Guid Id { get; protected set; }
    public DateTime CreatedAt { get; protected set; }
    public DateTime? LastModifiedAt { get; protected set; }
    public bool IsDeleted { get; protected set; }

    // Domain events collection
    private readonly List<INotification> _domainEvents = new();
    public IReadOnlyCollection<INotification> DomainEvents => _domainEvents.AsReadOnly();

    protected BaseEntity()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
        IsDeleted = false;
    }

    protected void AddDomainEvent(INotification domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    protected void RemoveDomainEvent(INotification domainEvent)
    {
        _domainEvents.Remove(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    public void SoftDelete()
    {
        IsDeleted = true;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void Restore()
    {
        IsDeleted = false;
        LastModifiedAt = DateTime.UtcNow;
    }
}