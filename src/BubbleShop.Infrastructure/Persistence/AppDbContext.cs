using BubbleShop.Application.Common.Interfaces;
using BubbleShop.Domain.Common;
using BubbleShop.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BubbleShop.Infrastructure.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options), IUnitOfWork
{
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Delivery> Deliveries => Set<Delivery>();
    public DbSet<Conversation> Conversations => Set<Conversation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var result = await base.SaveChangesAsync(cancellationToken);
        foreach (var entity in ChangeTracker.Entries<BaseEntity>().Select(x => x.Entity))
        {
            entity.ClearDomainEvents();
        }

        return result;
    }
}
