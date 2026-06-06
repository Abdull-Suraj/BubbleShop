using BubbleShop.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;

namespace BubbleShop.Infrastructure.Persistence.EntityConfigurations;

public sealed class ConversationConfiguration : IEntityTypeConfiguration<Conversation>
{
    public void Configure(EntityTypeBuilder<Conversation> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.WhatsAppNumber).HasMaxLength(32).IsRequired();
        var messageHistoryProperty = builder.Property(x => x.MessageHistory)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<List<ChatMessage>>(v, (JsonSerializerOptions?)null) ?? new List<ChatMessage>());

        messageHistoryProperty.Metadata.SetValueComparer(new ValueComparer<List<ChatMessage>>(
                (left, right) => JsonSerializer.Serialize(left, (JsonSerializerOptions?)null) == JsonSerializer.Serialize(right, (JsonSerializerOptions?)null),
                list => JsonSerializer.Serialize(list, (JsonSerializerOptions?)null).GetHashCode(),
                list => list.Select(item => new ChatMessage
                {
                    Role = item.Role,
                    Content = item.Content,
                    Timestamp = item.Timestamp
                }).ToList()));
        messageHistoryProperty.HasColumnType("nvarchar(max)");
    }
}
