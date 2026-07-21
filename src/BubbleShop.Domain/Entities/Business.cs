using BubbleShop.Domain.Common;
using BubbleShop.Domain.DomainEvents;
using BubbleShop.Domain.Enums;
using BubbleShop.Domain.Exceptions;
using MediatR;

namespace BubbleShop.Domain.Entities;

public class Business : BaseEntity
{

    public string BusinessName { get; private set; } = string.Empty;
    public string LegalName { get; private set; } = string.Empty;
    public string RegistrationNumber { get; private set; } = string.Empty;
    public string TaxId { get; private set; } = string.Empty;

    public string Email { get; private set; } = string.Empty;
    public string PhoneNumber { get; private set; } = string.Empty;
    public string WhatsAppNumber { get; private set; } = string.Empty;

   
    public string Address { get; private set; } = string.Empty;
    public string City { get; private set; } = string.Empty;
    public string State { get; private set; } = string.Empty;
    public string Country { get; private set; } = string.Empty;
    public string PostalCode { get; private set; } = string.Empty;

    public string PasswordHash { get; private set; } = string.Empty;

    public BusinessStatus Status { get; private set; }
    public bool IsVerified { get; private set; }
    public DateTime? VerifiedAt { get; private set; }


    public decimal WalletBalance { get; private set; }
    public string Currency { get; private set; } = "USD";
    public decimal CommissionRate { get; private set; } = 0.10m; // 10% platform commission

    public BusinessSettings Settings { get; private set; } = new();

    private readonly List<Product> _products = [];
    private readonly List<Customer> _customers = [];
    private readonly List<Order> _orders = [];
    private readonly List<Payment> _payments = [];
    private readonly List<AutomationRule> _automationRules = [];
    private readonly List<Conversation> _conversations = [];
    private readonly List<Channel> _channels = [];

    public IReadOnlyCollection<Channel> Channels => _channels;
    public IReadOnlyCollection<Conversation> Conversations
        => _conversations;
    public IReadOnlyCollection<Product> Products => _products;
    public IReadOnlyCollection<Customer> Customers => _customers;
    public IReadOnlyCollection<Order> Orders => _orders;
    public IReadOnlyCollection<Payment> Payments => _payments;
   public IReadOnlyCollection<AutomationRule> AutomationRules => _automationRules;

    private Business() { }


    public Business(
        string businessName,
        string email,
        string whatsAppNumber,
        string passwordHash,
        string? phoneNumber = null,
        string? address = null)
    {
        Id = Guid.NewGuid();
        BusinessName = businessName ?? throw new ArgumentNullException(nameof(businessName));
        Email = email ?? throw new ArgumentNullException(nameof(email));
        WhatsAppNumber = whatsAppNumber ?? throw new ArgumentNullException(nameof(whatsAppNumber));
        PasswordHash = passwordHash ?? throw new ArgumentNullException(nameof(passwordHash));

        PhoneNumber = phoneNumber ?? whatsAppNumber;
        Address = address ?? string.Empty;
        Status = BusinessStatus.Pending;
        IsVerified = false;
        WalletBalance = 0;
        CommissionRate = 0.10m;
        Currency = "USD";
        Settings = new BusinessSettings();
        //DeliverySettings = new BusinessDeliverySettings();
        CreatedAt = DateTime.UtcNow;
    }

    // ============ UPDATE METHODS ============


    public void UpdateProfile(
        string businessName,
        string email,
        string whatsAppNumber,
        string? phoneNumber = null,
        string? address = null,
        string? city = null,
        string? state = null,
        string? country = null,
        string? postalCode = null)
    {
        if (!string.IsNullOrEmpty(businessName))
            BusinessName = businessName;

        if (!string.IsNullOrEmpty(email))
            Email = email;

        if (!string.IsNullOrEmpty(whatsAppNumber))
            WhatsAppNumber = whatsAppNumber;

        if (!string.IsNullOrEmpty(phoneNumber))
            PhoneNumber = phoneNumber;

        if (!string.IsNullOrEmpty(address))
            Address = address;

        if (!string.IsNullOrEmpty(city))
            City = city;

        if (!string.IsNullOrEmpty(state))
            State = state;

        if (!string.IsNullOrEmpty(country))
            Country = country;

        if (!string.IsNullOrEmpty(postalCode))
            PostalCode = postalCode;

        LastModifiedAt = DateTime.UtcNow;
    }


    public void UpdateBusinessName(string businessName)
    {
        if (string.IsNullOrWhiteSpace(businessName))
            throw new DomainException("Business name cannot be empty");

        BusinessName = businessName;
        LastModifiedAt = DateTime.UtcNow;
    }


    public void UpdateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new DomainException("Email cannot be empty");

        Email = email;
        LastModifiedAt = DateTime.UtcNow;
    }


    public void UpdateWhatsAppNumber(string whatsAppNumber)
    {
        if (string.IsNullOrWhiteSpace(whatsAppNumber))
            throw new DomainException("WhatsApp number cannot be empty");

        WhatsAppNumber = whatsAppNumber;
        LastModifiedAt = DateTime.UtcNow;
    }


    public void UpdatePhoneNumber(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            throw new DomainException("Phone number cannot be empty");

        PhoneNumber = phoneNumber;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void UpdateAddress(string address, string? city = null, string? state = null, string? country = null, string? postalCode = null)
    {
        if (!string.IsNullOrEmpty(address))
            Address = address;

        if (!string.IsNullOrEmpty(city))
            City = city;

        if (!string.IsNullOrEmpty(state))
            State = state;

        if (!string.IsNullOrEmpty(country))
            Country = country;

        if (!string.IsNullOrEmpty(postalCode))
            PostalCode = postalCode;

        LastModifiedAt = DateTime.UtcNow;
    }

    public void UpdateLegalName(string legalName)
    {
        if (!string.IsNullOrEmpty(legalName))
            LegalName = legalName;
        LastModifiedAt = DateTime.UtcNow;
    }


    public void UpdateRegistrationNumber(string registrationNumber)
    {
        if (!string.IsNullOrEmpty(registrationNumber))
            RegistrationNumber = registrationNumber;
        LastModifiedAt = DateTime.UtcNow;
    }


    public void UpdateTaxId(string taxId)
    {
        if (!string.IsNullOrEmpty(taxId))
            TaxId = taxId;
        LastModifiedAt = DateTime.UtcNow;
    }

    // ============ PASSWORD METHODS ============


    public void UpdatePassword(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new DomainException("Password hash cannot be empty.");

        PasswordHash = passwordHash;
        LastModifiedAt = DateTime.UtcNow;
    }
    public bool VerifyPassword(string password, Func<string, string> hashProvider)
    {
        if (string.IsNullOrWhiteSpace(password))
            return false;

        var hashedPassword = hashProvider(password);
        return PasswordHash == hashedPassword;
    }

    // ============ STATUS METHODS ============

    public void Verify()
    {
        Status = BusinessStatus.Active;
        IsVerified = true;
        VerifiedAt = DateTime.UtcNow;
        LastModifiedAt = DateTime.UtcNow;

        AddDomainEvent(new BusinessVerifiedEvent(Id, BusinessName));
    }


    public void Suspend()
    {
        Status = BusinessStatus.Suspended;
        LastModifiedAt = DateTime.UtcNow;

        AddDomainEvent(new BusinessSuspendedEvent(Id, BusinessName));
    }

    public void Activate()
    {
        Status = BusinessStatus.Active;
        LastModifiedAt = DateTime.UtcNow;

        AddDomainEvent(new BusinessActivatedEvent(Id, BusinessName));
    }

  
    public void Ban()
    {
        Status = BusinessStatus.Banned;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void AddToWallet(decimal amount, string? description = null)
    {
        if (amount <= 0)
            throw new DomainException("Amount must be positive");

        WalletBalance += amount;
        LastModifiedAt = DateTime.UtcNow;

        AddDomainEvent(new WalletCreditedEvent(Id, amount, WalletBalance, description));
    }


    public void DeductFromWallet(decimal amount, string? description = null)
    {
        if (amount <= 0)
            throw new DomainException("Amount must be positive");

        if (WalletBalance < amount)
            throw new DomainException($"Insufficient wallet balance. Available: {WalletBalance:C}");

        WalletBalance -= amount;
        LastModifiedAt = DateTime.UtcNow;

        AddDomainEvent(new WalletDebitedEvent(Id, amount, WalletBalance, description));
    }

    public void UpdateSettings(BusinessSettings settings)
    {
        Settings = settings ?? throw new ArgumentNullException(nameof(settings));
        LastModifiedAt = DateTime.UtcNow;
    }


    public void UpdateCommissionRate(decimal rate)
    {
        if (rate < 0 || rate > 100)
            throw new DomainException("Commission rate must be between 0 and 100");

        CommissionRate = rate;
        LastModifiedAt = DateTime.UtcNow;
    }


    public void UpdateCurrency(string currency)
    {
        if (string.IsNullOrWhiteSpace(currency))
            throw new DomainException("Currency cannot be empty");

        Currency = currency.ToUpperInvariant();
        LastModifiedAt = DateTime.UtcNow;
    }

    public bool CanAcceptOrders => Status == BusinessStatus.Active && IsVerified;


    public bool IsActive => Status == BusinessStatus.Active;


    public string DisplayName => !string.IsNullOrEmpty(LegalName) ? LegalName : BusinessName;


    public override string ToString()
    {
        return $"{BusinessName} ({Email})";
    }
    // ============ CHANNEL METHODS ============

    public Channel RegisterChannel(
        ChannelType channelType,
        string? webhookUrl = null,
        string? apiKey = null,
        bool isActive = true)
    {
        if (_channels.Any(c => c.ChannelType == channelType))
            throw new DomainException($"{channelType} channel is already registered.");

        var channel = new Channel(
            Id,
            channelType,
            webhookUrl,
            apiKey,
            isActive);

        _channels.Add(channel);
        LastModifiedAt = DateTime.UtcNow;

        return channel;
    }

    public void RemoveChannel(ChannelType channelType)
    {
        var channel = _channels.FirstOrDefault(c => c.ChannelType == channelType);

        if (channel is null)
            throw new DomainException($"{channelType} channel not found.");

        _channels.Remove(channel);
        LastModifiedAt = DateTime.UtcNow;
    }

    public Channel GetChannel(ChannelType channelType)
    {
        var channel = _channels.FirstOrDefault(c => c.ChannelType == channelType);

        if (channel is null)
            throw new DomainException($"{channelType} channel not found.");

        return channel;
    }

    public bool HasChannel(ChannelType channelType)
    {
        return _channels.Any(c => c.ChannelType == channelType);
    }

    public void ActivateChannel(ChannelType channelType)
    {
        GetChannel(channelType).Activate();
        LastModifiedAt = DateTime.UtcNow;
    }

    public void DeactivateChannel(ChannelType channelType)
    {
        GetChannel(channelType).Deactivate();
        LastModifiedAt = DateTime.UtcNow;
    }

    public void VerifyChannel(ChannelType channelType)
    {
        GetChannel(channelType).Verify();
        LastModifiedAt = DateTime.UtcNow;
    }

    public void UpdateChannelWebhook(ChannelType channelType, string webhookUrl)
    {
        GetChannel(channelType).UpdateWebhookUrl(webhookUrl);
        LastModifiedAt = DateTime.UtcNow;
    }

    public void UpdateChannelApiKey(ChannelType channelType, string apiKey)
    {
        GetChannel(channelType).UpdateApiKey(apiKey);
        LastModifiedAt = DateTime.UtcNow;
    }

    public void RecordChannelActivity(ChannelType channelType)
    {
        GetChannel(channelType).RecordActivity();
    }
}

