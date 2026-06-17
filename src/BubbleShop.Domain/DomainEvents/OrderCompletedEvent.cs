using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BubbleShop.Domain.DomainEvents
{
    public record OrderCompletedEvent : INotification
    {
        public Guid OrderId { get; }
        public string OrderNumber { get; }
        public DateTime OccurredOn { get; }

        public OrderCompletedEvent(Guid orderId, string orderNumber)
        {
            OrderId = orderId;
            OrderNumber = orderNumber;
            OccurredOn = DateTime.UtcNow;
        }
    }
}
