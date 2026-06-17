namespace BubbleShop.Application.DTOs;

public class LoginResponseDto
{
    public string Token { get; set; } = string.Empty;
    public Guid BusinessId { get; set; }
    public string BusinessName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}