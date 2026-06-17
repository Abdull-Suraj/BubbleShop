using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BubbleShop.Domain.DomainEvents
{
    public record PaymentRefundedEvent : INotification
    {
        public Guid PaymentId { get; }
        public Guid OrderId { get; }
        public string TransactionReference { get; }
        public decimal RefundAmount { get; }
        public string Reason { get; }
        public DateTime OccurredOn { get; }

        public PaymentRefundedEvent(Guid paymentId, Guid orderId, string transactionReference, decimal refundAmount, string? reason = null)
        {
            PaymentId = paymentId;
            OrderId = orderId;
            TransactionReference = transactionReference;
            RefundAmount = refundAmount;
            Reason = reason ?? "No reason provided";
            OccurredOn = DateTime.UtcNow;
        }
    }
}
