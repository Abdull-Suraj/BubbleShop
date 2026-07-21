using BubbleShop.Domain.Common;
using BubbleShop.Domain.Enums;
using BubbleShop.Domain.Exceptions;

namespace BubbleShop.Domain.Entities;

public class Channel : BaseEntity
{
    public Guid BusinessId { get; private set; }
    public ChannelType ChannelType { get; private set; }

    public string? WebhookUrl { get; private set; }
    public string? ApiKey { get; private set; }

    public bool IsActive { get; private set; }
    public bool IsVerified { get; private set; }

    public DateTime? LastActiveAt { get; private set; }
    public DateTime? VerifiedAt { get; private set; }

    public Dictionary<string, string> Configuration { get; private set; } = new();

    public string? ChannelUsername { get; private set; }
    public string? ChannelId { get; private set; }

    // Navigation Property
    public Business Business { get; private set; } = null!;

    private Channel()
    {
    }

    public Channel(
        Guid businessId,
        ChannelType channelType,
        string? webhookUrl = null,
        string? apiKey = null,
        bool isActive = true)
    {
        Id = Guid.NewGuid();
        BusinessId = businessId;
        ChannelType = channelType;
        WebhookUrl = webhookUrl;
        ApiKey = apiKey;
        IsActive = isActive;
        IsVerified = false;

        CreatedAt = DateTime.UtcNow;
        LastModifiedAt = CreatedAt;

        Configuration = new Dictionary<string, string>();
    }

    public void Activate()
    {
        if (IsActive)
            return;

        IsActive = true;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        if (!IsActive)
            return;

        IsActive = false;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void Verify()
    {
        if (IsVerified)
            return;

        IsVerified = true;
        VerifiedAt = DateTime.UtcNow;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void RecordActivity()
    {
        LastActiveAt = DateTime.UtcNow;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void UpdateWebhookUrl(string? webhookUrl)
    {
        WebhookUrl = webhookUrl;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void UpdateApiKey(string? apiKey)
    {
        ApiKey = apiKey;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void UpdateConfiguration(Dictionary<string, string> configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        Configuration = new Dictionary<string, string>(configuration);
        LastModifiedAt = DateTime.UtcNow;
    }

    public void SetConfiguration(string key, string value)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new DomainException("Configuration key cannot be empty.");

        Configuration[key] = value;
        LastModifiedAt = DateTime.UtcNow;
    }

    public string? GetConfiguration(string key)
    {
        return Configuration.TryGetValue(key, out var value)
            ? value
            : null;
    }

    public void RemoveConfiguration(string key)
    {
        if (Configuration.Remove(key))
        {
            LastModifiedAt = DateTime.UtcNow;
        }
    }

    public void SetChannelId(string channelId, string? username = null)
    {
        if (string.IsNullOrWhiteSpace(channelId))
            throw new DomainException("Channel Id cannot be empty.");

        ChannelId = channelId;
        ChannelUsername = username;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void UpdateUsername(string? username)
    {
        ChannelUsername = username;
        LastModifiedAt = DateTime.UtcNow;
    }

    public override string ToString()
    {
        return $"{ChannelType} ({BusinessId})";
    }
}