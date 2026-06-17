using BubbleShop.Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BubbleShop.Domain.DomainEvents
{
    public record BusinessRegisteredEvent : INotification
    {
        public Guid BusinessId { get; }
        public string BusinessName { get; }
        public string Email { get; }
        public string WhatsAppNumber { get; }
        public DateTime OccurredOn { get; }

        public BusinessRegisteredEvent(Guid businessId, string businessName, string email, string whatsAppNumber)
        {
            BusinessId = businessId;
            BusinessName = businessName;
            Email = email;
            WhatsAppNumber = whatsAppNumber;
            OccurredOn = DateTime.UtcNow;
        }
    }
}
