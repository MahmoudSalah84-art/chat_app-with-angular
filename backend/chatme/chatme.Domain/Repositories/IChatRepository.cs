using chatme.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace chatme.Domain.Repositories
{
	public interface IChatRepository
	{
		Task<Chat?> GetByIdWithDetailsAsync(Guid chatId, CancellationToken cancellationToken = default);
		Task<Chat?> GetDirectChatBetweenAsync(Guid userAId, Guid userBId, CancellationToken cancellationToken = default);
		void Add(Chat chat);
		void AddMessage(Message message);
	}
}
