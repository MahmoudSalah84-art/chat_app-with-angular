using chatme.Application.Common;
using chatme.Application.Common.DTO;
using chatme.Domain.Common;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace chatme.Application.Features.Chats.Queries.GetChatMessages
{
	public sealed record GetChatMessagesQuery(Guid ChatId) : IRequest<Result<List<MessageDto>>>;


}
