using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BubbleShop.Domain.DomainEvents
{
    public record BusinessSuspendedEvent : INotification
    {
        public Guid BusinessId { get; }
        public string BusinessName { get; }
        public DateTime OccurredOn { get; }

        public BusinessSuspendedEvent(Guid businessId, string businessName)
        {
            BusinessId = businessId;
            BusinessName = businessName;
            OccurredOn = DateTime.UtcNow;
        }
    }
}
