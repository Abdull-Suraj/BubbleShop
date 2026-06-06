using BubbleShop.Domain.Interfaces.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BubbleShop.API.Controllers;

[ApiController]
[Authorize]
[Route("api/dashboard")]
public sealed class DashboardController(IOrderRepository orderRepository, IProductRepository productRepository) : ControllerBase
{
    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary(CancellationToken cancellationToken)
    {
        var orders = await orderRepository.GetAllAsync(cancellationToken);
        var products = await productRepository.GetAllAsync(cancellationToken);

        var totalRevenue = orders.Sum(x => x.TotalAmount);
        var lowStock = products.Count(x => x.StockQuantity < 5);

        return Ok(new
        {
            TotalOrders = orders.Count,
            Revenue = totalRevenue,
            LowStockAlerts = lowStock
        });
    }
}
