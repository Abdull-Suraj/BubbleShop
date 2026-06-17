using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BubbleShop.Domain.DomainEvents
{
    public record DeliveryCreatedEvent : INotification
    {
        public Guid DeliveryId { get; }
        public Guid OrderId { get; }
        public string TrackingNumber { get; }
        public DateTime OccurredOn { get; }

        public DeliveryCreatedEvent(Guid deliveryId, Guid orderId, string trackingNumber)
        {
            DeliveryId = deliveryId;
            OrderId = orderId;
            TrackingNumber = trackingNumber;
            OccurredOn = DateTime.UtcNow;
        }
    }

}
