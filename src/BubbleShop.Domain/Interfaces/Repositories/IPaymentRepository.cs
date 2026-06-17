
using BubbleShop.Domain.Entities;
using BubbleShop.Domain.Enums;
using BubbleShop.Domain.Interfaces.Repositories;

public interface IPaymentRepository : IRepository<Payment>
{
    Task<IReadOnlyList<Payment>> GetByBusinessIdAsync(Guid businessId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Payment>> GetPaymentsByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<Payment?> GetByTransactionReferenceAsync(string transactionReference, CancellationToken cancellationToken = default);
    Task<Payment?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Payment>> GetPaymentsByStatusAsync(PaymentStatus status, CancellationToken cancellationToken = default);
    Task<decimal> GetTotalRevenueByBusinessAsync(Guid businessId, DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default);
}