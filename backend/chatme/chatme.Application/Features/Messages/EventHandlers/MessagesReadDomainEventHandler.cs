using chatme.Application.Common.Interfaces;
using chatme.Domain.Events;
using MediatR;

namespace chatme.Application.Features.Messages.EventHandlers
{
	public sealed class MessagesReadDomainEventHandler(
		IChatNotificationService notificationService) : INotificationHandler<MessagesReadDomainEvent>
	{
		public Task Handle(MessagesReadDomainEvent e, CancellationToken cancellationToken) =>
			notificationService.NotifyMessagesReadAsync(e.ChatId, e.UserId, e.UpToSentAt, cancellationToken);
	}
}