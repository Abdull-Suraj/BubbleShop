using BubbleShop.Domain.Enums;

public class UserProfile
{
    public string Id { get; set; } = string.Empty;

    public ChannelType ChannelType { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public string? Username { get; set; }

    public string? ProfilePictureUrl { get; set; }

    public string? Language { get; set; }

    public bool IsVerified { get; set; }
}