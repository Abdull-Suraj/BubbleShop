using MediatR;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BubbleShop.Application.Features.Cart.Commands.RemoveFromCart
{
    public class RemoveFromCartCommand : IRequest<IActionResult>
    {
        public Guid CustomerId { get; set; }
        public Guid CartItemId { get; set; }
    }
}
