// Application/Features/Help/Queries/GetHelp/GetHelpQueryHandler.cs
using BubbleShop.Application.AppServices;
using BubbleShop.Application.Common.Interfaces;
using BubbleShop.Application.Common.Models;
using BubbleShop.Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BubbleShop.Application.Features.Help.Queries.GetHelp;

public sealed class GetHelpQueryHandler : IRequestHandler<GetHelpQuery, Result<MessageResponse>>
{
    private readonly IBusinessRepository _businessRepository;
    private readonly ILogger<GetHelpQueryHandler> _logger;

    public GetHelpQueryHandler(
        IBusinessRepository businessRepository,
        ILogger<GetHelpQueryHandler> logger)
    {
        _businessRepository = businessRepository;
        _logger = logger;
    }

    public async Task<Result<MessageResponse>> Handle(GetHelpQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Getting help for customer {CustomerId}", request.CustomerId);

            var business = await _businessRepository.GetByIdAsync(request.BusinessId, cancellationToken);
            var businessName = business?.BusinessName ?? "BubbleShop";

            var response = $"🤖 **Welcome to {businessName}!**\n\n" +
                          $"I'm your AI shopping assistant. Here's what I can help you with:\n\n" +
                          $"🛒 **Place an Order**\n" +
                          $"• Say 'I want [product]' or 'Buy [product]'\n" +
                          $"• Example: 'I want 2 bags of rice'\n\n" +
                          $"💰 **Check Prices**\n" +
                          $"• Ask 'How much is [product]?'\n" +
                          $"• Example: 'How much is rice?'\n\n" +
                          $"🔍 **Find Products**\n" +
                          $"• Say 'Show me [product]' or 'Search [product]'\n" +
                          $"• Example: 'Show me rice'\n\n" +
                          $"📊 **Check Stock**\n" +
                          $"• Ask 'Do you have [product]?'\n" +
                          $"• Example: 'Do you have rice in stock?'\n\n" +
                          $"🚚 **Track Orders**\n" +
                          $"• Say 'Track my order' or 'Where is my order?'\n" +
                          $"• Provide your order number\n\n" +
                          $"❌ **Cancel Orders**\n" +
                          $"• Say 'Cancel my order' followed by order number\n\n" +
                          $"🛒 **Cart Management**\n" +
                          $"• 'View cart' - See your cart\n" +
                          $"• 'Add [product]' - Add to cart\n" +
                          $"• 'Remove [product]' - Remove from cart\n\n" +
                          $"💬 **Tips**\n" +
                          $"• You can ask in natural language\n" +
                          $"• I'll guide you through each step\n" +
                          $"• Just type 'help' anytime\n\n" +
                          $"How can I assist you today? 😊";

            return Result<MessageResponse>.Success(new MessageResponse(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting help for customer {CustomerId}", request.CustomerId);
            return Result<MessageResponse>.Failure($"Failed to get help: {ex.Message}");
        }
    }
}