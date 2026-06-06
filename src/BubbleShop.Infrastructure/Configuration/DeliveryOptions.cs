namespace BubbleShop.Infrastructure.Configuration;

public sealed class DeliveryOptions
{
    public const string SectionName = "Delivery";
    public string ProviderApiUrl { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
}
