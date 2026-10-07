using chatme.Domain.Common;

namespace chatme.Domain.Events
{
	public sealed record MessagesReadDomainEvent(Guid ChatId, Guid UserId, DateTime UpToSentAt) : IDomainEvent
	{
		public DateTime OccurredOn { get; } = DateTime.UtcNow;
	}
}
 