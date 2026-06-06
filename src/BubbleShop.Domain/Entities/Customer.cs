namespace BubbleShop.Domain.Entities;

public sealed class Customer : BaseEntity
{
    private Customer()
    {
    }

    private Customer(Guid id, string whatsappNumber, string name, string? email, string? address)
    {
        Id = id;
        WhatsAppNumber = whatsappNumber;
        Name = name;
        Email = email;
        Address = address;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }
    public string WhatsAppNumber { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? Email { get; private set; }
    public string? Address { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public ICollection<Order> Orders { get; private set; } = [];

    public static Customer Create(string whatsappNumber, string name, string? email, string? address)
        => new(Guid.NewGuid(), whatsappNumber, name, email, address);

    public void Update(string name, string? email, string? address)
    {
        Name = name;
        Email = email;
        Address = address;
    }
}
