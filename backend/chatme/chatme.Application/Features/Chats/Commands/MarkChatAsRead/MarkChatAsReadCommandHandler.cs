using chatme.Application.Common;
using chatme.Application.Common.Interfaces;
using chatme.Domain.Common;
using chatme.Domain.Repositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace chatme.Application.Features.Chats.Commands.MarkChatAsRead
{

	public sealed class MarkChatAsReadCommandHandler(
		IChatRepository chatRepository,
		IUnitOfWork unitOfWork,
		ICurrentUserService currentUserService) : IRequestHandler<MarkChatAsReadCommand, Result>
	{
		public async Task<Result> Handle(MarkChatAsReadCommand request, CancellationToken cancellationToken)
		{
			var userId = currentUserService.UserId;
			if (userId is null)
				return Result.Unauthorized("لازم تسجل دخول الأول");

			var chat = await chatRepository.GetByIdWithDetailsAsync(request.ChatId, cancellationToken);
			if (chat is null)
				return Result.NotFound("المحادثة دي مش موجودة");

			var result = chat.MarkAsRead(userId.Value, request.LastReadMessageId);
			if (result.IsFailure)
				return result;

			await unitOfWork.SaveChangesAsync(cancellationToken);
			return Result.Success();
		}
	}

}
