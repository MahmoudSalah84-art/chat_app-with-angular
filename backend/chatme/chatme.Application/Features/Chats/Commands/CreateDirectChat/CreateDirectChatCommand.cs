using chatme.Application.Common;
using chatme.Application.Common.DTO;
using chatme.Domain.Common;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace chatme.Application.Features.Chats.Commands.CreateDirectChat
{

	public sealed record CreateDirectChatCommand(Guid OtherUserId) : IRequest<Result<ChatDto>>;

}
