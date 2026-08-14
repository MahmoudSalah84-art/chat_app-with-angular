using chatme.Application.Common.DTO;

namespace chatme.Application.Common.Interfaces
{
	public interface IChatNotificationService
	{
		Task NotifyMessageSentAsync(Guid chatId, MessageDto message, CancellationToken cancellationToken = default);
		Task NotifyMessageEditedAsync(Guid chatId, MessageDto message, CancellationToken cancellationToken = default);
		Task NotifyMessageDeletedAsync(Guid chatId, Guid messageId, CancellationToken cancellationToken = default);
		Task NotifyChatCreatedAsync(Guid chatId, IReadOnlyCollection<Guid> participantIds, CancellationToken cancellationToken = default);
	}
}
