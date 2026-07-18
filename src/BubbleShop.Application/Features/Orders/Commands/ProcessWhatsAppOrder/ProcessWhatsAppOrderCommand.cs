
using BubbleShop.Application.Common.Models;

using MediatR;


namespace BubbleShop.Application.Features.Orders.Commands.ProcessWhatsAppOrder
{
   
 

    public sealed record ProcessWhatsAppOrderCommand(
        string CustomerWhatsApp,
        string CustomerName,
        string BusinessWhatsApp,
        string Message
    ) : IRequest<Result<WhatsAppOrderResponse>>;

    public record WhatsAppOrderResponse(
        string ResponseMessage,
        Guid? OrderId,
        string? OrderNumber,
        decimal? TotalAmount,
        string? ProductName,
        int? Quantity
    );
}
