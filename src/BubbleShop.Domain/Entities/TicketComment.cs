// Domain/Entities/TicketComment.cs
namespace BubbleShop.Domain.Entities;

// Ticket Comment Entity
public class TicketComment
{
    public Guid Id { get; set; }
    public string Message { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public bool IsInternal { get; set; }
    public DateTime CreatedAt { get; set; }
}