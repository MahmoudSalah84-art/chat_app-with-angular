using chatme.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace chatme.Domain.Events
{

	public sealed record MessageEditedDomainEvent(Guid ChatId, Guid MessageId) : IDomainEvent
	{
		public DateTime OccurredOn { get; } = DateTime.UtcNow;
	}

}
