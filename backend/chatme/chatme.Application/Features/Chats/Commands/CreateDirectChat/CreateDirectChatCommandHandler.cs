using chatme.Application.Common;
using chatme.Application.Common.DTO;
using chatme.Application.Common.Interfaces;
using chatme.Domain.Common;
using chatme.Domain.Entities;
using chatme.Domain.Repositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace chatme.Application.Features.Chats.Commands.CreateDirectChat
{
	public sealed class CreateDirectChatCommandHandler(
		IChatRepository chatRepository,
		IIdentityService identityService,
		IUnitOfWork unitOfWork,
		ICurrentUserService currentUserService) : IRequestHandler<CreateDirectChatCommand, Result<ChatDto>>
	{
		public async Task<Result<ChatDto>> Handle(CreateDirectChatCommand request, CancellationToken cancellationToken)
		{
			var userId = currentUserService.UserId;
			if (userId is null)
				return Result<ChatDto>.Unauthorized("لازم تسجل دخول الأول");

			var otherUserResult = await identityService.GetUserByIdAsync(request.OtherUserId, cancellationToken);
			if (otherUserResult.IsFailure)
				return otherUserResult.ToFailure<ChatDto>();

			var meResult = await identityService.GetUserByIdAsync(userId.Value, cancellationToken);
			if (meResult.IsFailure)
				return meResult.ToFailure<ChatDto>();

			var otherUser = otherUserResult.Value!;
			var me = meResult.Value!;

			// لو فيه محادثة بينهم بالفعل، رجّعها بدل ما نعمل واحدة مكررة
			var existingChat = await chatRepository.GetDirectChatBetweenAsync(userId.Value, request.OtherUserId, cancellationToken);
			if (existingChat is not null)
				return Result<ChatDto>.Success(new ChatDto(existingChat.Id, false, otherUser.Name, otherUser.AvatarUrl, [me, otherUser], null, 0));

			var chatResult = Chat.CreateDirect(userId.Value, request.OtherUserId);
			if (chatResult.IsFailure)
				return chatResult.ToFailure<ChatDto>();

			var chat = chatResult.Value!;
			chatRepository.Add(chat);
			await unitOfWork.SaveChangesAsync(cancellationToken);

			return Result<ChatDto>.Success(new ChatDto(chat.Id, false, otherUser.Name, otherUser.AvatarUrl, [me, otherUser], null, 0));
		}
	}

}
