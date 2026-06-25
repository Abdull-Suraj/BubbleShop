using BubbleShop.Application.Common.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BubbleShop.Application.AppServices
{
    public class GetStoreHoursQuery : IRequest<IActionResult>
    {
        public Guid BusinessId { get; set; }
    }
}