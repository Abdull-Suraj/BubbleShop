using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BubbleShop.Application.DTOs
{
    public class CartDto
    {
        public Guid Id { get; set; }
        public List<CartItemDto> Items { get; set; } = [];
        public decimal TotalAmount { get; set; }
    }
}
