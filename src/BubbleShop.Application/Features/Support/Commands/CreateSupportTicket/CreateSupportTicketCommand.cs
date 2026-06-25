using BubbleShop.Application.Common.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BubbleShop.Application.Features.Support.Commands.CreateSupportTicket
{
    public class CreateSupportTicketCommand : IRequest<Result>
    {
        public Guid? CustomerId { get; set; }
        public string CustomerWhatsApp { get; set; } = string.Empty;
        public string? CustomerName { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Channel { get; set; } = string.Empty;
        public string Priority { get; set; } = "Normal";
    }
}
