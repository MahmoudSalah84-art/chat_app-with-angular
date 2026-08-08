using System;
using System.Collections.Generic;
using System.Text;

namespace chatme.Application.Features.Chats.Queries.GetChatMessages
{
	public sealed record GetChatMessagesQuery(Guid ChatId) : IRequest<List<MessageDto>>;

}
