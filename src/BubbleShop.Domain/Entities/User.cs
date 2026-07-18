
using BubbleShop.Domain.Common;
using BubbleShop.Domain.Enums;

namespace BubbleShop.Domain.Entities;

public class User : BaseEntity
{
    public Guid BusinessId { get; private set; }
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public UserRole Role { get; private set; }
    public bool IsActive { get; private set; }
    public string? PhoneNumber { get; private set; }
    public string? ProfileImageUrl { get; private set; }

    public string FullName => $"{FirstName} {LastName}".Trim();

    // Navigation Properties
    public Business Business { get; private set; } = null!;
    private readonly List<SupportTicket> _assignedTickets = new();
    public IReadOnlyCollection<SupportTicket> AssignedTickets => _assignedTickets.AsReadOnly();

    private User() { }

    public User(
        Guid businessId,
        string firstName,
        string lastName,
        string email,
        string passwordHash,
        UserRole role = UserRole.Agent)
    {
        FirstName = string.IsNullOrWhiteSpace(firstName)
    ? throw new ArgumentException(nameof(firstName))
    : firstName.Trim();

        LastName = string.IsNullOrWhiteSpace(lastName)
            ? throw new ArgumentException(nameof(lastName))
            : lastName.Trim();

        Email = string.IsNullOrWhiteSpace(email)
            ? throw new ArgumentException(nameof(email))
            : email.Trim().ToLowerInvariant();

        PasswordHash = string.IsNullOrWhiteSpace(passwordHash)
            ? throw new ArgumentException(nameof(passwordHash))
            : passwordHash;
        Id = Guid.NewGuid();
        BusinessId = businessId;
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        PasswordHash = passwordHash;
        Role = role;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }

    public void UpdateProfile(string firstName, string lastName, string? phoneNumber = null, string? profileImageUrl = null)
    {
        FirstName = firstName;
        LastName = lastName;
        PhoneNumber = phoneNumber;
        ProfileImageUrl = profileImageUrl;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void UpdateRole(UserRole role)
    {
        Role = role;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void UpdatePassword(string passwordHash)
    {
        PasswordHash = passwordHash;
        LastModifiedAt = DateTime.UtcNow;
    }

    public override string ToString()
    {
        return $"{FullName} ({Email})";
    }
}
