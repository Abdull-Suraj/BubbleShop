using BubbleShop.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;

namespace BubbleShop.Infrastructure.Persistence.EntityConfigurations
{



    public class AutomationRuleConfiguration : IEntityTypeConfiguration<AutomationRule>
    {
        public void Configure(EntityTypeBuilder<AutomationRule> builder)
        {
            builder.ToTable("AutomationRules");

            builder.HasKey(ar => ar.Id);

            builder.Property(ar => ar.TriggerKeyword)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(ar => ar.AutoReplyMessage)
                .IsRequired()
                .HasMaxLength(1000);

            builder.Property(ar => ar.Action)
                .HasConversion<string>()
                .HasMaxLength(50);



            builder.Property(ar => ar.IsActive)
                .HasDefaultValue(true);

            builder.Property(ar => ar.Priority)
                .HasDefaultValue(0);

            builder.Property(ar => ar.TimesTriggered)
                .HasDefaultValue(0);

            // Store ActiveDays as JSON
            builder.Property(ar => ar.ActiveDays)
      .HasConversion(
          v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),  // Explicit null for optional parameter
          v => JsonSerializer.Deserialize<List<DayOfWeek>>(v, (JsonSerializerOptions?)null) ?? new List<DayOfWeek>()
      )
      .HasColumnType("nvarchar(max)");

            builder.HasIndex(ar => new { ar.BusinessId, ar.TriggerKeyword })
                .IsUnique();

            builder.HasIndex(ar => ar.IsActive);
            builder.HasIndex(ar => ar.Action);

            builder.HasOne(ar => ar.Business)
                .WithMany(b => b.AutomationRules)
                .HasForeignKey(ar => ar.BusinessId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(ar => ar.AssociatedProduct)
                .WithMany()
                .HasForeignKey(ar => ar.AssociatedProductId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
