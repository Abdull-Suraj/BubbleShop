using MediatR;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BubbleShop.Application.Features.Cart.Commands.AddToCart
{
    public class AddToCartCommand : IRequest<IActionResult>
    {
        public Guid CustomerId { get; set; }
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
