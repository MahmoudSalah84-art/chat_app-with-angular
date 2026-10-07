using chatme.Application.Common.DTO;
using chatme.Domain.Common;
using MediatR;

namespace chatme.Application.Features.Messages.Commands.SendAttachment
{
	public sealed record SendAttachmentCommand(
		Guid ChatId,
		Stream Content,
		string FileName,
		string ContentType,
		long Size,
		string? Caption,
		Guid? ReplyToMessageId) : IRequest<Result<MessageDto>>;
}
