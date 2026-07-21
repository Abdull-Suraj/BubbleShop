using BubbleShop.Domain.Entities;
using BubbleShop.Domain.Enums;
using BubbleShop.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BubbleShop.Infrastructure.Persistence.Repositories;

public sealed class OrderRepository : Repository<Order>, IOrderRepository
{
    public OrderRepository(AppDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<IReadOnlyList<Order>> GetByBusinessIdAsync(Guid businessId, CancellationToken cancellationToken = default)
        => await _dbSet
            .Include(x => x.OrderItems)
            .Include(x => x.Payment)
            //.Include(x => x.Delivery)
            .Where(x => x.BusinessId == businessId && !x.IsDeleted)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Order>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default)
        => await _dbSet
            .Include(x => x.OrderItems)
            .Include(x => x.Payment)
            //.Include(x => x.Delivery)
            .Where(x => x.CustomerId == customerId && !x.IsDeleted)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<Order?> GetByOrderNumberAsync(string orderNumber, CancellationToken cancellationToken = default)
        => await _dbSet
            .Include(x => x.OrderItems)
            .Include(x => x.Payment)
            //.Include(x => x.Delivery)
            .FirstOrDefaultAsync(x => x.OrderNumber == orderNumber && !x.IsDeleted, cancellationToken);

    public async Task<Order?> GetWithItemsAsync(Guid orderId, CancellationToken cancellationToken = default)
        => await _dbSet
            .Include(x => x.OrderItems)
            .Include(x => x.Payment)
            //.Include(x => x.Delivery)
            .FirstOrDefaultAsync(x => x.Id == orderId && !x.IsDeleted, cancellationToken);
    public async Task<Order?> GetLatestPendingOrderAsync(
    Guid customerId,
    CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(x => x.OrderItems)
            .Where(x =>
                x.CustomerId == customerId &&
                !x.IsDeleted &&
                (x.Status == OrderStatus.Pending ||
                 x.Status == OrderStatus.Confirmed))
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }
}