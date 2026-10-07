using chatme.Application.Common.Interfaces;
using chatme.Domain.Events;
using MediatR;
 

namespace chatme.Application.Features.Messages.EventHandlers
{
	public sealed class MessagesDeliveredDomainEventHandler(
		IChatNotificationService notificationService) : INotificationHandler<MessagesDeliveredDomainEvent>
	{
		public Task Handle(MessagesDeliveredDomainEvent e, CancellationToken cancellationToken) =>
			notificationService.NotifyMessagesDeliveredAsync(e.ChatId, e.UserId, e.UpToSentAt, cancellationToken);
	}
}
