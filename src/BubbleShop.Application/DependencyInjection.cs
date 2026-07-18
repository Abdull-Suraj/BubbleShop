using BubbleShop.Application.AppServices;
using BubbleShop.Application.Channels;
using BubbleShop.Application.Common.Behaviours;
using BubbleShop.Application.Common.Interfaces;

using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;


namespace BubbleShop.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
    
        // Register Application Services
        services.AddScoped<IAutomationService, AutomationService>();
        services.AddScoped<IAIIntentService, AIIntentService>();
        services.AddScoped<ICommandFactory, CommandFactory>();
        services.AddScoped<IMessageRouter, MessageRouter>();
        services.AddScoped<IAIAgentService, DummyAIAgentService>();
        services.AddScoped<IChannelAdapter, WhatsAppChannelAdapter>();
        //services.AddScoped<IChannelAdapter, TelegramChannelAdapter>();
        //services.AddScoped<IChannelAdapter, InstagramChannelAdapter>();
        services.AddScoped<IChannelAdapter, FacebookChannelAdapter>();

        // Register MediatR (CQRS)
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);

            // Add pipeline behaviors
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehaviour<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ExceptionHandlingBehaviour<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(PerformanceBehaviour<,>));
        });



        // Register FluentValidation
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        // Register Pipeline Behaviors (Alternative way if not using AddBehavior)
        // services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehaviour<,>));
        // services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));
        // services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ExceptionHandlingBehaviour<,>));

        return services;
    }
}