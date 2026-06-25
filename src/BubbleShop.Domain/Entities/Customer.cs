// Domain/Entities/Customer.cs
using BubbleShop.Domain.Common;
using BubbleShop.Domain.Enums;
using BubbleShop.Domain.Exceptions;

namespace BubbleShop.Domain.Entities;

public sealed class Customer : BaseEntity
{
    private Customer()
    {
    }

    public Customer( 
        string whatsappNumber, 
        string name, 
        string? email =null, 
         string ? phoneNumber = null,
        Guid? businessId = null)
    {
        Id = Guid.NewGuid();
        WhatsAppNumber = whatsappNumber;
        Name = name;
        Email = email;
        BusinessId = businessId;
        PhoneNumber = phoneNumber ?? WhatsAppNumber;
        Status = CustomerStatus.Active;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = CreatedAt;
        Notes = string.Empty;
        TotalOrders = 0;
        TotalSpent = 0;
    }

    // Properties
    //public Guid Id { get; private set; }
    public Guid? BusinessId { get; private set; }
    public string PhoneNumber { get; private set; } = string.Empty;
    public string WhatsAppNumber { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? Email { get; private set; }
    public string? Address { get; private set; }
    public string? City { get; private set; }
    public string? State { get; private set; }
    public CustomerStatus Status { get; private set; }
    public string Notes { get; private set; } = string.Empty;
    public int TotalOrders { get; private set; }
    public decimal TotalSpent { get; private set; }
    public DateTimeOffset? LastOrderDate { get; private set; }
    //public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    // Navigation Properties
    public ICollection<Order> Orders { get; private set; } = [];
    public Business? Business { get; private set; }

    //// Factory Methods
    //public static Customer Create(Id, string whatsappNumber, string name, string? email)
    //    => new(Guid.NewGuid(), whatsappNumber, name, email);

    //public static Customer Create(string whatsappNumber, string name, string? email,  Guid businessId)
    //    => new(Guid.NewGuid(), whatsappNumber, name, email, businessId);

    // Update Methods
    public void Update(string name, string? email, string? address)
    {
        Name = name;
        Email = email;
        Address = address;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Name cannot be empty");

        Name = name;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void UpdateEmail(string? email)
    {
        Email = email;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void UpdateAddress(string? address, string? city = null, string? state = null, string? country = null, string? postalCode = null)
    {
        Address = address;
        City = city ?? City;
        State = state ?? State;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateWhatsAppNumber(string whatsAppNumber)
    {
        if (string.IsNullOrWhiteSpace(whatsAppNumber))
            throw new DomainException("WhatsApp number cannot be empty");

        WhatsAppNumber = whatsAppNumber;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    // Status Methods
    public void Block(string? reason = null)
    {
        if (Status == CustomerStatus.Blocked)
            throw new DomainException("Customer is already blocked.");

        Status = CustomerStatus.Blocked;
        UpdatedAt = DateTimeOffset.UtcNow;

        if (!string.IsNullOrEmpty(reason))
        {
            AddNote($"Blocked: {reason}");
        }
        else
        {
            AddNote("Blocked");
        }
    }

    public void Unblock()
    {
        if (Status != CustomerStatus.Blocked)
            throw new DomainException("Customer is not blocked.");

        Status = CustomerStatus.Active;
        UpdatedAt = DateTimeOffset.UtcNow;
        AddNote("Unblocked");
    }

    public void Activate()
    {
        if (Status == CustomerStatus.Active)
            throw new DomainException("Customer is already active.");

        Status = CustomerStatus.Active;
        UpdatedAt = DateTimeOffset.UtcNow;
        AddNote("Activated");
    }

    public void Deactivate()
    {
        if (Status == CustomerStatus.Inactive)
            throw new DomainException("Customer is already inactive.");

        Status = CustomerStatus.Inactive;
        UpdatedAt = DateTimeOffset.UtcNow;
        AddNote("Deactivated");
    }

    // Note Methods
    public void AddNote(string note)
    {
        if (string.IsNullOrWhiteSpace(note))
            return;

        var timestamp = DateTimeOffset.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
        var formattedNote = $"[{timestamp}] {note}";

        Notes = string.IsNullOrEmpty(Notes)
            ? formattedNote
            : $"{Notes}\n{formattedNote}";

        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void ClearNotes()
    {
        Notes = string.Empty;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    // Order Tracking Methods
    public void RecordOrder(decimal amount)
    {
        TotalOrders++;
        TotalSpent += amount;
        LastOrderDate = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    // Helper Properties
    public string FullName => Name;
    public bool IsActive => Status == CustomerStatus.Active;
    public bool IsBlocked => Status == CustomerStatus.Blocked;
    public bool IsInactive => Status == CustomerStatus.Inactive;

    public void AssignToBusiness(Guid businessId)
    {
        BusinessId = businessId;
        UpdatedAt = DateTime.UtcNow;
    }

    public override string ToString()
    {
        return $"{Name} ({WhatsAppNumber})";
    }
}

