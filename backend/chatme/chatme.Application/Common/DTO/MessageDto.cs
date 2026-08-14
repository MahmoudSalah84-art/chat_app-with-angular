using chatme.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace chatme.Application.Common.DTO
{

	public sealed record MessageDto(
		Guid Id, Guid ChatId, Guid SenderId, MessageType Type, string Content,
		DateTime SentAt, Guid? ReplyToMessageId, bool IsEdited, bool IsDeleted);

}
