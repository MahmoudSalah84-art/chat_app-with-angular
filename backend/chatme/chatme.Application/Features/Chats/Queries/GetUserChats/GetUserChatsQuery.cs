using System;
using System.Collections.Generic;
using System.Text;

namespace chatme.Application.Features.Chats.Queries.GetUserChats
{

	public sealed record GetUserChatsQuery : IRequest<List<ChatDto>>;

}
