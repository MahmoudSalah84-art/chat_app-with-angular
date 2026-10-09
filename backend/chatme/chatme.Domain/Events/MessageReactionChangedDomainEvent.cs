using chatme.Domain.Common;

namespace chatme.Domain.Events
{
	public sealed record MessageReactionChangedDomainEvent(
		Guid ChatId, Guid MessageId, Guid UserId, string? Emoji) : IDomainEvent
	{
		public DateTime OccurredOn { get; } = DateTime.UtcNow;
	}
}