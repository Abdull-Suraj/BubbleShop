using BubbleShop.Domain.Common;
using BubbleShop.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BubbleShop.Infrastructure.Persistence.Repositories;

public abstract class Repository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly AppDbContext DbContext;
    protected readonly DbSet<T> _dbSet;

    protected Repository(AppDbContext dbContext)
    {
        DbContext = dbContext;
        _dbSet = DbContext.Set<T>();
    }

    public virtual async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _dbSet.FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted, cancellationToken);

    public virtual async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _dbSet.Where(e => !e.IsDeleted).ToListAsync(cancellationToken);

    public virtual async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddAsync(entity, cancellationToken);
        return entity;
    }

    public virtual Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        _dbSet.Update(entity);
        return Task.CompletedTask;
    }

    public virtual Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
    {
        entity.SoftDelete();  // Soft delete instead of hard delete
        _dbSet.Update(entity);
        return Task.CompletedTask;
    }
}