using chatme.Application.Common.DTO;
using chatme.Application.Common.Interfaces;
using chatme.Application.Common.Mappings;
using chatme.Domain.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace chatme.Application.Features.Messages.EventHandlers
{

	public sealed class MessageSentDomainEventHandler(
		IApplicationDbContext dbContext,
		IChatNotificationService notificationService) : INotificationHandler<MessageSentDomainEvent>
	{
		public async Task Handle(MessageSentDomainEvent notification, CancellationToken cancellationToken)
		{
			var message = await dbContext.MessagesReadOnly
				.Where(m => m.Id == notification.MessageId)
				.Select(m => MessageProjections.Map(m))
				.FirstOrDefaultAsync(cancellationToken);

			if (message is not null)
				await notificationService.NotifyMessageSentAsync(notification.ChatId, message, cancellationToken);
		}
	}
}
