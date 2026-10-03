using chatme.Domain.Common;
using chatme.Domain.Enums;
using chatme.Domain.Exceptions;
using chatme.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace chatme.Domain.Entities
{


	public sealed class Message : BaseEntity
	{
		public Guid ChatId { get; private set; }
		public Guid SenderId { get; private set; }
		public MessageType Type { get; private set; }
		public string Content { get; private set; } = string.Empty;

		public Attachment? Attachment { get; private set; }
		public DateTime SentAt { get; private set; }
		public Guid? ReplyToMessageId { get; private set; }
		public bool IsEdited { get; private set; }
		public bool IsDeleted { get; private set; }

		private Message() { }

		internal static Result<Message> Create(
			Guid chatId, Guid senderId, MessageType type, string content, Guid? replyToMessageId)
		{
			if (string.IsNullOrWhiteSpace(content))
				return Result<Message>.Failure("مينفعش تبعت رسالة فاضية");

			var message = new Message
			{
				Id = Guid.NewGuid(),
				ChatId = chatId,
				SenderId = senderId,
				Type = type,
				Content = content,
				ReplyToMessageId = replyToMessageId,
				SentAt = DateTime.UtcNow,
			};

			return Result<Message>.Success(message);
		}

		internal Result Edit(Guid requesterId, string newContent)
		{
			if (SenderId != requesterId)
				return Result.Forbidden("متقدرش تعدّل رسالة مش بتاعتك");

			if (IsDeleted)
				return Result.Failure("الرسالة دي محذوفة بالفعل");

			if (Type != MessageType.Text)
				return Result.Failure("مينفعش تعدّل غير الرسائل النصية");

			if (string.IsNullOrWhiteSpace(newContent))
				return Result.Failure("مينفعش تسيب الرسالة فاضية");

			Content = newContent.Trim();
			IsEdited = true;
			return Result.Success();
		}

		internal static Result<Message> CreateWithAttachment(
			Guid chatId, Guid senderId, MessageType type, Attachment attachment,
			string? caption, Guid? replyToMessageId)
		{
			if (type == MessageType.Text)
				return Result<Message>.Failure("نوع الرسالة مش مناسب لمرفق");

			return Result<Message>.Success(new Message
			{
				Id = Guid.NewGuid(),
				ChatId = chatId,
				SenderId = senderId,
				Type = type,
				Content = caption?.Trim() ?? string.Empty,
				Attachment = attachment,
				ReplyToMessageId = replyToMessageId,
				SentAt = DateTime.UtcNow,
			});
		}

		internal Result SoftDelete(Guid requesterId)
		{
			if (SenderId != requesterId)
				return Result.Forbidden("متقدرش تحذف رسالة مش بتاعتك");

			if (IsDeleted)
				return Result.Failure("الرسالة دي محذوفة بالفعل");

			Content = string.Empty;
			IsDeleted = true;
			ReplyToMessageId = null;
			return Result.Success();
		}
	}
}