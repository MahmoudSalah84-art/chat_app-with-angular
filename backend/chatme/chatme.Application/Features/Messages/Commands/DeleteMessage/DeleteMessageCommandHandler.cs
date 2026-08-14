using chatme.Application.Common;
using chatme.Application.Common.Interfaces;
using chatme.Domain.Common;
using chatme.Domain.Repositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace chatme.Application.Features.Messages.Commands.DeleteMessage
{

	public sealed class DeleteMessageCommandHandler(
		IChatRepository chatRepository,
		IUnitOfWork unitOfWork,
		ICurrentUserService currentUserService) : IRequestHandler<DeleteMessageCommand, Result>
	{
		public async Task<Result> Handle(DeleteMessageCommand request, CancellationToken cancellationToken)
		{
			var userId = currentUserService.UserId;
			if (userId is null)
				return Result.Unauthorized("لازم تسجل دخول الأول");

			var chat = await chatRepository.GetByIdWithDetailsAsync(request.ChatId, cancellationToken);
			if (chat is null)
				return Result.NotFound("المحادثة دي مش موجودة");

			var deleteResult = chat.DeleteMessage(userId.Value, request.MessageId);
			if (deleteResult.IsFailure)
				return deleteResult;

			await unitOfWork.SaveChangesAsync(cancellationToken);
			return Result.Success();
		}
	}

}
