using chatme.Application.Common;
using chatme.Domain.Common;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace chatme.Application.Features.Users.Commands.SetOnlineStatus
{
	public sealed record SetOnlineStatusCommand(bool IsOnline) : IRequest<Result>;

}
