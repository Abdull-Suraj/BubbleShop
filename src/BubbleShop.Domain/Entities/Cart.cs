
using BubbleShop.Domain.Common;
using BubbleShop.Domain.Exceptions;

namespace BubbleShop.Domain.Entities;

public class Cart : BaseEntity
{
    private readonly List<CartItem> _items = new();

    public Guid CustomerId { get; private set; }
    public string? SessionId { get; private set; }
    public DateTime? LastActivityAt { get; private set; }
    public CartStatus Status { get; private set; }
    public string? CouponCode { get; private set; }
    public decimal DiscountAmount { get; private set; }
    public string? Notes { get; private set; }

    // Navigation Properties
    public Customer Customer { get; private set; } = null!;
    public IReadOnlyCollection<CartItem> Items => _items.AsReadOnly();

    private Cart() { }

    public Cart(Guid customerId, string? sessionId = null)
    {
        Id = Guid.NewGuid();
        CustomerId = customerId;
        SessionId = sessionId;
        Status = CartStatus.Active;
        LastActivityAt = DateTime.UtcNow;
        CreatedAt = DateTime.UtcNow;
        DiscountAmount = 0;
    }

    public void AddItem(Guid productId, string productName, int quantity, decimal unitPrice, string? imageUrl = null)
    {
        if (quantity <= 0)
            throw new DomainException("Quantity must be greater than zero");

        var existingItem = _items.FirstOrDefault(i => i.ProductId == productId);

        if (existingItem is not null)
        {
            existingItem.UpdateQuantity(existingItem.Quantity + quantity);
        }
        else
        {
            var item = new CartItem(
                cartId: Id,
                productId: productId,
                productName: productName,
                quantity: quantity,
                unitPrice: unitPrice,
                productImage: imageUrl
            );
            _items.Add(item);
        }

        LastActivityAt = DateTime.UtcNow;
        LastModifiedAt = DateTime.UtcNow;
    }

    public bool RemoveItem(Guid productId)
    {
        var item = _items.FirstOrDefault(i => i.ProductId == productId);
        if (item is null)
            return false;

        _items.Remove(item);
        LastActivityAt = DateTime.UtcNow;
        LastModifiedAt = DateTime.UtcNow;
        return true;
    }

    public bool RemoveItem(string productName)
    {
        var item = _items.FirstOrDefault(i =>
            i.ProductName.Equals(productName, StringComparison.OrdinalIgnoreCase));

        if (item is null)
            return false;

        _items.Remove(item);
        LastActivityAt = DateTime.UtcNow;
        LastModifiedAt = DateTime.UtcNow;
        return true;
    }
    public void AssignCustomer(Guid customerId)
    {
        CustomerId = customerId;
        LastModifiedAt = DateTime.UtcNow;
    }
    public void UpdateQuantity(Guid productId, int quantity)
    {
        if (quantity <= 0)
            throw new DomainException("Quantity must be greater than zero");

        var item = _items.FirstOrDefault(i => i.ProductId == productId);
        if (item is null)
            throw new DomainException($"Product {productId} not found in cart");

        item.UpdateQuantity(quantity);
        LastActivityAt = DateTime.UtcNow;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void Clear()
    {
        _items.Clear();
        DiscountAmount = 0;
        CouponCode = null;
        LastActivityAt = DateTime.UtcNow;
        LastModifiedAt = DateTime.UtcNow;
    }

    public decimal GetTotal()
    {
        var subtotal = _items.Sum(i => i.TotalPrice);
        return subtotal - DiscountAmount;
    }

    public int GetTotalItems()
    {
        return _items.Sum(i => i.Quantity);
    }

    public int GetUniqueItems()
    {
        return _items.Count;
    }

    public void ApplyDiscount(decimal discountAmount, string? couponCode = null)
    {
        if (discountAmount < 0)
            throw new DomainException("Discount amount cannot be negative");

        if (discountAmount > GetTotal())
            throw new DomainException("Discount cannot exceed cart total");

        DiscountAmount = discountAmount;
        CouponCode = couponCode;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void RemoveDiscount()
    {
        DiscountAmount = 0;
        CouponCode = null;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void MarkAsAbandoned()
    {
        Status = CartStatus.Abandoned;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void MarkAsActive()
    {
        Status = CartStatus.Active;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void MarkAsConverted()
    {
        Status = CartStatus.Converted;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void UpdateSessionId(string? sessionId)
    {
        SessionId = sessionId;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void AddNote(string note)
    {
        Notes = string.IsNullOrEmpty(Notes) ? note : $"{Notes}\n{note}";
        LastModifiedAt = DateTime.UtcNow;
    }

    public bool IsEmpty => !_items.Any();
    public bool IsAbandoned => Status == CartStatus.Abandoned;
    public bool IsActive => Status == CartStatus.Active;
}


public enum CartStatus
{
    Active,
    Abandoned,
    Converted
}