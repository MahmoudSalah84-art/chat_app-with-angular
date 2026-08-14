using chatme.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace chatme.Domain.Events
{
	public sealed record GroupChatCreatedDomainEvent(
	Guid ChatId, IReadOnlyCollection<Guid> ParticipantIds) : IDomainEvent
	{
		public DateTime OccurredOn { get; } = DateTime.UtcNow;
	}
}
