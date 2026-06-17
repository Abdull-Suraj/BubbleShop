// Infrastructure/Persistence/Repositories/PaymentRepository.cs
using BubbleShop.Domain.Entities;
using BubbleShop.Domain.Enums;
using BubbleShop.Domain.Interfaces.Repositories;
using BubbleShop.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BubbleShop.Infrastructure.Persistence.Repositories;

public class PaymentRepository : Repository<Payment>, IPaymentRepository
{
    public PaymentRepository(AppDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<IReadOnlyList<Payment>> GetByBusinessIdAsync(Guid businessId, CancellationToken cancellationToken = default)
        => await _dbSet
            .Include(p => p.Order)
            .Where(p => p.BusinessId == businessId && !p.IsDeleted)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Payment>> GetPaymentsByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default)
        => await _dbSet
            .Include(p => p.Order)
            .Where(p => p.CustomerId == customerId && !p.IsDeleted)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<Payment?> GetByTransactionReferenceAsync(string transactionReference, CancellationToken cancellationToken = default)
        => await _dbSet
            .Include(p => p.Order)
            .FirstOrDefaultAsync(p => p.TransactionReference == transactionReference && !p.IsDeleted, cancellationToken);

    public async Task<Payment?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default)
        => await _dbSet
            .Include(p => p.Order)
            .FirstOrDefaultAsync(p => p.OrderId == orderId && !p.IsDeleted, cancellationToken);

    public async Task<IReadOnlyList<Payment>> GetPaymentsByStatusAsync(PaymentStatus status, CancellationToken cancellationToken = default)
        => await _dbSet
            .Where(p => p.Status == status && !p.IsDeleted)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<decimal> GetTotalRevenueByBusinessAsync(Guid businessId, DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.Where(p => p.BusinessId == businessId && p.Status == PaymentStatus.Successful && !p.IsDeleted);

        if (fromDate.HasValue)
            query = query.Where(p => p.PaidAt >= fromDate.Value);
        if (toDate.HasValue)
            query = query.Where(p => p.PaidAt <= toDate.Value);

        return await query.SumAsync(p => p.Amount, cancellationToken);
    }
}