using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BubbleShop.Domain.DomainEvents
{
    public record CustomerUnblockedEvent : INotification
    {
        public Guid CustomerId { get; }
        public string CustomerName { get; }
        public DateTime OccurredOn { get; }

        public CustomerUnblockedEvent(Guid customerId, string customerName)
        {
            CustomerId = customerId;
            CustomerName = customerName;
            OccurredOn = DateTime.UtcNow;
        }
    }
}
