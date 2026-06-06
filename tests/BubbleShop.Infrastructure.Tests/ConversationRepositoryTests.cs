using BubbleShop.Domain.Entities;

namespace BubbleShop.Infrastructure.Tests;

public sealed class ConversationRepositoryTests
{
    [Fact]
    public void Conversation_UpdateHistory_ShouldUpdateTimestampAndMessages()
    {
        var conversation = Conversation.Create(Guid.NewGuid(), "123");
        var messages = new List<ChatMessage> { new() { Content = "hi" } };

        conversation.UpdateHistory(messages);

        Assert.Single(conversation.MessageHistory);
        Assert.Equal("hi", conversation.MessageHistory[0].Content);
    }
}
