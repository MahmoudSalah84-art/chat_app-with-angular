using chatme.Domain.Entities;

namespace chatme.Domain.Repositories
{
	public interface IChatRepository
	{
		Task<Chat?> GetByIdWithDetailsAsync(Guid chatId, CancellationToken cancellationToken = default);
		Task<Chat?> GetDirectChatBetweenAsync(Guid userAId, Guid userBId, CancellationToken cancellationToken = default);
		void Add(Chat chat);
		void AddMessage(Message message);

		Task<Chat?> GetByIdForReactionAsync(Guid chatId, Guid messageId, CancellationToken cancellationToken = default);
		void AddReaction(MessageReaction reaction);
	}
}
