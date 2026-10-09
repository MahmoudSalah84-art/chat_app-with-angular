using chatme.Domain.Common;

namespace chatme.Domain.Entities
{
	public sealed class MessageReaction : BaseEntity
	{
		public Guid MessageId { get; private set; }
		public Guid UserId { get; private set; }
		public string Emoji { get; private set; } = string.Empty;
		public DateTime ReactedAt { get; private set; }

		private MessageReaction() { }

		internal static MessageReaction Create(Guid messageId, Guid userId, string emoji) => new()
		{
			Id = Guid.NewGuid(),
			MessageId = messageId,
			UserId = userId,
			Emoji = emoji,
			ReactedAt = DateTime.UtcNow,
		};

		internal void ChangeEmoji(string emoji)
		{
			Emoji = emoji;
			ReactedAt = DateTime.UtcNow;
		}
	}

	public sealed record ReactionChange(MessageReaction? Added, string? CurrentEmoji);
}