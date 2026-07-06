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

        builder.HasOne(c => c.Customer)
            .WithMany(cu => cu.Conversations)
            .HasForeignKey(c => c.CustomerId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(c => c.Business)
            .WithMany(b => b.Conversations)
            .HasForeignKey(c => c.BusinessId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}