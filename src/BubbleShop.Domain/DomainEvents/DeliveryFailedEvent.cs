//using MediatR;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace BubbleShop.Domain.DomainEvents
//{
//    public record DeliveryFailedEvent : INotification
//    {
//        public Guid DeliveryId { get; }
//        public Guid OrderId { get; }
//        public string TrackingNumber { get; }
//        public string Reason { get; }
//        public DateTime OccurredOn { get; }

//        public DeliveryFailedEvent(Guid deliveryId, Guid orderId, string trackingNumber, string reason)
//        {
//            DeliveryId = deliveryId;
//            OrderId = orderId;
//            TrackingNumber = trackingNumber;
//            Reason = reason;
//            OccurredOn = DateTime.UtcNow;
//        }
//    }
//}
