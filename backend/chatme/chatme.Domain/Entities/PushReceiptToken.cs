using chatme.Domain.Common;

namespace chatme.Domain.Entities
{
	public sealed class PushReceiptToken : BaseEntity
	{
		public Guid ChatId { get; private set; }
		public Guid MessageId { get; private set; }
		public Guid ReceiverId { get; private set; }

		public string TokenHash { get; private set; } = null!;

		public DateTime ExpiresAtUtc { get; private set; }
		public DateTime? UsedAtUtc { get; private set; }

		private PushReceiptToken() { }

		public static PushReceiptToken Create(
			Guid chatId,
			Guid messageId,
			Guid receiverId,
			string tokenHash)
		{
			return new PushReceiptToken
			{
				Id = Guid.NewGuid(),
				ChatId = chatId,
				MessageId = messageId,
				ReceiverId = receiverId,
				TokenHash = tokenHash,
				ExpiresAtUtc = DateTime.UtcNow.AddDays(2)
			};
		}

		public bool IsValid(DateTime now)
		{
			return UsedAtUtc is null && ExpiresAtUtc > now;
		}

		public bool MarkAsUsed(DateTime now)
		{
			if (!IsValid(now))
				return false;

			UsedAtUtc = now;
			return true;
		}
	}
}
