using BubbleShop.Domain.Entities;
using BubbleShop.Domain.Exceptions;

namespace BubbleShop.Domain.Tests;

public sealed class ProductTests
{
    [Fact]
    public void ReduceStock_ShouldThrow_WhenInsufficientStock()
    {
        var product = Product.Create("Tea", "Test", 10m, 1, null);
        Assert.Throws<ProductOutOfStockException>(() => product.ReduceStock(2));
    }

    [Fact]
    public void ReduceStock_ShouldUpdateStock_WhenSufficient()
    {
        var product = Product.Create("Tea", "Test", 10m, 5, null);
        product.ReduceStock(2);
        Assert.Equal(3, product.StockQuantity);
    }
}
