using BubbleShop.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;

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
        builder.Property(p => p.CompareAtPrice)
    .HasPrecision(18, 2);

        builder.Property(p => p.Cost)
            .HasPrecision(18, 2);

        builder.Property(p => p.Images)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new());

        builder.Property(p => p.Tags)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new());
        builder.HasOne(p => p.Business)
        .WithMany(b => b.Products)
        .HasForeignKey(p => p.BusinessId)
        .OnDelete(DeleteBehavior.Cascade);
    }
}
