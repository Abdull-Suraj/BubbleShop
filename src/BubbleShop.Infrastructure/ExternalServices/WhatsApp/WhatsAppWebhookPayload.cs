using System.Text.Json.Serialization;

namespace BubbleShop.Infrastructure.ExternalServices.WhatsApp;

public sealed class WhatsAppWebhookPayload
{
    [JsonPropertyName("object")]
    public string Object { get; set; } = string.Empty;

    [JsonPropertyName("entry")]
    public List<Entry> Entry { get; set; } = [];
}

public sealed class Entry
{
    [JsonPropertyName("changes")]
    public List<Change> Changes { get; set; } = [];
}

public sealed class Change
{
    [JsonPropertyName("value")]
    public ChangeValue Value { get; set; } = new();
}

public sealed class ChangeValue
{
    [JsonPropertyName("messages")]
    public List<Message> Messages { get; set; } = [];
}

public sealed class Message
{
    [JsonPropertyName("from")]
    public string From { get; set; } = string.Empty;

    [JsonPropertyName("text")]
    public MessageText Text { get; set; } = new();

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
}

public sealed class MessageText
{
    [JsonPropertyName("body")]
    public string Body { get; set; } = string.Empty;
}
