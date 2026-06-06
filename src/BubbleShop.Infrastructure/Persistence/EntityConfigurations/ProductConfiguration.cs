using BubbleShop.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BubbleShop.Infrastructure.Persistence.EntityConfigurations;

public sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(128).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(1024);
        builder.Property(x => x.Price).HasPrecision(18, 2);
        builder.Property(x => x.ImageUrl).HasMaxLength(2048);
    }
}
