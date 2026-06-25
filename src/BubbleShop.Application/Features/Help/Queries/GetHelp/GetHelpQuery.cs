using BubbleShop.Application.Common.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BubbleShop.Application.Features.Help.Queries.GetHelp
{
    public class GetHelpQuery : IRequest<Result>
    {
        public Guid BusinessId { get; set; }
        public Guid? CustomerId { get; set; }
        public string Topic { get; set; } = "General";
    }
}
