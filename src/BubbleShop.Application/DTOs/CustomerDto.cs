// Application/DTOs/CustomerDto.cs
namespace BubbleShop.Application.DTOs;

public class CustomerDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string WhatsAppNumber { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public int TotalOrders { get; set; }
    public decimal TotalSpent { get; set; }
    public DateTimeOffset? LastOrderDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    // Default constructor
    public CustomerDto() { }

    // Constructor with 5 parameters
    public CustomerDto(Guid id, string whatsAppNumber, string name, string? email, string? address)
    {
        Id = id;
        WhatsAppNumber = whatsAppNumber;
        Name = name;
        Email = email;
        Address = address;
        Status = "Active";
        CreatedAt = DateTime.UtcNow;
    }

    // Constructor with all parameters
    public CustomerDto(
        Guid id,
        string name,
        string phoneNumber,
        string whatsAppNumber,
        string? email,
        string? address,
        string? city,
        string? state,
        int totalOrders,
        decimal totalSpent,
        DateTime? lastOrderDate,
        string status,
        DateTime createdAt)
    {
        Id = id;
        Name = name;
        PhoneNumber = phoneNumber;
        WhatsAppNumber = whatsAppNumber;
        Email = email;
        Address = address;
        City = city;
        State = state;
        TotalOrders = totalOrders;
        TotalSpent = totalSpent;
        LastOrderDate = lastOrderDate;
        Status = status;
        CreatedAt = createdAt;
    }
}