using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BubbleShop.Domain.DomainEvents
{
    public record PaymentFailedEvent : INotification
    {
        public Guid PaymentId { get; }
        public Guid OrderId { get; }
        public string TransactionReference { get; }
        public string FailureReason { get; }
        public DateTime OccurredOn { get; }

        public PaymentFailedEvent(Guid paymentId, Guid orderId, string transactionReference, string failureReason)
        {
            PaymentId = paymentId;
            OrderId = orderId;
            TransactionReference = transactionReference;
            FailureReason = failureReason;
            OccurredOn = DateTime.UtcNow;
        }
    }
}
