namespace BubbleShop.Infrastructure.Configuration;

public sealed class AzureOpenAIOptions
{
    public const string SectionName = "AzureOpenAI";
    public string Endpoint { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string DeploymentName { get; set; } = "gpt-4o";
    public int MaxTokens { get; set; } = 2000;
}
