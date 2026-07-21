using BubbleShop.Domain.Entities;
using BubbleShop.Domain.Exceptions;

namespace BubbleShop.Domain.Tests;

public sealed class ProductTests
{

    private static Product CreateProduct(int stock = 5)
    {
        return Product.Create(
            Guid.NewGuid(),
            "Tea",
            "Test",
            10m,
            stock,
            null);
    }
    [Fact]
    public void ReduceStock_ShouldThrow_WhenInsufficientStock()
    {
        var product = CreateProduct(1);

        Assert.Throws<ProductOutOfStockException>(() => product.ReduceStock(2));
    }

    [Fact]
    public void ReduceStock_ShouldUpdateStock_WhenSufficient()
    {
        var product = CreateProduct(5);

        product.ReduceStock(2);

        Assert.Equal(3, product.StockQuantity);
    }
}
