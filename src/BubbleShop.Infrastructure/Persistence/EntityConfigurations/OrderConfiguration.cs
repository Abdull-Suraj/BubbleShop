using BubbleShop.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BubbleShop.Infrastructure.Persistence.EntityConfigurations;

public sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.TotalAmount).HasPrecision(18, 2);
        builder.HasMany(x => x.OrderItems).WithOne().HasForeignKey(x => x.OrderId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Payment).WithOne().HasForeignKey<Payment>(x => x.OrderId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Delivery).WithOne().HasForeignKey<Delivery>(x => x.OrderId).OnDelete(DeleteBehavior.Cascade);
    }
}
