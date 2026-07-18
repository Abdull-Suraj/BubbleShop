using BubbleShop.Application.Common.Models;
using BubbleShop.Domain.Common;
using MediatR;

namespace BubbleShop.Application.Common.Interfaces;

public interface ICommandFactory
{
    Task<IBaseRequest> CreateCommandAsync(
        IntentResult intent,
        MessageContext context,
        CancellationToken cancellationToken = default);
}