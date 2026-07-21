using BubbleShop.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace BubbleShop.Application.Features.Orders.Commands.CancelOrder
{
    public record CancelOrderRequest(
        Guid BusinessId,
        string ChannelUserId,
        ChannelType Channel,
        string? Reason = null
    );
}
