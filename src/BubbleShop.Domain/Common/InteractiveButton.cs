
namespace BubbleShop.Domain.Common;

public class InteractiveButton
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public ButtonAction Action { get; set; }
    public string? Url { get; set; }
    public string? Payload { get; set; }
}
