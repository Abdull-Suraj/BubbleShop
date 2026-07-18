using BubbleShop.Domain.Entities;

namespace BubbleShop.Domain.Interfaces.Repositories
{
    public interface ICartRepository
    {
        Task<Cart?> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);
        Task<Cart> GetOrCreateCartAsync(Guid customerId, CancellationToken cancellationToken = default);
        Task<Cart?> GetCartWithDetailsAsync(Guid customerId, CancellationToken cancellationToken = default);
        Task ClearCartAsync(Guid customerId, CancellationToken cancellationToken = default);
        Task<int> GetCartItemCountAsync(Guid customerId, CancellationToken cancellationToken = default);
        Task<decimal> GetCartTotalAsync(Guid customerId, CancellationToken cancellationToken = default);
        Task<bool> IsProductInCartAsync(Guid customerId, Guid productId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Cart>> GetAbandonedCartsAsync(DateTime olderThan, CancellationToken cancellationToken = default);
        Task<Cart> MergeCartsAsync(Guid customerId, Guid guestCartId, CancellationToken cancellationToken = default);
        Task<bool> RemoveExpiredCartsAsync(DateTime olderThan, CancellationToken cancellationToken = default);
        Task<int> GetActiveCartCountAsync(CancellationToken cancellationToken = default);
        Task<Dictionary<Guid, int>> GetCartCountsByBusinessAsync(CancellationToken cancellationToken = default);
    }
}