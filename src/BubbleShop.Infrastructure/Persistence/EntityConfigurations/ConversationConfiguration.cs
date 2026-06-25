using BubbleShop.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BubbleShop.Infrastructure.Persistence.EntityConfigurations;

public sealed class ConversationConfiguration : IEntityTypeConfiguration<Conversation>
{
    public void Configure(EntityTypeBuilder<Conversation> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.WhatsAppNumber)
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(x => x.CustomerName)
            .HasMaxLength(200);

        builder.HasMany(x => x.Messages)
            .WithOne()
            .HasForeignKey("ConversationId")
            .OnDelete(DeleteBehavior.Cascade);
    }
}