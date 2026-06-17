using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BubbleShop.Domain.DomainEvents
{
    public record OrderStatusChangedEvent : INotification
    {
        public Guid OrderId { get; }
        public string OrderNumber { get; }
        public string OldStatus { get; }
        public string NewStatus { get; }
        public DateTime OccurredOn { get; }

        public OrderStatusChangedEvent(Guid orderId, string orderNumber, string oldStatus, string newStatus)
        {
            OrderId = orderId;
            OrderNumber = orderNumber;
            OldStatus = oldStatus;
            NewStatus = newStatus;
            OccurredOn = DateTime.UtcNow;
        }
    }
}
