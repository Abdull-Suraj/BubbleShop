
using Microsoft.EntityFrameworkCore;
using BubbleShop.Domain.Entities;
using BubbleShop.Domain.Interfaces.Repositories;
using BubbleShop.Infrastructure.Persistence;

namespace BubbleShop.Infrastructure.Persistence.Repositories;

public class CartRepository : Repository<Cart>, ICartRepository
{
    public CartRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<Cart?> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.CustomerId == customerId && !c.IsDeleted, cancellationToken);
    }

    public async Task<Cart> GetOrCreateCartAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        var cart = await GetByCustomerIdAsync(customerId, cancellationToken);

        if (cart is not null)
        {
            // Reactivate if abandoned
            if (cart.Status == CartStatus.Abandoned)
            {
                cart.MarkAsActive();
                await UpdateAsync(cart, cancellationToken);
            }
            return cart;
        }

        cart = new Cart(customerId);
        await AddAsync(cart, cancellationToken);
        return cart;
    }

    public async Task<Cart?> GetCartWithDetailsAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(c => c.Items)
                .ThenInclude(i => i.Product)
            .Include(c => c.Customer)
            .FirstOrDefaultAsync(c => c.CustomerId == customerId && !c.IsDeleted, cancellationToken);
    }

    public async Task ClearCartAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        var cart = await GetByCustomerIdAsync(customerId, cancellationToken);
        if (cart is not null)
        {
            cart.Clear();
            await UpdateAsync(cart, cancellationToken);
        }
    }

    public async Task<int> GetCartItemCountAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        var cart = await GetByCustomerIdAsync(customerId, cancellationToken);
        return cart?.GetTotalItems() ?? 0;
    }

    public async Task<decimal> GetCartTotalAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        var cart = await GetByCustomerIdAsync(customerId, cancellationToken);
        return cart?.GetTotal() ?? 0;
    }

    public async Task<bool> IsProductInCartAsync(Guid customerId, Guid productId, CancellationToken cancellationToken = default)
    {
        var cart = await GetByCustomerIdAsync(customerId, cancellationToken);
        if (cart is null)
            return false;

        return cart.Items.Any(i => i.ProductId == productId);
    }

    public async Task<IReadOnlyList<Cart>> GetAbandonedCartsAsync(DateTime olderThan, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(c => c.Items)
            .Include(c => c.Customer)
            .Where(c => c.Status == CartStatus.Abandoned
                        && c.LastActivityAt <= olderThan
                        && !c.IsDeleted)
            .ToListAsync(cancellationToken);
    }

    public async Task<Cart> MergeCartsAsync(Guid customerId, Guid guestCartId, CancellationToken cancellationToken = default)
    {
        // Get both carts
        var userCart = await GetByCustomerIdAsync(customerId, cancellationToken);
        var guestCart = await GetByIdAsync(guestCartId, cancellationToken);

        if (guestCart is null)
            return userCart ?? new Cart(customerId);

        if (userCart is null)
        {
            // Transfer guest cart to user
            guestCart.UpdateSessionId(null);
            guestCart.AssignCustomer(customerId);
            guestCart.MarkAsActive();
            await UpdateAsync(guestCart, cancellationToken);
            return guestCart;
        }

        // Merge guest cart items into user cart
        foreach (var guestItem in guestCart.Items.ToList())
        {
            userCart.AddItem(
                guestItem.ProductId,
                guestItem.ProductName,
                guestItem.Quantity,
                guestItem.UnitPrice,
                guestItem.ProductImage
            );
        }

        // Delete guest cart
        await DeleteAsync(guestCart, cancellationToken);
        await UpdateAsync(userCart, cancellationToken);

        return userCart;
    }

    // Additional helper methods for cart management
    public async Task<bool> RemoveExpiredCartsAsync(DateTime olderThan, CancellationToken cancellationToken = default)
    {
        var expiredCarts = await _dbSet
            .Where(c => c.LastActivityAt <= olderThan
                        && c.Status == CartStatus.Abandoned
                        && !c.IsDeleted)
            .ToListAsync(cancellationToken);

        if (!expiredCarts.Any())
            return false;

        foreach (var cart in expiredCarts)
        {
            await DeleteAsync(cart, cancellationToken);
        }

        return true;
    }

    public async Task<int> GetActiveCartCountAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .CountAsync(c => c.Status == CartStatus.Active && !c.IsDeleted, cancellationToken);
    }

    public async Task<Dictionary<Guid, int>> GetCartCountsByBusinessAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(c => c.Customer)
            .Where(c =>
                c.Status == CartStatus.Active &&
                !c.IsDeleted &&
                c.Customer.BusinessId.HasValue)
            .GroupBy(c => c.Customer.BusinessId!.Value)
            .Select(g => new
            {
                BusinessId = g.Key,
                Count = g.Count()
            })
            .ToDictionaryAsync(
                x => x.BusinessId,
                x => x.Count,
                cancellationToken);
    }
}