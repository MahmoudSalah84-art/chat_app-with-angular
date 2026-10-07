using chatme.Application.Common;
using chatme.Application.Common.Interfaces;
using chatme.Domain.Common;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace chatme.Application.Features.Users.Commands.SetOnlineStatus
{
	public sealed class SetOnlineStatusCommandHandler(
		IIdentityService identityService,
		ICurrentUserService currentUserService,
		IChatNotificationService chatNotificationService) : IRequestHandler<SetOnlineStatusCommand, Result>
	{
		public async Task<Result> Handle(SetOnlineStatusCommand request, CancellationToken cancellationToken)
		{
			var userId = currentUserService.UserId;

			if (userId is null)
				return Result.Success();

			var result = await identityService.SetOnlineStatusAsync(userId.Value, request.IsOnline, cancellationToken);
			if (result.IsSuccess)
			{
				var lastSeen = request.IsOnline ? (DateTime?)null : DateTime.UtcNow;
				await chatNotificationService.NotifyUserStatusChangedAsync(userId.Value, request.IsOnline, lastSeen, cancellationToken);
			}

			return result;
		}
	}

}
