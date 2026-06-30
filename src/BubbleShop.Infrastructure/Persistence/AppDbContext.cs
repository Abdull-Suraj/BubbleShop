using BubbleShop.Application.Common.Interfaces;
using BubbleShop.Domain.Common;
using BubbleShop.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BubbleShop.Infrastructure.Persistence;

public sealed class AppDbContext : DbContext, IUnitOfWork
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Payment> Payments => Set<Payment>();
    //public DbSet<Delivery> Deliveries => Set<Delivery>();
    public DbSet<Conversation> Conversations => Set<Conversation>();
    public DbSet<Business> Businesses => Set<Business>();
    public DbSet<AutomationRule> AutomationRules => Set<AutomationRule>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Business Configuration
        modelBuilder.Entity<Business>(entity =>
        {
            entity.HasKey(b => b.Id);
            entity.HasIndex(b => b.Email).IsUnique();
            entity.HasIndex(b => b.WhatsAppNumber).IsUnique();

            entity.Property(b => b.BusinessName)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(b => b.Email)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(b => b.WhatsAppNumber)
                .IsRequired()
                .HasMaxLength(20);

            entity.Property(b => b.WalletBalance)
                .HasPrecision(18, 2);

            entity.Property(b => b.CommissionRate)
                .HasPrecision(5, 2);

            entity.Property(b => b.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            entity.Property(b => b.LastModifiedAt)
                .IsRequired(false);
        });

        // Product Configuration
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.HasIndex(p => p.SKU).IsUnique();
            entity.HasIndex(p => p.BusinessId);

            entity.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(p => p.Description)
                .HasMaxLength(2000);

            entity.Property(p => p.Price)
                .HasPrecision(18, 2);

            entity.Property(p => p.CompareAtPrice)
                .HasPrecision(18, 2);

            entity.Property(p => p.Cost)
                .HasPrecision(18, 2);

            entity.Property(p => p.StockQuantity)
                .IsRequired();

            entity.Property(p => p.ReservedQuantity)
                .HasDefaultValue(0);

            entity.Property(p => p.Category)
                .HasMaxLength(100);

            entity.Property(p => p.ThumbnailUrl)
                .HasMaxLength(500);

            //entity.Property(p => p.Status)
            //    .HasConversion<string>()
            //    .HasMaxLength(50);

            entity.Property(p => p.IsDigital)
                .HasDefaultValue(false);

            entity.Property(p => p.LowStockThreshold)
                .HasDefaultValue(10);

            // Store Tags as JSON
            entity.Property(p => p.Tags)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>()
                )
                .HasColumnType("nvarchar(max)");

            // Store Images as JSON
            entity.Property(p => p.Images)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>()
                )
                .HasColumnType("nvarchar(max)");

            // OwnsOne for Weight
            //entity.OwnsOne(p => p.Weight, weight =>
            //{
            //    weight.Property(w => w.Value)
            //        .HasColumnName("WeightValue")
            //        .HasPrecision(18, 2);
            //    weight.Property(w => w.Unit)
            //        .HasColumnName("WeightUnit")
            //        .HasMaxLength(10);
            //});

            // OwnsOne for Dimensions
            //entity.OwnsOne(p => p.Dimensions, dimensions =>
            //{
            //    dimensions.Property(d => d.Length)
            //        .HasColumnName("Length")
            //        .HasPrecision(18, 2);
            //    dimensions.Property(d => d.Width)
            //        .HasColumnName("Width")
            //        .HasPrecision(18, 2);
            //    dimensions.Property(d => d.Height)
            //        .HasColumnName("Height")
            //        .HasPrecision(18, 2);
            //    dimensions.Property(d => d.Unit)
            //        .HasColumnName("DimensionUnit")
            //        .HasMaxLength(10);
            //});

            entity.HasOne(p => p.Business)
                .WithMany(b => b.Products)
                .HasForeignKey(p => p.BusinessId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Customer Configuration
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.HasIndex(c => new { c.BusinessId, c.WhatsAppNumber }).IsUnique();
            entity.HasIndex(c => c.Email);

            entity.Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(c => c.Email)
                .HasMaxLength(200);

            entity.Property(c => c.PhoneNumber)
                .HasMaxLength(20);

            entity.Property(c => c.WhatsAppNumber)
                .IsRequired()
                .HasMaxLength(20);

            entity.Property(c => c.Address)
                .HasMaxLength(500);

            entity.Property(c => c.City)
                .HasMaxLength(100);

            entity.Property(c => c.State)
                .HasMaxLength(100);


            entity.Property(c => c.TotalOrders)
                .HasDefaultValue(0);

            entity.Property(c => c.TotalSpent)
                .HasPrecision(18, 2)
                .HasDefaultValue(0);

            entity.Property(c => c.Status)
                .HasConversion<string>()
                .HasMaxLength(50);

            entity.Property(c => c.Notes)
                .HasMaxLength(1000);

            entity.HasOne(c => c.Business)
                .WithMany(b => b.Customers)
                .HasForeignKey(c => c.BusinessId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Order Configuration
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(o => o.Id);
            entity.HasIndex(o => o.OrderNumber).IsUnique();
            entity.HasIndex(o => o.Status);
            entity.HasIndex(o => o.CustomerId);
            entity.HasIndex(o => o.BusinessId);
            entity.HasIndex(o => o.CreatedAt);

            entity.Property(o => o.OrderNumber)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(o => o.Subtotal)
                .HasPrecision(18, 2);

    

            entity.Property(o => o.DeliveryFee)
                .HasPrecision(18, 2);

            entity.Property(o => o.TotalAmount)
                .HasPrecision(18, 2);

            entity.Property(o => o.CustomerName)
                .HasMaxLength(200);

            entity.Property(o => o.CustomerEmail)
                .HasMaxLength(200);

            entity.Property(o => o.CustomerPhone)
                .HasMaxLength(20);

            entity.Property(o => o.CustomerWhatsApp)
                .HasMaxLength(20);

            entity.Property(o => o.ShippingAddress)
                .HasMaxLength(500);

            entity.Property(o => o.BillingAddress)
                .HasMaxLength(500);


            entity.Property(o => o.Channel)
                .HasMaxLength(50);

            entity.Property(o => o.CancellationReason)
                .HasMaxLength(500);

            // Store Metadata as JSON
            entity.Property(o => o.Metadata)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, (JsonSerializerOptions?)null) ?? new Dictionary<string, string>()
                )
                .HasColumnType("nvarchar(max)");

            // Relationships
            entity.HasMany(o => o.OrderItems)
                .WithOne(oi => oi.Order)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(o => o.Payment)
                .WithOne(p => p.Order)
                .HasForeignKey<Payment>(p => p.OrderId)
                .OnDelete(DeleteBehavior.Restrict);



            entity.HasOne(o => o.Business)
                .WithMany(b => b.Orders)
                .HasForeignKey(o => o.BusinessId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(o => o.Customer)
                .WithMany(c => c.Orders)
                .HasForeignKey(o => o.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // OrderItem Configuration
        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(oi => oi.Id);

            entity.Property(oi => oi.ProductName)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(oi => oi.ProductSKU)
                .HasMaxLength(100);

            entity.Property(oi => oi.ProductImage)
                .HasMaxLength(500);

            entity.Property(oi => oi.ProductImageThumbnail)
                .HasMaxLength(500);

            entity.Property(oi => oi.UnitPrice)
                .HasPrecision(18, 2);

            entity.Property(oi => oi.OriginalUnitPrice)
                .HasPrecision(18, 2);

            entity.Property(oi => oi.TotalPrice)
                .HasPrecision(18, 2);


            // Configure SelectedOptions as owned collection
            entity.OwnsMany(oi => oi.SelectedOptions, options =>
            {
                options.WithOwner().HasForeignKey("OrderItemId");
                options.Property(o => o.Id).ValueGeneratedOnAdd();
                options.Property(o => o.Name).IsRequired().HasMaxLength(100);
                options.Property(o => o.Value).IsRequired().HasMaxLength(100);
                options.Property(o => o.PriceAdjustment).HasPrecision(18, 2);
                options.ToTable("OrderItemOptions");
            });

            entity.HasOne(oi => oi.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(oi => oi.Product)
                .WithMany(p => p.OrderItems)
                .HasForeignKey(oi => oi.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Payment Configuration
        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.HasIndex(p => p.TransactionReference).IsUnique();
            entity.HasIndex(p => p.Status);
            entity.HasIndex(p => p.OrderId);
            entity.HasIndex(p => p.BusinessId);

            entity.Property(p => p.TransactionReference)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(p => p.Provider)
                .HasMaxLength(50);

            entity.Property(p => p.TransactionId)
                .HasMaxLength(100);

            entity.Property(p => p.PaymentMethod)
                .HasConversion<string>()
                .HasMaxLength(50);

            entity.Property(p => p.Status)
                .HasConversion<string>()
                .HasMaxLength(50);

            entity.Property(p => p.PaymentType)
                .HasConversion<string>()
                .HasMaxLength(50);

            entity.Property(p => p.Amount)
                .HasPrecision(18, 2);

            entity.Property(p => p.AmountPaid)
                .HasPrecision(18, 2);

            entity.Property(p => p.AmountRefunded)
                .HasPrecision(18, 2);

            entity.Property(p => p.PlatformFee)
                .HasPrecision(18, 2);

            entity.Property(p => p.PaymentGatewayFee)
                .HasPrecision(18, 2);

            entity.Property(p => p.BusinessEarnings)
                .HasPrecision(18, 2);

            entity.Property(p => p.Currency)
                .HasMaxLength(10);

            entity.Property(p => p.CustomerName)
                .HasMaxLength(200);

            entity.Property(p => p.CustomerEmail)
                .HasMaxLength(200);

            entity.Property(p => p.CustomerPhone)
                .HasMaxLength(20);

            entity.Property(p => p.FailureReason)
                .HasMaxLength(500);

            // Store GatewayResponse as JSON
            entity.Property(p => p.GatewayResponse)
                .HasColumnType("nvarchar(max)");

            // Store Metadata as JSON
            entity.Property(p => p.Metadata)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, (JsonSerializerOptions?)null) ?? new Dictionary<string, string>()
                )
                .HasColumnType("nvarchar(max)");

            entity.HasOne(p => p.Order)
                .WithOne(o => o.Payment)
                .HasForeignKey<Payment>(p => p.OrderId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(p => p.Business)
                .WithMany(b => b.Payments)
                .HasForeignKey(p => p.BusinessId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Delivery Configuration
        //modelBuilder.Entity<Delivery>(entity =>
        //{
        //    entity.HasKey(d => d.Id);
        //    entity.HasIndex(d => d.TrackingNumber).IsUnique();
        //    entity.HasIndex(d => d.Status);
        //    entity.HasIndex(d => d.OrderId);

        //    entity.Property(d => d.TrackingNumber)
        //        .IsRequired()
        //        .HasMaxLength(50);

        //    entity.Property(d => d.DeliveryAddress)
        //        .IsRequired()
        //        .HasMaxLength(500);

        //    entity.Property(d => d.CurrentLocation)
        //        .HasMaxLength(500);

        //    entity.Property(d => d.DeliveryType)
        //        .HasConversion<string>()
        //        .HasMaxLength(50);

        //    entity.Property(d => d.Status)
        //        .HasConversion<string>()
        //        .HasMaxLength(50);

        //    entity.Property(d => d.DeliveryPersonName)
        //        .HasMaxLength(200);

        //    entity.Property(d => d.DeliveryPersonPhone)
        //        .HasMaxLength(20);

        //    // Store TrackingHistory as JSON
        //    entity.OwnsMany(d => d.TrackingHistory, history =>
        //    {
        //        history.WithOwner().HasForeignKey("DeliveryId");
        //        history.Property(h => h.Status).HasConversion<string>();
        //        history.Property(h => h.Description).HasMaxLength(500);
        //        history.Property(h => h.Location).HasMaxLength(500);
        //        history.ToTable("DeliveryTrackingHistory");
        //    });

        //    entity.HasOne(d => d.Order)
        //        .WithOne(o => o.Delivery)
        //        .HasForeignKey<Delivery>(d => d.OrderId)
        //        .OnDelete(DeleteBehavior.Restrict);

        //    entity.HasOne(d => d.Business)
        //        .WithMany(b => b.Deliveries)
        //        .HasForeignKey(d => d.BusinessId)
        //        .OnDelete(DeleteBehavior.Restrict);
        //});

        // Conversation Configuration
        modelBuilder.Entity<Conversation>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.HasIndex(c => new { c.BusinessId, c.WhatsAppNumber }).IsUnique();
            entity.HasIndex(c => c.Status);

            entity.Property(c => c.WhatsAppNumber)
                .IsRequired()
                .HasMaxLength(20);

            entity.Property(c => c.CustomerName)
                .HasMaxLength(200);

            entity.Property(c => c.Status)
                .HasConversion<string>()
                .HasMaxLength(50);

            entity.Property(c => c.UnreadCount)
                .HasDefaultValue(0);

            // Store Messages as JSON
            var messageComparer = new ValueComparer<ICollection<ConversationMessage>>(
                (c1, c2) => c1 != null && c2 != null && c1.SequenceEqual(c2),
                c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                c => c.ToList());

            // Use HasValueComparer with the comparer
            entity.Property(c => c.Messages)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<ICollection<ConversationMessage>>(v, (JsonSerializerOptions?)null) ?? new List<ConversationMessage>()
                )
                .HasColumnType("nvarchar(max)");
                //.HasValueComparer(messageComparer);

            entity.HasOne(c => c.Business)
                .WithMany()
                .HasForeignKey(c => c.BusinessId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // AutomationRule Configuration
        modelBuilder.Entity<AutomationRule>(entity =>
        {
            entity.HasKey(ar => ar.Id);
            entity.HasIndex(ar => new { ar.BusinessId, ar.TriggerKeyword }).IsUnique();
            entity.HasIndex(ar => ar.IsActive);
            entity.HasIndex(ar => ar.Action);

            entity.Property(ar => ar.TriggerKeyword)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(ar => ar.AutoReplyMessage)
                .IsRequired()
                .HasMaxLength(1000);

            entity.Property(ar => ar.Action)
                .HasConversion<string>()
                .HasMaxLength(50);

            entity.Property(ar => ar.MatchType)
                .HasConversion<string>()
                .HasMaxLength(20)
                .HasDefaultValue(Domain.Entities.MatchType.Contains);

            entity.Property(ar => ar.IsActive)
                .HasDefaultValue(true);

            entity.Property(ar => ar.Priority)
                .HasDefaultValue(0);

            entity.Property(ar => ar.TimesTriggered)
                .HasDefaultValue(0);

            // Store ActiveDays as JSON with comparer
            var dayOfWeekComparer = new ValueComparer<List<DayOfWeek>>(
        (c1, c2) => c1 != null && c2 != null && c1.SequenceEqual(c2),
        c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
        c => c.ToList());

            entity.Property(ar => ar.ActiveDays)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<List<DayOfWeek>>(v, (JsonSerializerOptions?)null) ?? new List<DayOfWeek>()
                )
                .HasColumnType("nvarchar(max)");


            // Store StartTime and EndTime as string
            entity.Property(ar => ar.StartTime)
                .HasConversion(
                    v => v.HasValue ? v.Value.ToString() : null,
                    v => string.IsNullOrEmpty(v) ? null : TimeOnly.Parse(v)
                );

            entity.Property(ar => ar.EndTime)
                .HasConversion(
                    v => v.HasValue ? v.Value.ToString() : null,
                    v => string.IsNullOrEmpty(v) ? null : TimeOnly.Parse(v)
                );

            entity.HasOne(ar => ar.Business)
                .WithMany(b => b.AutomationRules)
                .HasForeignKey(ar => ar.BusinessId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(ar => ar.AssociatedProduct)
                .WithMany()
                .HasForeignKey(ar => ar.AssociatedProductId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Get all entities with domain events
        var entities = ChangeTracker
            .Entries<BaseEntity>()
            .Where(e => e.Entity.DomainEvents.Any())
            .Select(e => e.Entity)
            .ToList();

        // Get all domain events
        var domainEvents = entities
            .SelectMany(e => e.DomainEvents)
            .ToList();

        // Clear events
        foreach (var entity in entities)
        {
            entity.ClearDomainEvents();
        }

        // Save changes
        var result = await base.SaveChangesAsync(cancellationToken);

        // Dispatch events after save
        foreach (var domainEvent in domainEvents)
        {
            // You can publish events here if you have MediatR
            // await _mediator.Publish(domainEvent, cancellationToken);
        }

        return result;
    }
}