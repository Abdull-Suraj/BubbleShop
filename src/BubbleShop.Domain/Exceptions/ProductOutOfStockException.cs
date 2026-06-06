namespace BubbleShop.Domain.Exceptions;

public sealed class ProductOutOfStockException : DomainException
{
    public ProductOutOfStockException(Guid productId) : base($"Product {productId} is out of stock.")
    {
    }
}
