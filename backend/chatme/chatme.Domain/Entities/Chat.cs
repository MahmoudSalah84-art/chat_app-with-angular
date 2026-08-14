using chatme.Domain.Common;
using chatme.Domain.Enums;
using chatme.Domain.Events;

namespace chatme.Domain.Entities
{
	public sealed class Chat : AggregateRoot
	{
		private readonly List<ChatParticipant> _participants = [];
		private readonly List<Message> _messages = [];

		public bool IsGroup { get; private set; }
		public string? Name { get; private set; }
		public string AvatarUrl { get; private set; } = string.Empty;
		public Guid CreatedByUserId { get; private set; }
		public DateTime CreatedAt { get; private set; }

		public IReadOnlyCollection<ChatParticipant> Participants => _participants.AsReadOnly();
		public IReadOnlyCollection<Message> Messages => _messages.AsReadOnly();

		private Chat() { }

		public static Result<Chat> CreateDirect(Guid userAId, Guid userBId)
		{
			if (userAId == userBId)
				return Result<Chat>.Failure("متقدرش تعمل محادثة مع نفسك");

			var chat = new Chat
			{
				Id = Guid.NewGuid(),
				IsGroup = false,
				CreatedByUserId = userAId,
				CreatedAt = DateTime.UtcNow,
			};

			chat._participants.Add(ChatParticipant.Create(chat.Id, userAId, ChatParticipantRole.Member));
			chat._participants.Add(ChatParticipant.Create(chat.Id, userBId, ChatParticipantRole.Member));

			chat.RaiseDomainEvent(new DirectChatCreatedDomainEvent(chat.Id, [userAId, userBId]));

			return Result<Chat>.Success(chat);
		}

		public static Result<Chat> CreateGroup(string name, string avatarUrl, Guid creatorId, IEnumerable<Guid> memberIds)
		{
			if (string.IsNullOrWhiteSpace(name))
				return Result<Chat>.Failure("اسم المجموعة مطلوب");

			var distinctMembers = memberIds.Distinct().Where(id => id != creatorId).ToList();
			if (distinctMembers.Count == 0)
				return Result<Chat>.Failure("لازم تختار عضو واحد على الأقل غيرك عشان تعمل مجموعة");

			var chat = new Chat
			{
				Id = Guid.NewGuid(),
				IsGroup = true,
				Name = name.Trim(),
				AvatarUrl = avatarUrl,
				CreatedByUserId = creatorId,
				CreatedAt = DateTime.UtcNow,
			};

			chat._participants.Add(ChatParticipant.Create(chat.Id, creatorId, ChatParticipantRole.Admin));
			foreach (var memberId in distinctMembers)
				chat._participants.Add(ChatParticipant.Create(chat.Id, memberId, ChatParticipantRole.Member));

			var allParticipantIds = distinctMembers.Append(creatorId).ToList();
			chat.RaiseDomainEvent(new GroupChatCreatedDomainEvent(chat.Id, allParticipantIds));

			return Result<Chat>.Success(chat);
		}

		public Result<Message> SendMessage(Guid senderId, MessageType type, string content, Guid? replyToMessageId)
		{
			if (!IsParticipant(senderId))
				return Result<Message>.Forbidden("إنت مش عضو في المحادثة دي");

			if (replyToMessageId is not null && _messages.All(m => m.Id != replyToMessageId))
				return Result<Message>.Failure("الرسالة اللي بترد عليها مش موجودة في المحادثة دي");

			var messageResult = Message.Create(Id, senderId, type, content, replyToMessageId);
			if (messageResult.IsFailure)
				return messageResult;

			_messages.Add(messageResult.Value!);
			RaiseDomainEvent(new MessageSentDomainEvent(Id, messageResult.Value!.Id, senderId));

			return messageResult;
		}

		public Result EditMessage(Guid requesterId, Guid messageId, string newContent)
		{
			var message = _messages.FirstOrDefault(m => m.Id == messageId);
			if (message is null)
				return Result.NotFound("الرسالة دي مش موجودة");

			var editResult = message.Edit(requesterId, newContent);
			if (editResult.IsFailure)
				return editResult;

			RaiseDomainEvent(new MessageEditedDomainEvent(Id, messageId));
			return Result.Success();
		}

		public Result DeleteMessage(Guid requesterId, Guid messageId)
		{
			var message = _messages.FirstOrDefault(m => m.Id == messageId);
			if (message is null)
				return Result.NotFound("الرسالة دي مش موجودة");

			var deleteResult = message.SoftDelete(requesterId);
			if (deleteResult.IsFailure)
				return deleteResult;

			RaiseDomainEvent(new MessageDeletedDomainEvent(Id, messageId));
			return Result.Success();
		}

		public Result MarkAsRead(Guid userId, Guid lastReadMessageId)
		{
			var participant = _participants.FirstOrDefault(p => p.UserId == userId);
			if (participant is null)
				return Result.Forbidden("إنت مش عضو في المحادثة دي");

			participant.MarkAsRead(lastReadMessageId);
			return Result.Success();
		}

		public int GetUnreadCount(Guid userId)
		{
			var participant = _participants.FirstOrDefault(p => p.UserId == userId);
			if (participant is null) return 0;

			var orderedMessages = _messages.OrderBy(m => m.SentAt).ToList();
			var lastReadIndex = participant.LastReadMessageId is null
				? -1
				: orderedMessages.FindIndex(m => m.Id == participant.LastReadMessageId);

			return orderedMessages
				.Skip(lastReadIndex + 1)
				.Count(m => m.SenderId != userId);
		}

		public bool IsParticipant(Guid userId) => _participants.Any(p => p.UserId == userId);
	}

}
