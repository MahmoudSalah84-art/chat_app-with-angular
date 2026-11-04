using chatme.Application.Common.DTO;
using chatme.Application.Common.Interfaces;
using chatme.Application.Common.Mappings;
using chatme.Domain.Common;
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
				.AsNoTracking()
				.Where(c =>
					c.Participants.Any(p => p.UserId == userId) &&
					c.Messages.Any())
				.Select(c => new
				{
					Chat = c,

					Participants = c.Participants
						.Select(p => new
						{
							p.UserId,
							p.LastDeliveredAt,
							p.LastReadAt
						})
						.ToList(),

					LastMessage = c.Messages
						.OrderByDescending(m => m.SentAt)
						.ThenByDescending(m => m.Id)
						.Select(m => MessageProjections.Map(m))
						.FirstOrDefault()
				})
				.ToListAsync(cancellationToken);


			// 2. Collect unique participant IDs
			var participantIds = chats
			   .SelectMany(c => c.Participants.Select(p => p.UserId))
			   .Distinct()
			   .ToList();


			// 3. Get users from Identity
			var users = await identityService.GetUsersByIdsAsync(
				participantIds,
				cancellationToken);

			var usersById = users.ToDictionary(u => u.Id);



			// 4. Build Chat DTOs
			var result = chats.Select(item =>
			{
				var participantDtos = item.Participants
					.Where(p => usersById.ContainsKey(p.UserId))
					.Select(p => usersById[p.UserId])
					.ToList();

				var otherParticipant = participantDtos
					.FirstOrDefault(p => p.Id != userId);

				var receipts = item.Participants
					.Select(p => new ParticipantReceiptDto(
						p.UserId,
						p.LastDeliveredAt,
						p.LastReadAt))
					.ToList();

				return new ChatDto(
					item.Chat.Id,
					item.Chat.IsGroup,
					item.Chat.IsGroup
						? item.Chat.Name ?? "مجموعة"
						: otherParticipant?.Name ?? "مستخدم",
					item.Chat.IsGroup
						? item.Chat.AvatarUrl
						: otherParticipant?.AvatarUrl ?? string.Empty,
					participantDtos,
					item.LastMessage,
					item.Chat.GetUnreadCount(userId.Value),
					receipts
				);
			})
			.OrderByDescending(c => c.LastMessage?.SentAt ?? DateTime.MinValue)
			.ToList();

			return Result<List<ChatDto>>.Success(result);
		}
	}
}