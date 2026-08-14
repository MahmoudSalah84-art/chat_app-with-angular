using chatme.Application.Common;
using chatme.Application.Common.DTO;
using chatme.Domain.Common;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace chatme.Application.Features.Chats.Commands.CreateGroupChat
{
	public sealed record CreateGroupChatCommand(string Name, string? AvatarUrl, List<Guid> MemberIds) : IRequest<Result<ChatDto>>;

}
