using chatme.API.Hubs;
using chatme.Application.Common.DTO;
using chatme.Application.Common.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace chatme.API.Services
{
	public sealed class SignalRChatNotificationService(IHubContext<ChatHub> hubContext) : IChatNotificationService
	{
		public Task NotifyMessageSentAsync(Guid chatId, MessageDto message, CancellationToken cancellationToken = default) =>
			hubContext.Clients.Group(GroupName(chatId)).SendAsync("MessageReceived", message, cancellationToken);

		public Task NotifyMessageEditedAsync(Guid chatId, MessageDto message, CancellationToken cancellationToken = default) =>
			hubContext.Clients.Group(GroupName(chatId)).SendAsync("MessageEdited", message, cancellationToken);

		public Task NotifyMessageDeletedAsync(Guid chatId, Guid messageId, CancellationToken cancellationToken = default) =>
			hubContext.Clients.Group(GroupName(chatId)).SendAsync("MessageDeleted", chatId, messageId, cancellationToken);

		public Task NotifyChatCreatedAsync(Guid chatId, IReadOnlyCollection<Guid> participantIds, CancellationToken cancellationToken = default) =>
			hubContext.Clients.Users(participantIds.Select(id => id.ToString())).SendAsync("ChatCreated", chatId, cancellationToken);

		public static string GroupName(Guid chatId) => $"chat-{chatId}";
	}
}
