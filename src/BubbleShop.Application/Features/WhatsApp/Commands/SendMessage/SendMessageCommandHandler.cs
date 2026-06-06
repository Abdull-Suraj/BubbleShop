using BubbleShop.Application.Common.Interfaces;
using BubbleShop.Application.Common.Models;
using MediatR;

namespace BubbleShop.Application.Features.WhatsApp.Commands.SendMessage;

public sealed class SendMessageCommandHandler(IWhatsAppService whatsAppService) : IRequestHandler<SendMessageCommand, Result>
{
    public async Task<Result> Handle(SendMessageCommand request, CancellationToken cancellationToken)
    {
        await whatsAppService.SendMessageAsync(request.ToNumber, request.Message, cancellationToken);
        return Result.Success();
    }
}
