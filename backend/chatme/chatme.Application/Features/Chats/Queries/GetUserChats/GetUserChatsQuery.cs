using chatme.Application.Common;
using chatme.Application.Common.DTO;
using chatme.Domain.Common;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace chatme.Application.Features.Chats.Queries.GetUserChats
{

	public sealed record GetUserChatsQuery : IRequest<Result<List<ChatDto>>>;


}
