using BubbleShop.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BubbleShop.Infrastructure.Persistence.EntityConfigurations;

public sealed class DeliveryConfiguration : IEntityTypeConfiguration<Delivery>
{
    public void Configure(EntityTypeBuilder<Delivery> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.RecipientName).HasMaxLength(128).IsRequired();
        builder.Property(x => x.AddressLine1).HasMaxLength(256).IsRequired();
        builder.Property(x => x.AddressLine2).HasMaxLength(256);
        builder.Property(x => x.City).HasMaxLength(128).IsRequired();
        builder.Property(x => x.Postcode).HasMaxLength(32).IsRequired();
        builder.Property(x => x.Country).HasMaxLength(64).IsRequired();
        builder.Property(x => x.Provider).HasMaxLength(64);
        builder.Property(x => x.TrackingNumber).HasMaxLength(128);
    }
}
