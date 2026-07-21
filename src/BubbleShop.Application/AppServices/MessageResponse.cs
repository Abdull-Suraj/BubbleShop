// Application/Services/MessageResponse.cs
using BubbleShop.Domain.Common;

namespace BubbleShop.Application.AppServices;

public class MessageResponse
{
    public bool IsSuccess { get; set; }
    public string Text { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
    public InteractiveMessage? Interactive { get; set; }
    public Guid? ConversationId { get; set; }
    public string? Intent { get; set; }


    public static MessageResponse Success(
           string text,
           InteractiveMessage? interactive = null,
           Guid? conversationId = null,
           string? intent = null)
    {
        return new MessageResponse
        {
            IsSuccess = true,
            Text = text,
            Interactive = interactive,
            ConversationId = conversationId,
            Intent = intent
        };
    }

    public static MessageResponse Error(string error)
    {
        return new MessageResponse
        {
            IsSuccess = false,
            ErrorMessage = error,
            Text = error
        };
    }
}