
namespace BubbleShop.Domain.Common;

public class CarouselItem
{
    public string Title { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public List<InteractiveButton> Buttons { get; set; } = new();
}
