using BubbleShop.Domain.Common;

public class InteractiveMessage
{
    public string Title { get; set; } = string.Empty;

    public string Text { get; set; } = string.Empty;

    public string? Footer { get; set; }

    public List<InteractiveButton> Buttons { get; set; } = [];

    public List<string> QuickReplies { get; set; } = [];

    public CarouselItem? Carousel { get; set; }

    public bool IsTyping { get; set; }

    public Dictionary<string, string>? Metadata { get; set; }
}