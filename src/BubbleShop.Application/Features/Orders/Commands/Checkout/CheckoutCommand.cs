using BubbleShop.Application.Common.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BubbleShop.Application.Features.Orders.Commands.Checkout
{
    public class CheckoutCommand : IRequest<IActionResult>
    {
        public Guid CustomerId { get; set; }
        public Guid BusinessId { get; set; }
        public string Channel { get; set; } = "Whatsapp";
    }
}
