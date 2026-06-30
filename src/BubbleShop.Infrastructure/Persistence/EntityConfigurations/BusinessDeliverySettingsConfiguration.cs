//using System.Text.Json;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.EntityFrameworkCore.ChangeTracking;
//using Microsoft.EntityFrameworkCore.Metadata.Builders;
//using BubbleShop.Domain.Entities;

//namespace BubbleShop.Infrastructure.Persistence.EntityConfigurations;

//public class BusinessDeliverySettingsConfiguration
//    : IEntityTypeConfiguration<BusinessDeliverySettings>
//{
//    public void Configure(EntityTypeBuilder<BusinessDeliverySettings> builder)
//    {
//        builder.HasKey(x => x.Id);

//        // DeliveryAreas
//        builder.Property(x => x.DeliveryAreas)
//            .HasConversion(
//                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
//                v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null)
//                     ?? new List<string>())
//            .Metadata.SetValueComparer(
//                new ValueComparer<List<string>>(
//                    (c1, c2) => JsonSerializer.Serialize(c1, (JsonSerializerOptions?)null)
//                              == JsonSerializer.Serialize(c2, (JsonSerializerOptions?)null),
//                    c => JsonSerializer.Serialize(c, (JsonSerializerOptions?)null).GetHashCode(),
//                    c => c.ToList()));

//        builder.Property(x => x.DeliveryAreas)
//            .HasColumnType("nvarchar(max)");

//        // ZoneDeliveryFees
//        builder.Property(x => x.ZoneDeliveryFees)
//            .HasConversion(
//                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
//                v => JsonSerializer.Deserialize<Dictionary<string, decimal>>(v, (JsonSerializerOptions?)null)
//                     ?? new Dictionary<string, decimal>())
//            .Metadata.SetValueComparer(
//                new ValueComparer<Dictionary<string, decimal>>(
//                    (d1, d2) => JsonSerializer.Serialize(d1, (JsonSerializerOptions?)null)
//                              == JsonSerializer.Serialize(d2, (JsonSerializerOptions?)null),
//                    d => JsonSerializer.Serialize(d, (JsonSerializerOptions?)null).GetHashCode(),
//                    d => d.ToDictionary(x => x.Key, x => x.Value)));

//        builder.Property(x => x.ZoneDeliveryFees)
//            .HasColumnType("nvarchar(max)");
//    }
//}