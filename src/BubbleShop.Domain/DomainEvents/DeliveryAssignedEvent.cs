using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BubbleShop.Domain.DomainEvents
{
    public record DeliveryAssignedEvent : INotification
    {
        public Guid DeliveryId { get; }
        public Guid OrderId { get; }
        public string TrackingNumber { get; }
        public string DeliveryPersonName { get; }
        public DateTime OccurredOn { get; }

        public DeliveryAssignedEvent(Guid deliveryId, Guid orderId, string trackingNumber, string deliveryPersonName)
        {
            DeliveryId = deliveryId;
            OrderId = orderId;
            TrackingNumber = trackingNumber;
            DeliveryPersonName = deliveryPersonName;
            OccurredOn = DateTime.UtcNow;
        }
    }
}
