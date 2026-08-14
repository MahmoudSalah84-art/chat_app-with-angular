using chatme.Domain.Common;

namespace chatme.Domain.Events
{
	public sealed record DirectChatCreatedDomainEvent(
		Guid ChatId, IReadOnlyCollection<Guid> ParticipantIds) : IDomainEvent
	{
		public DateTime OccurredOn { get; } = DateTime.UtcNow;
	}
}
