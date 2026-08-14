using chatme.Application.Common;
using chatme.Application.Common.DTO;
using chatme.Domain.Common;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace chatme.Application.Features.Messages.Commands.EditMessage
{
	public sealed record EditMessageCommand(Guid ChatId, Guid MessageId, string NewContent) : IRequest<Result<MessageDto>>;

}
