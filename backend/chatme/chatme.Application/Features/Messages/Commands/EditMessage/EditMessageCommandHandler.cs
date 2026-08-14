using chatme.Application.Common;
using chatme.Application.Common.DTO;
using chatme.Application.Common.Interfaces;
using chatme.Domain.Common;
using chatme.Domain.Repositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace chatme.Application.Features.Messages.Commands.EditMessage
{

	public sealed class EditMessageCommandHandler(
		IChatRepository chatRepository,
		IUnitOfWork unitOfWork,
		ICurrentUserService currentUserService) : IRequestHandler<EditMessageCommand, Result<MessageDto>>
	{
		public async Task<Result<MessageDto>> Handle(EditMessageCommand request, CancellationToken cancellationToken)
		{
			var userId = currentUserService.UserId;
			if (userId is null)
				return Result<MessageDto>.Unauthorized("لازم تسجل دخول الأول");

			var chat = await chatRepository.GetByIdWithDetailsAsync(request.ChatId, cancellationToken);
			if (chat is null)
				return Result<MessageDto>.NotFound("المحادثة دي مش موجودة");

			var editResult = chat.EditMessage(userId.Value, request.MessageId, request.NewContent);
			if (editResult.IsFailure)
				return editResult.ToFailure<MessageDto>();

			await unitOfWork.SaveChangesAsync(cancellationToken);

			var message = chat.Messages.First(m => m.Id == request.MessageId);
			return Result<MessageDto>.Success(new MessageDto(
				message.Id, message.ChatId, message.SenderId, message.Type, message.Content,
				message.SentAt, message.ReplyToMessageId, message.IsEdited, message.IsDeleted));
		}
	}

}
