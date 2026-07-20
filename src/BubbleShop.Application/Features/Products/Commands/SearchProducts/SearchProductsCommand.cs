using BubbleShop.Application.AppServices;
using BubbleShop.Application.Common.Models;
using MediatR;

namespace BubbleShop.Application.Features.Products.Commands.SearchProducts
{
    public sealed record SearchProductsCommand(
        Guid BusinessId,
        Guid CustomerId,
        string Keyword
    ) : IRequest<Result<MessageResponse>>;
}
