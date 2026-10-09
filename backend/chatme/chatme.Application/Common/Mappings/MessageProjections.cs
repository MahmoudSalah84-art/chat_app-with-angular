using System.Linq.Expressions;
using chatme.Application.Common.DTO;
using chatme.Domain.Entities;

namespace chatme.Application.Common.Mappings
{
	public static class MessageProjections
	{
		public static readonly Expression<Func<Message, MessageDto>> ToDto = m => new MessageDto(
			m.Id, m.ChatId, m.SenderId, m.Type, m.Content, m.SentAt, m.ReplyToMessageId,
			m.IsEdited, m.IsDeleted,
			m.Attachment == null
				? null
				: new AttachmentDto(m.Attachment.Url, m.Attachment.FileName, m.Attachment.ContentType, m.Attachment.SizeInBytes),
			m.Reactions.Where(r => !m.IsDeleted).Select(r => new ReactionDto(r.UserId, r.Emoji)).ToList());

		private static readonly Func<Message, MessageDto> Compiled = ToDto.Compile();
		public static MessageDto Map(Message message) => Compiled(message);
	}
}