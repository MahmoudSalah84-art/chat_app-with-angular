using chatme.Application.Common;
using chatme.Application.Common.DTO;
using chatme.Application.Common.Interfaces;
using chatme.Domain.Common;
using chatme.Domain.Repositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace chatme.Application.Features.Messages.Commands.SendMessage
{

	public sealed class SendMessageCommandHandler(
		IChatRepository chatRepository,
		IUnitOfWork unitOfWork,
		ICurrentUserService currentUserService) : IRequestHandler<SendMessageCommand, Result<MessageDto>>
	{
		public async Task<Result<MessageDto>> Handle(SendMessageCommand request, CancellationToken cancellationToken)
		{
			var userId = currentUserService.UserId;
			if (userId is null)
				return Result<MessageDto>.Unauthorized("لازم تسجل دخول الأول");

			var chat = await chatRepository.GetByIdWithDetailsAsync(request.ChatId, cancellationToken);
			if (chat is null)
				return Result<MessageDto>.NotFound("المحادثة دي مش موجودة");

			var messageResult = chat.SendMessage(userId.Value, request.Type, request.Content, request.ReplyToMessageId);
			if (messageResult.IsFailure)
				return messageResult.ToFailure<MessageDto>();

			await unitOfWork.SaveChangesAsync(cancellationToken);

			var message = messageResult.Value!;
			return Result<MessageDto>.Success(new MessageDto(
				message.Id, message.ChatId, message.SenderId, message.Type, message.Content,
				message.SentAt, message.ReplyToMessageId, message.IsEdited, message.IsDeleted));
		}
	}
}
