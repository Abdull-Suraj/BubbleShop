using BubbleShop.Domain.Common;
using BubbleShop.Domain.DomainEvents;
using BubbleShop.Domain.Enums;
using BubbleShop.Domain.Exceptions;
using MediatR;

namespace BubbleShop.Domain.Entities;

public class Business : BaseEntity
{
    // ============ BASIC INFORMATION ============
    public string BusinessName { get; private set; } = string.Empty;
    public string LegalName { get; private set; } = string.Empty;
    public string RegistrationNumber { get; private set; } = string.Empty;
    public string TaxId { get; private set; } = string.Empty;

    // ============ CONTACT INFORMATION ============
    public string Email { get; private set; } = string.Empty;
    public string PhoneNumber { get; private set; } = string.Empty;
    public string WhatsAppNumber { get; private set; } = string.Empty;

    // ============ ADDRESS ============
    public string Address { get; private set; } = string.Empty;
    public string City { get; private set; } = string.Empty;
    public string State { get; private set; } = string.Empty;
    public string Country { get; private set; } = string.Empty;
    public string PostalCode { get; private set; } = string.Empty;

    // ============ AUTHENTICATION ============
    public string PasswordHash { get; private set; } = string.Empty;

    // ============ BUSINESS STATUS ============
    public BusinessStatus Status { get; private set; }
    public bool IsVerified { get; private set; }
    public DateTime? VerifiedAt { get; private set; }

    // ============ FINANCIAL ============
    public decimal WalletBalance { get; private set; }
    public string Currency { get; private set; } = "USD";
    public decimal CommissionRate { get; private set; } = 0.10m; // 10% platform commission

    // ============ SETTINGS ============
    public BusinessSettings Settings { get; private set; } = new();
    //public BusinessDeliverySettings DeliverySettings { get; private set; } = new();

    // ============ NAVIGATION PROPERTIES ============
    private readonly List<Product> _products = new();
    private readonly List<Customer> _customers = new();
    private readonly List<Order> _orders = new();
    private readonly List<Payment> _payments = new();
    //private readonly List<Delivery> _deliveries = new();
    private readonly List<AutomationRule> _automationRules = new();

    public IReadOnlyCollection<Product> Products => _products.AsReadOnly();
    public IReadOnlyCollection<Customer> Customers => _customers.AsReadOnly();
    public IReadOnlyCollection<Order> Orders => _orders.AsReadOnly();
    public IReadOnlyCollection<Payment> Payments => _payments.AsReadOnly();
    //public IReadOnlyCollection<Delivery> Deliveries => _deliveries.AsReadOnly();
    public IReadOnlyCollection<AutomationRule> AutomationRules => _automationRules.AsReadOnly();

    private Business() { }

    /// <summary>
    /// Main constructor for creating a new business
    /// </summary>
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

    /// <summary>
    /// Update business profile information
    /// </summary>
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

    /// <summary>
    /// Update business name
    /// </summary>
    public void UpdateBusinessName(string businessName)
    {
        if (string.IsNullOrWhiteSpace(businessName))
            throw new DomainException("Business name cannot be empty");

        BusinessName = businessName;
        LastModifiedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Update email address
    /// </summary>
    public void UpdateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new DomainException("Email cannot be empty");

        Email = email;
        LastModifiedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Update WhatsApp number
    /// </summary>
    public void UpdateWhatsAppNumber(string whatsAppNumber)
    {
        if (string.IsNullOrWhiteSpace(whatsAppNumber))
            throw new DomainException("WhatsApp number cannot be empty");

        WhatsAppNumber = whatsAppNumber;
        LastModifiedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Update phone number
    /// </summary>
    public void UpdatePhoneNumber(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            throw new DomainException("Phone number cannot be empty");

        PhoneNumber = phoneNumber;
        LastModifiedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Update address
    /// </summary>
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

    /// <summary>
    /// Update legal name
    /// </summary>
    public void UpdateLegalName(string legalName)
    {
        if (!string.IsNullOrEmpty(legalName))
            LegalName = legalName;
        LastModifiedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Update registration number
    /// </summary>
    public void UpdateRegistrationNumber(string registrationNumber)
    {
        if (!string.IsNullOrEmpty(registrationNumber))
            RegistrationNumber = registrationNumber;
        LastModifiedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Update tax ID
    /// </summary>
    public void UpdateTaxId(string taxId)
    {
        if (!string.IsNullOrEmpty(taxId))
            TaxId = taxId;
        LastModifiedAt = DateTime.UtcNow;
    }

    // ============ PASSWORD METHODS ============

    /// <summary>
    /// Update password hash
    /// </summary>
    public void UpdatePassword(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new DomainException("Password hash cannot be empty");

        PasswordHash = passwordHash;
        LastModifiedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Verify password
    /// </summary>
    public bool VerifyPassword(string password, Func<string, string> hashProvider)
    {
        if (string.IsNullOrWhiteSpace(password))
            return false;

        var hashedPassword = hashProvider(password);
        return PasswordHash == hashedPassword;
    }

    // ============ STATUS METHODS ============

    /// <summary>
    /// Verify the business
    /// </summary>
    public void Verify()
    {
        Status = BusinessStatus.Active;
        IsVerified = true;
        VerifiedAt = DateTime.UtcNow;
        LastModifiedAt = DateTime.UtcNow;

        AddDomainEvent(new BusinessVerifiedEvent(Id, BusinessName));
    }

    /// <summary>
    /// Suspend the business
    /// </summary>
    public void Suspend()
    {
        Status = BusinessStatus.Suspended;
        LastModifiedAt = DateTime.UtcNow;

        AddDomainEvent(new BusinessSuspendedEvent(Id, BusinessName));
    }

    /// <summary>
    /// Activate the business
    /// </summary>
    public void Activate()
    {
        Status = BusinessStatus.Active;
        LastModifiedAt = DateTime.UtcNow;

        AddDomainEvent(new BusinessActivatedEvent(Id, BusinessName));
    }

    /// <summary>
    /// Ban the business
    /// </summary>
    public void Ban()
    {
        Status = BusinessStatus.Banned;
        LastModifiedAt = DateTime.UtcNow;
    }

    // ============ WALLET METHODS ============

    /// <summary>
    /// Add funds to wallet
    /// </summary>
    public void AddToWallet(decimal amount, string? description = null)
    {
        if (amount <= 0)
            throw new DomainException("Amount must be positive");

        WalletBalance += amount;
        LastModifiedAt = DateTime.UtcNow;

        AddDomainEvent(new WalletCreditedEvent(Id, amount, WalletBalance, description));
    }

    /// <summary>
    /// Deduct funds from wallet
    /// </summary>
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

    // ============ SETTINGS METHODS ============

    /// <summary>
    /// Update business settings
    /// </summary>
    public void UpdateSettings(BusinessSettings settings)
    {
        Settings = settings ?? throw new ArgumentNullException(nameof(settings));
        LastModifiedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Update delivery settings
    /// </summary>
    //public void UpdateDeliverySettings(BusinessDeliverySettings deliverySettings)
    //{
    //    DeliverySettings = deliverySettings ?? throw new ArgumentNullException(nameof(deliverySettings));
    //    LastModifiedAt = DateTime.UtcNow;
    //}

    /// <summary>
    /// Update commission rate
    /// </summary>
    public void UpdateCommissionRate(decimal rate)
    {
        if (rate < 0 || rate > 100)
            throw new DomainException("Commission rate must be between 0 and 100");

        CommissionRate = rate;
        LastModifiedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Update currency
    /// </summary>
    public void UpdateCurrency(string currency)
    {
        if (string.IsNullOrWhiteSpace(currency))
            throw new DomainException("Currency cannot be empty");

        Currency = currency.ToUpperInvariant();
        LastModifiedAt = DateTime.UtcNow;
    }

    // ============ BUSINESS METHODS ============

    /// <summary>
    /// Check if business can accept orders
    /// </summary>
    public bool CanAcceptOrders => Status == BusinessStatus.Active && IsVerified;

    /// <summary>
    /// Check if business is active
    /// </summary>
    public bool IsActive => Status == BusinessStatus.Active;

    /// <summary>
    /// Get business display name
    /// </summary>
    public string DisplayName => !string.IsNullOrEmpty(LegalName) ? LegalName : BusinessName;

    // ============ OVERRIDES ============

    public override string ToString()
    {
        return $"{BusinessName} ({Email})";
    }
}
