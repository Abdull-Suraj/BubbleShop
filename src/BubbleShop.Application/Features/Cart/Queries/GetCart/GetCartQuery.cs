using MediatR;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BubbleShop.Application.Features.Cart.Queries.GetCart
{
    public class GetCartQuery : IRequest<IActionResult>
    {
        public Guid CustomerId { get; set; }
        public Guid BusinessId { get; set; }
    }
}
