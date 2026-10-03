using chatme.Application.Common;
using chatme.Application.Common.DTO;
using chatme.Application.Common.Interfaces;
using chatme.Application.Common.Mappings;
using chatme.Domain.Common;
using chatme.Domain.Repositories;
using chatme.Domain.ValueObjects;
using MediatR;

namespace chatme.Application.Features.Messages.Commands.SendAttachment
{
	public sealed class SendAttachmentCommandHandler(
		IChatRepository chatRepository,
		IUnitOfWork unitOfWork,
		ICurrentUserService currentUserService,
		IFileStorageService fileStorage) : IRequestHandler<SendAttachmentCommand, Result<MessageDto>>
	{
		public async Task<Result<MessageDto>> Handle(SendAttachmentCommand request, CancellationToken cancellationToken)
		{
			var userId = currentUserService.UserId;
			if (userId is null)
				return Result<MessageDto>.Unauthorized("لازم تسجل دخول الأول");

			var chat = await chatRepository.GetByIdWithDetailsAsync(request.ChatId, cancellationToken);
			if (chat is null)
				return Result<MessageDto>.NotFound("المحادثة دي مش موجودة");

			if (!chat.IsParticipant(userId.Value))
				return Result<MessageDto>.Forbidden("إنت مش عضو في المحادثة دي");

			var type = AttachmentRules.Resolve(request.FileName);
			if (type is null)
				return Result<MessageDto>.Failure("نوع الملف ده مش مسموح");

			var upload = await fileStorage.UploadAsync(request.Content, request.FileName, type.Value, cancellationToken);
			if (upload.IsFailure)
				return upload.ToFailure<MessageDto>();

			var stored = upload.Value!;
			var attachment = new Attachment(stored.Url, stored.PublicId, request.FileName, request.ContentType, request.Size);

			var messageResult = chat.SendAttachment(userId.Value, type.Value, attachment, request.Caption, request.ReplyToMessageId);
			if (messageResult.IsFailure)
			{
				await fileStorage.DeleteAsync(stored.PublicId, type.Value, CancellationToken.None);
				return messageResult.ToFailure<MessageDto>();
			}

			chatRepository.AddMessage(messageResult.Value!);

			try
			{
				await unitOfWork.SaveChangesAsync(cancellationToken);
			}
			catch
			{
				await fileStorage.DeleteAsync(stored.PublicId, type.Value, CancellationToken.None);
				throw;
			}

			return Result<MessageDto>.Success(MessageProjections.Map(messageResult.Value!));
		}
	}
}