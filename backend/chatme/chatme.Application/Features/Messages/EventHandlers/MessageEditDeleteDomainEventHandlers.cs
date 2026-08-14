using chatme.Application.Common.DTO;
using chatme.Application.Common.Interfaces;
using chatme.Domain.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;


namespace chatme.Application.Features.Messages.EventHandlers
{

	public sealed class MessageEditedDomainEventHandler(
		IApplicationDbContext dbContext,
		IChatNotificationService notificationService) : INotificationHandler<MessageEditedDomainEvent>
	{
		public async Task Handle(MessageEditedDomainEvent notification, CancellationToken cancellationToken)
		{
			var message = await dbContext.MessagesReadOnly
				.Where(m => m.Id == notification.MessageId)
				.Select(m => new MessageDto(m.Id, m.ChatId, m.SenderId, m.Type, m.Content, m.SentAt, m.ReplyToMessageId, m.IsEdited, m.IsDeleted))
				.FirstOrDefaultAsync(cancellationToken);

			if (message is not null)
				await notificationService.NotifyMessageEditedAsync(notification.ChatId, message, cancellationToken);
		}
	}

	public sealed class MessageDeletedDomainEventHandler(
		IChatNotificationService notificationService) : INotificationHandler<MessageDeletedDomainEvent>
	{
		public Task Handle(MessageDeletedDomainEvent notification, CancellationToken cancellationToken) =>
			notificationService.NotifyMessageDeletedAsync(notification.ChatId, notification.MessageId, cancellationToken);
	}
}
