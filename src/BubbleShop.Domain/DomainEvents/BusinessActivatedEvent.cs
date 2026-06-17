using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BubbleShop.Domain.DomainEvents
{
    public record BusinessActivatedEvent : INotification
    {
        public Guid BusinessId { get; }
        public string BusinessName { get; }
        public DateTime OccurredOn { get; }

        public BusinessActivatedEvent(Guid businessId, string businessName)
        {
            BusinessId = businessId;
            BusinessName = businessName;
            OccurredOn = DateTime.UtcNow;
        }
    }
}
