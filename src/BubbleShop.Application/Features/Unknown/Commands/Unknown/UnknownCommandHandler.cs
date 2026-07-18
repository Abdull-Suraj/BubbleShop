// Application/Features/Unknown/Commands/Unknown/UnknownCommandHandler.cs
using BubbleShop.Application.AppServices;
using BubbleShop.Application.Common.Interfaces;
using BubbleShop.Application.Common.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BubbleShop.Application.Features.Unknown.Commands.Unknown;

public sealed class UnknownCommandHandler : IRequestHandler<UnknownCommand, Result<MessageResponse>>
{
    private readonly ILogger<UnknownCommandHandler> _logger;

    public UnknownCommandHandler(ILogger<UnknownCommandHandler> logger)
    {
        _logger = logger;
    }

    public async Task<Result<MessageResponse>> Handle(UnknownCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Handling unknown command from customer {CustomerId}", request.CustomerId);

            var response = $"🤔 **I'm not sure I understand**\n\n" +
                          $"I'm still learning and I didn't quite get that.\n\n" +
                          $"Here are some things you can say:\n\n" +
                          $"🛒 **Orders**\n" +
                          $"• 'I want to buy [product]'\n" +
                          $"• 'Order [product]'\n\n" +
                          $"💰 **Prices**\n" +
                          $"• 'How much is [product]?'\n" +
                          $"• 'Price of [product]'\n\n" +
                          $"🔍 **Products**\n" +
                          $"• 'Show me [product]'\n" +
                          $"• 'Search [product]'\n\n" +
                          $"🚚 **Tracking**\n" +
                          $"• 'Track my order'\n" +
                          $"• 'Where is my order?'\n\n" +
                          $"❓ **Help**\n" +
                          $"• 'Help'\n" +
                          $"• 'What can you do?'\n\n" +
                          $"😊 **Try rephrasing your message** and I'll do my best to help!";

            return Result<MessageResponse>.Success(new MessageResponse(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling unknown command from customer {CustomerId}", request.CustomerId);
            return Result<MessageResponse>.Failure($"Failed to process command: {ex.Message}");
        }
    }
}