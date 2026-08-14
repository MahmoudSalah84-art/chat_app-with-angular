using chatme.Application.Common;
using chatme.Domain.Common;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace chatme.Application.Features.Chats.Commands.MarkChatAsRead
{
	public sealed record MarkChatAsReadCommand(Guid ChatId, Guid LastReadMessageId) : IRequest<Result>;

}
