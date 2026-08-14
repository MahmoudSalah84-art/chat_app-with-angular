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

namespace chatme.Application.Features.Chats.Commands.CreateGroupChat
{

	public sealed class CreateGroupChatCommandHandler(
		IChatRepository chatRepository,
		IIdentityService identityService,
		IUnitOfWork unitOfWork,
		ICurrentUserService currentUserService) : IRequestHandler<CreateGroupChatCommand, Result<ChatDto>>
	{
		public async Task<Result<ChatDto>> Handle(CreateGroupChatCommand request, CancellationToken cancellationToken)
		{
			var userId = currentUserService.UserId;
			if (userId is null)
				return Result<ChatDto>.Unauthorized("لازم تسجل دخول الأول");

			var defaultAvatar = request.AvatarUrl ??
				$"https://ui-avatars.com/api/?name={Uri.EscapeDataString(request.Name)}&background=A0AEC0&color=fff";

			var chatResult = Chat.CreateGroup(request.Name, defaultAvatar, userId.Value, request.MemberIds);
			if (chatResult.IsFailure)
				return chatResult.ToFailure<ChatDto>();

			var chat = chatResult.Value!;
			chatRepository.Add(chat);
			await unitOfWork.SaveChangesAsync(cancellationToken);

			var allParticipantIds = request.MemberIds.Append(userId.Value).Distinct().ToList();
			var participants = await identityService.GetUsersByIdsAsync(allParticipantIds, cancellationToken);

			return Result<ChatDto>.Success(new ChatDto(chat.Id, true, chat.Name!, chat.AvatarUrl, participants, null, 0));
		}
	}
}
