using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BubbleShop.Domain.DomainEvents
{
    public record CustomerRegisteredEvent : INotification
    {
        public Guid CustomerId { get; }
        public string CustomerName { get; }
        public string WhatsAppNumber { get; }
        public Guid BusinessId { get; }
        public DateTime OccurredOn { get; }

        public CustomerRegisteredEvent(Guid customerId, string customerName, string whatsAppNumber, Guid businessId)
        {
            CustomerId = customerId;
            CustomerName = customerName;
            WhatsAppNumber = whatsAppNumber;
            BusinessId = businessId;
            OccurredOn = DateTime.UtcNow;
        }
    }
}
