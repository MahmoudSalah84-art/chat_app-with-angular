using chatme.Application.Common.DTO;
using chatme.Application.Common.Interfaces;
using chatme.Domain.Common;
using chatme.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace chatme.Application.Features.Chats.Queries.GetUserChats
{
	public sealed class GetUserChatsQueryHandler(
		IApplicationDbContext dbContext,
		IIdentityService identityService,
		ICurrentUserService currentUserService) : IRequestHandler<GetUserChatsQuery, Result<List<ChatDto>>>
	{
		public async Task<Result<List<ChatDto>>> Handle(GetUserChatsQuery request, CancellationToken cancellationToken)
		{
			var userId = currentUserService.UserId;
			if (userId is null)
				return Result<List<ChatDto>>.Unauthorized("لازم تسجل دخول الأول");

			var chats = await dbContext.Chats
				.Include(c => c.Participants)
				.Include(c => c.Messages)
				.Where(c => c.Participants.Any(p => p.UserId == userId))
				.ToListAsync(cancellationToken);

			var allParticipantIds = chats
				.SelectMany(c => c.Participants.Select(p => p.UserId))
				.Distinct()
				.ToList();

			// بدل ما نستعلم على جدول Users مباشرة، بنسأل IIdentityService -
			// هو اللي عارف فين وإزاي بيانات المستخدمين متخزنة فعليًا (Identity)
			var users = await identityService.GetUsersByIdsAsync(allParticipantIds, cancellationToken);
			var usersById = users.ToDictionary(u => u.Id);

			var result = chats.Select(chat =>
			{
				var participantDtos = chat.Participants
					.Where(p => usersById.ContainsKey(p.UserId))
					.Select(p => usersById[p.UserId])
					.ToList();

				var otherParticipant = participantDtos.FirstOrDefault(p => p.Id != userId);
				var lastMessage = chat.Messages.OrderByDescending(m => m.SentAt).FirstOrDefault();

				return new ChatDto(
					chat.Id,
					chat.IsGroup,
					chat.IsGroup ? (chat.Name ?? "مجموعة") : (otherParticipant?.Name ?? "مستخدم"),
					chat.IsGroup ? chat.AvatarUrl : (otherParticipant?.AvatarUrl ?? string.Empty),
					participantDtos,
					lastMessage is null ? null : MapMessage(lastMessage),
					chat.GetUnreadCount(userId.Value));
			})
			.OrderByDescending(c => c.LastMessage?.SentAt ?? DateTime.MinValue)
			.ToList();

			return Result<List<ChatDto>>.Success(result);
		}

		private static MessageDto MapMessage(Message m) => new(
			m.Id, m.ChatId, m.SenderId, m.Type, m.Content, m.SentAt, m.ReplyToMessageId, m.IsEdited, m.IsDeleted);
	}

}
