using chatme.Domain.Common;
using chatme.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace chatme.Domain.Entities
{

	public sealed class ChatParticipant : BaseEntity
	{
		public Guid ChatId { get; private set; }
		public Guid UserId { get; private set; }
		public ChatParticipantRole Role { get; private set; }
		public DateTime JoinedAt { get; private set; }
		public Guid? LastReadMessageId { get; private set; }

		private ChatParticipant() { }

		internal static ChatParticipant Create(Guid chatId, Guid userId, ChatParticipantRole role) => new()
		{
			Id = Guid.NewGuid(),
			ChatId = chatId,
			UserId = userId,
			Role = role,
			JoinedAt = DateTime.UtcNow,
		};

		internal void MarkAsRead(Guid lastReadMessageId)
		{
			LastReadMessageId = lastReadMessageId;
		}
	}

}