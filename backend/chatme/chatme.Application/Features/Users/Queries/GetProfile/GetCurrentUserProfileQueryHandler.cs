using chatme.Application.Common;
using chatme.Application.Common.DTO;
using chatme.Application.Common.Interfaces;
using chatme.Domain.Common;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace chatme.Application.Features.Users.Queries.GetProfile
{

	public sealed class GetCurrentUserProfileQueryHandler(
		IIdentityService identityService,
		ICurrentUserService currentUserService) : IRequestHandler<GetCurrentUserProfileQuery, Result<UserDto>>
	{
		public Task<Result<UserDto>> Handle(GetCurrentUserProfileQuery request, CancellationToken cancellationToken)
		{
			var userId = currentUserService.UserId;
			if (userId is null)
				return Task.FromResult(Result<UserDto>.Unauthorized("لازم تسجل دخول الأول"));

			return identityService.GetUserByIdAsync(userId.Value, cancellationToken);
		}
	}
}
