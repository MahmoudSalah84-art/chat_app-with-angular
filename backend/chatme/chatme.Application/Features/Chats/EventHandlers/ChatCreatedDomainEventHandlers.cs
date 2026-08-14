using chatme.Application.Common.Interfaces;
using chatme.Domain.Events;
using MediatR;


namespace chatme.Application.Features.Chats.EventHandlers
{

	public sealed class DirectChatCreatedDomainEventHandler(
		IChatNotificationService notificationService) : INotificationHandler<DirectChatCreatedDomainEvent>
	{
		public Task Handle(DirectChatCreatedDomainEvent notification, CancellationToken cancellationToken) =>
			notificationService.NotifyChatCreatedAsync(notification.ChatId, notification.ParticipantIds, cancellationToken);
	}

	public sealed class GroupChatCreatedDomainEventHandler(
		IChatNotificationService notificationService) : INotificationHandler<GroupChatCreatedDomainEvent>
	{
		public Task Handle(GroupChatCreatedDomainEvent notification, CancellationToken cancellationToken) =>
			notificationService.NotifyChatCreatedAsync(notification.ChatId, notification.ParticipantIds, cancellationToken);
	}

}


