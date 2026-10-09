using chatme.Domain.Enums;

namespace chatme.Application.Common.DTO
{
	public sealed record MessageDto(
		Guid Id, Guid ChatId, Guid SenderId, MessageType Type, string Content,
		DateTime SentAt, Guid? ReplyToMessageId, bool IsEdited, bool IsDeleted,
		AttachmentDto? Attachment, List<ReactionDto> Reactions);
}
