using chatme.Application.Common.DTO;
using chatme.Application.Common.Interfaces;
using chatme.Application.Common.Mappings;
using chatme.Domain.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace chatme.Application.Features.Chats.Queries.GetChatMessages
{
	public sealed class GetChatMessagesQueryHandler(
		IApplicationDbContext dbContext,
		ICurrentUserService currentUserService) : IRequestHandler<GetChatMessagesQuery, Result<List<MessageDto>>>
	{
		public async Task<Result<List<MessageDto>>> Handle(GetChatMessagesQuery request, CancellationToken cancellationToken)
		{
			var userId = currentUserService.UserId;
			if (userId is null)
				return Result<List<MessageDto>>.Unauthorized("لازم تسجل دخول الأول");

			var isParticipant = await dbContext.Chats
				.Where(c => c.Id == request.ChatId)
				.SelectMany(c => c.Participants)
				.AnyAsync(p => p.UserId == userId, cancellationToken);

			if (!isParticipant)
				return Result<List<MessageDto>>.Forbidden("إنت مش عضو في المحادثة دي");

			var messages = await dbContext.MessagesReadOnly
				.Include(m => m.Reactions)
				.Where(m => m.ChatId == request.ChatId)
				.OrderBy(m => m.SentAt)
				.Select(m => MessageProjections.Map(m))
				.ToListAsync(cancellationToken);

			return Result<List<MessageDto>>.Success(messages);
		}
	}
}
