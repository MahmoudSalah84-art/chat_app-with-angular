using chatme.Application.Common.Interfaces;
using chatme.Domain.Events;
using MediatR;

namespace chatme.Application.Features.Messages.EventHandlers
{
	public sealed class MessageReactionChangedDomainEventHandler(
		IChatNotificationService notificationService) : INotificationHandler<MessageReactionChangedDomainEvent>
	{
		public Task Handle(MessageReactionChangedDomainEvent e, CancellationToken cancellationToken) =>
			notificationService.NotifyMessageReactionChangedAsync(e.ChatId, e.MessageId, e.UserId, e.Emoji, cancellationToken);
	}
}