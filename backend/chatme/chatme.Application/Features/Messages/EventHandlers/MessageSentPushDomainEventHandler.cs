using chatme.Application.Common.Interfaces;
using chatme.Domain.Events;
using MediatR;

namespace chatme.Application.Features.Messages.EventHandlers
{
	public sealed class MessageSentPushDomainEventHandler(IPushJobQueue queue) : INotificationHandler<MessageSentDomainEvent>
	{
		public Task Handle(MessageSentDomainEvent notification, CancellationToken cancellationToken)
		{
			queue.Enqueue(new MessagePushJob(notification.ChatId, notification.MessageId, notification.SenderId));
			return Task.CompletedTask;
		}
	}
}