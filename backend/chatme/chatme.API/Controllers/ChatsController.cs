using chatme.Application.Common.DTO;
using chatme.Application.Features.Chats.Commands.CreateDirectChat;
using chatme.Application.Features.Chats.Commands.CreateGroupChat;
using chatme.Application.Features.Chats.Commands.MarkChatAsRead;
using chatme.Application.Features.Chats.Queries.GetChatMessages;
using chatme.Application.Features.Chats.Queries.GetUserChats;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace chatme.API.Controllers
{

	[Route("api/chats")]
	[Authorize]
	public sealed class ChatsController(IMediator mediator) : ApiControllerBase
	{
		[HttpGet]
		public async Task<ActionResult<List<ChatDto>>> GetChats(CancellationToken cancellationToken) =>
			HandleResult(await mediator.Send(new GetUserChatsQuery(), cancellationToken));

		[HttpGet("{chatId:guid}/messages")]
		public async Task<ActionResult<List<MessageDto>>> GetMessages(Guid chatId, CancellationToken cancellationToken) =>
			HandleResult(await mediator.Send(new GetChatMessagesQuery(chatId), cancellationToken));

		[HttpPost("direct")]
		public async Task<ActionResult<ChatDto>> CreateDirectChat(CreateDirectChatCommand command, CancellationToken cancellationToken) =>
			HandleResult(await mediator.Send(command, cancellationToken));

		[HttpPost("group")]
		public async Task<ActionResult<ChatDto>> CreateGroupChat(CreateGroupChatCommand command, CancellationToken cancellationToken) =>
			HandleResult(await mediator.Send(command, cancellationToken));

		[HttpPost("{chatId:guid}/read")]
		public async Task<IActionResult> MarkAsRead(Guid chatId, [FromBody] Guid lastReadMessageId, CancellationToken cancellationToken) =>
			HandleResult(await mediator.Send(new MarkChatAsReadCommand(chatId, lastReadMessageId), cancellationToken));
	}

}
