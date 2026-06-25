using BubbleShop.Application.Common.Interfaces;
using BubbleShop.Domain.Interfaces.Repositories;
using BubbleShop.Infrastructure.Configuration;
using BubbleShop.Infrastructure.ExternalServices.WhatsApp;
using BubbleShop.Infrastructure.Persistence;
using BubbleShop.Infrastructure.Persistence.Repositories;
using BubbleShop.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BubbleShop.Infrastructure;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Configure Options
        services.Configure<WhatsAppOptions>(configuration.GetSection(WhatsAppOptions.SectionName));
        // services.Configure<AzureOpenAIOptions>(configuration.GetSection(AzureOpenAIOptions.SectionName)); // Commented if not using Azure
        services.Configure<StripeOptions>(configuration.GetSection(StripeOptions.SectionName));
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.Configure<DeliveryOptions>(configuration.GetSection(DeliveryOptions.SectionName));
        services.Configure<EmailOptions>(configuration.GetSection("Email"));

        // Register DbContext - Use DefaultConnection or BubbleShopConnection
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection") ??
                configuration.GetConnectionString("BubbleShopConnection"),
                sqlOptions => sqlOptions.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)));

        // Register UnitOfWork
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Register Repositories
        services.AddScoped<IBusinessRepository, BusinessRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IConversationRepository, ConversationRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IAutomationRuleRepository, AutomationRuleRepository>();

        // Register Services
        services.AddHttpClient<IWhatsAppService, WhatsAppService>();
        services.AddScoped<IEmailService, EmailService>();
        // services.AddScoped<IPaymentService, StripePaymentService>();
        // services.AddScoped<IDeliveryService, DeliveryService>();
        // services.AddScoped<IAIAgentService, AzureOpenAIAgentService>();

        return services;
    }
}