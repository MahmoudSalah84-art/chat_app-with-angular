using chatme.API.Services;
using chatme.Application.Common.DTO;
using chatme.Application.Common.Interfaces;
using chatme.Application.Features.Chats.Commands.MarkChatAsRead;
using chatme.Application.Features.Chats.Queries.GetUserChats;
using chatme.Application.Features.Messages.Commands.DeleteMessage;
using chatme.Application.Features.Messages.Commands.EditMessage;
using chatme.Application.Features.Messages.Commands.SendMessage;
using chatme.Application.Features.Users.Commands.SetOnlineStatus;
using chatme.Domain.Common;
using chatme.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace chatme.API.Hubs
{

	[Authorize]
	public sealed class ChatHub(IMediator mediator , ICurrentUserService currentUser) : Hub
	{
		public override async Task OnConnectedAsync()
		{
			try
			{
				Console.WriteLine($"SignalR Connected: {Context.ConnectionId}");

				await mediator.Send(new SetOnlineStatusCommand(true));

				var chatsResult = await mediator.Send(new GetUserChatsQuery());

				if (chatsResult.IsSuccess)
				{
					foreach (var chat in chatsResult.Value!)
					{
						await Groups.AddToGroupAsync(
							Context.ConnectionId,
							SignalRChatNotificationService.GroupName(chat.Id));
					}
				}

				await base.OnConnectedAsync();
			}
			catch (Exception ex)
			{
				Console.WriteLine($"SignalR OnConnectedAsync ERROR: {ex}");

				throw;
			}
		}

		public override async Task OnDisconnectedAsync(Exception? exception)
		{
			await mediator.Send(new SetOnlineStatusCommand(false));
			await base.OnDisconnectedAsync(exception);
		}

		public async Task<MessageDto> SendMessage(Guid chatId, MessageType type, string content, Guid? replyToMessageId) =>
			Unwrap(await mediator.Send(new SendMessageCommand(chatId, type, content, replyToMessageId)));

		public async Task<MessageDto> EditMessage(Guid chatId, Guid messageId, string newContent) =>
			Unwrap(await mediator.Send(new EditMessageCommand(chatId, messageId, newContent)));

		public async Task DeleteMessage(Guid chatId, Guid messageId) =>
			Unwrap(await mediator.Send(new DeleteMessageCommand(chatId, messageId)));

		public async Task MarkAsRead(Guid chatId, Guid lastReadMessageId) =>
			Unwrap(await mediator.Send(new MarkChatAsReadCommand(chatId, lastReadMessageId)));

		public Task StartTyping(Guid chatId) =>
			Clients.OthersInGroup(SignalRChatNotificationService.GroupName(chatId)).SendAsync("UserTyping", chatId, GetUserId());

		public Task StopTyping(Guid chatId) =>
			Clients.OthersInGroup(SignalRChatNotificationService.GroupName(chatId)).SendAsync("UserStoppedTyping", chatId, GetUserId());

		public Task JoinChatGroup(Guid chatId) =>
			Groups.AddToGroupAsync(Context.ConnectionId, SignalRChatNotificationService.GroupName(chatId));

		

		private static T Unwrap<T>(Result<T> result)
		{
			if (result.IsFailure)
				throw new HubException(string.Join(" - ", result.Errors));

			return result.Value!;
		}

		private static void Unwrap(Result result)
		{
			if (result.IsFailure)
				throw new HubException(string.Join(" - ", result.Errors));
		}

		private Guid? GetUserId()
		{
			return currentUser.UserId ;
		}
	}
}
