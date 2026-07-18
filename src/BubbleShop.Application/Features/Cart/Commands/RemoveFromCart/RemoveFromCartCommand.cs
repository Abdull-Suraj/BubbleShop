using BubbleShop.Application.AppServices;
using BubbleShop.Application.Common.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BubbleShop.Application.Features.Cart.Commands.RemoveFromCart
{

    public sealed record RemoveFromCartCommand(
    string Channel,
    Guid CustomerId,
    Guid BusinessId,
    Guid CartItemId,
    string Message
) : IRequest<Result<MessageResponse>>;
}
