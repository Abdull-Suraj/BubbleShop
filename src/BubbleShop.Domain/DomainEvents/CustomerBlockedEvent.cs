using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BubbleShop.Domain.DomainEvents
{
    public record CustomerBlockedEvent : INotification
    {
        public Guid CustomerId { get; }
        public string CustomerName { get; }
        public string? Reason { get; }
        public DateTime OccurredOn { get; }

        public CustomerBlockedEvent(Guid customerId, string customerName, string? reason = null)
        {
            CustomerId = customerId;
            CustomerName = customerName;
            Reason = reason;
            OccurredOn = DateTime.UtcNow;
        }
    }
}
