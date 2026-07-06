using BubbleShop.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BubbleShop.Infrastructure.Persistence.EntityConfigurations;

public sealed class BusinessConfiguration : IEntityTypeConfiguration<Business>
{
    public void Configure(EntityTypeBuilder<Business> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.BusinessName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Email)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(x => x.WhatsAppNumber)
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(x => x.PhoneNumber)
            .HasMaxLength(32);

        builder.Property(x => x.Address)
            .HasMaxLength(300);

        builder.Property(x => x.City)
            .HasMaxLength(100);

        builder.Property(x => x.State)
            .HasMaxLength(100);

        builder.Property(x => x.Country)
            .HasMaxLength(100);

        builder.Property(x => x.PostalCode)
            .HasMaxLength(30);

        builder.Property(x => x.Currency)
            .HasMaxLength(10);

        builder.Property(x => x.CommissionRate)
            .HasPrecision(18, 2);

        builder.Property(x => x.WalletBalance)
            .HasPrecision(18, 2);

        builder.HasMany(b => b.Customers)
     .WithOne(c => c.Business)
     .HasForeignKey(c => c.BusinessId)
     .OnDelete(DeleteBehavior.NoAction);

        builder.Navigation(b => b.Customers)
    .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(b => b.Products)
            .WithOne(p => p.Business)
            .HasForeignKey(p => p.BusinessId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(b => b.Products)
    .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(b => b.Orders)
            .WithOne(o => o.Business)
            .HasForeignKey(o => o.BusinessId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Navigation(b => b.Orders)
    .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(b => b.Payments)
            .WithOne(p => p.Business)
            .HasForeignKey(p => p.BusinessId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Navigation(b => b.Payments)
    .UsePropertyAccessMode(PropertyAccessMode.Field);
       
        builder.HasMany(b => b.Conversations)
            .WithOne(c => c.Business)
            .HasForeignKey(c => c.BusinessId)
            .OnDelete(DeleteBehavior.NoAction);
        builder.Navigation(b => b.Conversations)
    .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(b => b.AutomationRules)
            .WithOne(a => a.Business)
            .HasForeignKey(a => a.BusinessId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(b => b.AutomationRules)
    .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}