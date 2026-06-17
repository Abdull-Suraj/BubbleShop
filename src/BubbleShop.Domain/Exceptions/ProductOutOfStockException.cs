// Domain/Exceptions/ProductOutOfStockException.cs
using BubbleShop.Domain.Exceptions;

namespace BubbleShop.Domain.Exceptions;

public class ProductOutOfStockException : DomainException
{
    public Guid ProductId { get; }

    public ProductOutOfStockException(Guid productId)
        : base($"Product {productId} is out of stock.")
    {
        ProductId = productId;
    }

    public ProductOutOfStockException(Guid productId, string message)
        : base(message)
    {
        ProductId = productId;
    }
}