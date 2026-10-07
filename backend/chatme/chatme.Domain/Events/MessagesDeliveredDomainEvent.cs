using chatme.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace chatme.Domain.Events
{
	public sealed record MessagesDeliveredDomainEvent(Guid ChatId, Guid UserId, DateTime UpToSentAt) : IDomainEvent
	{
		public DateTime OccurredOn { get; } = DateTime.UtcNow;
	}
}
