using chatme.Application.Common;
using chatme.Application.Common.DTO;
using chatme.Domain.Common;
using chatme.Domain.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace chatme.Application.Features.Messages.Commands.SendMessage
{
	public sealed record SendMessageCommand(Guid ChatId, MessageType Type, string Content, Guid? ReplyToMessageId) : IRequest<Result<MessageDto>>;

}
