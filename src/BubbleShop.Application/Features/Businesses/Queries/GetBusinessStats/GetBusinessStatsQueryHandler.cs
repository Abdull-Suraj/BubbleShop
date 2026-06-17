using BubbleShop.Application.Common.Models;
using BubbleShop.Application.DTOs;
using BubbleShop.Domain.Entities;
using BubbleShop.Domain.Enums;
using BubbleShop.Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BubbleShop.Application.Features.Businesses.Queries.GetBusinessStats;

public sealed class GetBusinessStatsQueryHandler : IRequestHandler<GetBusinessStatsQuery, Result<BusinessStatsDto>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly ILogger<GetBusinessStatsQueryHandler> _logger;

    public GetBusinessStatsQueryHandler(
        IOrderRepository orderRepository,
        IProductRepository productRepository,
        ICustomerRepository customerRepository,
        IPaymentRepository paymentRepository,
        ILogger<GetBusinessStatsQueryHandler> logger)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _customerRepository = customerRepository;
        _paymentRepository = paymentRepository;
        _logger = logger;
    }

    public async Task<Result<BusinessStatsDto>> Handle(GetBusinessStatsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Getting business stats for: {BusinessId}", request.BusinessId);

            // Get all orders for the business
            var orders = await _orderRepository.GetByBusinessIdAsync(request.BusinessId, cancellationToken);
            var ordersList = orders.ToList();

            // Filter by date range if provided
            if (request.FromDate.HasValue)
                ordersList = ordersList.Where(o => o.CreatedAt >= request.FromDate.Value).ToList();
            if (request.ToDate.HasValue)
                ordersList = ordersList.Where(o => o.CreatedAt <= request.ToDate.Value).ToList();

            // Get all products
            var products = await _productRepository.GetByBusinessIdAsync(request.BusinessId, cancellationToken);
            var productsList = products.ToList();

            // Get all customers
            var customers = await _customerRepository.GetByBusinessIdAsync(request.BusinessId, cancellationToken);
            var customersList = customers.ToList();

            // Get all successful payments - FIXED: Use PaymentStatus.Successful
            var payments = await _paymentRepository.GetByBusinessIdAsync(request.BusinessId, cancellationToken);
            var successfulPayments = payments.Where(p => p.Status == 
            
            PaymentStatus.Successful).ToList();

            // Calculate stats
            var totalOrders = ordersList.Count;
            var completedOrders = ordersList.Count(o => o.Status == OrderStatus.Completed || o.Status == OrderStatus.Delivered);
            var pendingOrders = ordersList.Count(o => o.Status == OrderStatus.Pending || o.Status == OrderStatus.PaymentPending);
            var cancelledOrders = ordersList.Count(o => o.Status == OrderStatus.Cancelled);

            var totalRevenue = successfulPayments.Sum(p => p.Amount);

            // FIXED: Use correct property names - PlatformFee and BusinessEarnings
            var platformFees = successfulPayments.Sum(p => p.PlatformFee);
            var netRevenue = successfulPayments.Sum(p => p.BusinessEarnings);

            var averageOrderValue = totalOrders > 0 ? totalRevenue / totalOrders : 0;

            var totalProducts = productsList.Count;
            var activeProducts = productsList.Count(p => p.IsActive);
            var outOfStockProducts = productsList.Count(p => p.StockQuantity == 0);
            var lowStockProducts = productsList.Count(p => p.StockQuantity > 0 && p.StockQuantity <= 10);

            var totalCustomers = customersList.Count;
            var newCustomers = customersList.Count(c => c.CreatedAt >= DateTime.UtcNow.AddMonths(-1));

            // FIXED: Get recent orders with correct property names
            var recentOrders = ordersList
                .OrderByDescending(o => o.CreatedAt)
                .Take(10)
                .Select(o => new RecentOrderDto
                {
                    OrderId = o.Id,
                    OrderNumber = o.OrderNumber,
                    CustomerName = o.CustomerName ?? "Customer",  // Handle null
                    Amount = o.TotalAmount,
                    Status = o.Status.ToString(),
                    CreatedAt = o.CreatedAt  // FIXED: DateTimeOffset to DateTime conversion
                }).ToList();

            var stats = new BusinessStatsDto
            {
                // Order Stats
                TotalOrders = totalOrders,
                CompletedOrders = completedOrders,
                PendingOrders = pendingOrders,
                CancelledOrders = cancelledOrders,

                // Revenue Stats
                TotalRevenue = totalRevenue,
                PlatformFees = platformFees,
                NetRevenue = netRevenue,
                AverageOrderValue = averageOrderValue,

                // Product Stats
                TotalProducts = totalProducts,
                ActiveProducts = activeProducts,
                OutOfStockProducts = outOfStockProducts,
                LowStockProducts = lowStockProducts,

                // Customer Stats
                TotalCustomers = totalCustomers,
                NewCustomersThisMonth = newCustomers,

                // Recent Activity
                RecentOrders = recentOrders,

                // Period
                PeriodStart = request.FromDate,
                PeriodEnd = request.ToDate
            };

            return Result<BusinessStatsDto>.Success(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting business stats for: {BusinessId}", request.BusinessId);
            return Result<BusinessStatsDto>.Failure($"Failed to retrieve business stats: {ex.Message}");
        }
    }
}