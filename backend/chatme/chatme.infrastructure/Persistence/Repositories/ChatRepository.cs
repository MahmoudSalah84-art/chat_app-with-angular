using chatme.Domain.Entities;
using chatme.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace chatme.infrastructure.Persistence.Repositories
{

	public sealed class ChatRepository(ApplicationDbContext dbContext) : IChatRepository
	{
		public Task<Chat?> GetByIdWithDetailsAsync(Guid chatId, CancellationToken cancellationToken = default) =>
			dbContext.Chats.Include(c => c.Participants).Include(c => c.Messages).FirstOrDefaultAsync(c => c.Id == chatId, cancellationToken);

		public async Task<Chat?> GetDirectChatBetweenAsync(Guid userAId, Guid userBId, CancellationToken cancellationToken = default)
		{
			var candidates = await dbContext.Chats
				.Include(c => c.Participants)
				.Include(c => c.Messages)
				.Where(c => !c.IsGroup && c.Participants.Any(p => p.UserId == userAId))
				.ToListAsync(cancellationToken);

			return candidates.FirstOrDefault(c => c.Participants.Any(p => p.UserId == userBId));
		}

		public void Add(Chat chat) => dbContext.Chats.Add(chat);
		public void AddMessage(Message message) => dbContext.Messages.Add(message);



		public Task<Chat?> GetByIdForReactionAsync(Guid chatId, Guid messageId, CancellationToken cancellationToken = default) =>
			dbContext.Chats
				.Include(c => c.Participants)
				.Include(c => c.Messages.Where(m => m.Id == messageId))
					.ThenInclude(m => m.Reactions)
				.FirstOrDefaultAsync(c => c.Id == chatId, cancellationToken);

		public void AddReaction(MessageReaction reaction) => dbContext.Add(reaction);
	}
}