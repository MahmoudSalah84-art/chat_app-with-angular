using chatme.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace chatme.Domain.Events
{
	public sealed record MessageSentDomainEvent(
	Guid ChatId, Guid MessageId, Guid SenderId) : IDomainEvent
	{
		public DateTime OccurredOn { get; } = DateTime.UtcNow;
	}

}
