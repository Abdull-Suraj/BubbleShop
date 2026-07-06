using BubbleShop.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BubbleShop.Infrastructure.Persistence.EntityConfigurations;

public sealed class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Provider).HasMaxLength(64);
        builder.Property(x => x.Amount).HasPrecision(18, 2);
        builder.Property(x => x.TransactionId).HasMaxLength(128);
        builder.Property(p => p.PlatformFee)
         .HasPrecision(18, 2);

        builder.Property(p => p.PaymentGatewayFee)
            .HasPrecision(18, 2);

        builder.Property(p => p.BusinessEarnings)
            .HasPrecision(18, 2);

        builder.Property(p => p.AmountPaid)
            .HasPrecision(18, 2);

        builder.Property(p => p.AmountRefunded)
            .HasPrecision(18, 2);

        builder.HasOne(p => p.Order)
    .WithOne(o => o.Payment)
    .HasForeignKey<Payment>(p => p.OrderId);

        builder.HasOne(p => p.Business)
            .WithMany(b => b.Payments)
            .HasForeignKey(p => p.BusinessId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(p => p.Customer)
            .WithMany()
            .HasForeignKey(p => p.CustomerId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
