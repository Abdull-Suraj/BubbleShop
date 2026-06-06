using BubbleShop.Domain.Entities;
using BubbleShop.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BubbleShop.Infrastructure.Persistence.Repositories;

public sealed class OrderRepository(AppDbContext dbContext) : Repository<Order>(dbContext), IOrderRepository
{
    public async Task<IReadOnlyList<Order>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default)
        => await DbContext.Orders.Include(x => x.OrderItems).Where(x => x.CustomerId == customerId).ToListAsync(cancellationToken);

    public async Task<Order?> GetWithItemsAsync(Guid orderId, CancellationToken cancellationToken = default)
        => await DbContext.Orders.Include(x => x.OrderItems).Include(x => x.Payment).Include(x => x.Delivery).FirstOrDefaultAsync(x => x.Id == orderId, cancellationToken);
}
