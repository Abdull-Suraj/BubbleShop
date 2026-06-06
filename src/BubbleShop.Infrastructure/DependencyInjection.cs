using BubbleShop.Application.Common.Interfaces;
using BubbleShop.Domain.Interfaces.Repositories;
using BubbleShop.Infrastructure.Configuration;
using BubbleShop.Infrastructure.ExternalServices.AI;
using BubbleShop.Infrastructure.ExternalServices.Delivery;
using BubbleShop.Infrastructure.ExternalServices.Payment;
using BubbleShop.Infrastructure.ExternalServices.WhatsApp;
using BubbleShop.Infrastructure.Persistence;
using BubbleShop.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BubbleShop.Infrastructure;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<WhatsAppOptions>(configuration.GetSection(WhatsAppOptions.SectionName));
        services.Configure<AzureOpenAIOptions>(configuration.GetSection(AzureOpenAIOptions.SectionName));
        services.Configure<StripeOptions>(configuration.GetSection(StripeOptions.SectionName));
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.Configure<DeliveryOptions>(configuration.GetSection(DeliveryOptions.SectionName));

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IConversationRepository, ConversationRepository>();

        services.AddHttpClient<IWhatsAppService, WhatsAppService>();
        services.AddScoped<IPaymentService, StripePaymentService>();
        services.AddScoped<IDeliveryService, DeliveryService>();
        services.AddScoped<IAIAgentService, AzureOpenAIAgentService>();

        return services;
    }
}
