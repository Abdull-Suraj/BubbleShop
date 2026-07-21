using BubbleShop.Application.Common.Models;
using MediatR;


namespace BubbleShop.Application.AppServices
{
    public sealed record GetStoreHoursQuery(
        Guid BusinessId
    ) : IRequest<Result<MessageResponse>>;
}