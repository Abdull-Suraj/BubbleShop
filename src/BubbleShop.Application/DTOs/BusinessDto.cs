namespace BubbleShop.Application.DTOs;

public class BusinessDto
{
    public Guid Id { get; set; }
    public string BusinessName { get; set; } = string.Empty;
    public string? LegalName { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string WhatsAppNumber { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? PostalCode { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal WalletBalance { get; set; }
    public string Currency { get; set; } = "NGN";
    public bool IsVerified { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}