using chatme.Application.Common;
using chatme.Application.Common.DTO;
using chatme.Application.Common.Interfaces;
using chatme.Domain.Common;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace chatme.Application.Features.Users.Commands.UpdateProfile
{

	public sealed class UpdateProfileCommandHandler(
		IIdentityService identityService,
		ICurrentUserService currentUserService) : IRequestHandler<UpdateProfileCommand, Result<UserDto>>
	{
		public Task<Result<UserDto>> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
		{
			var userId = currentUserService.UserId;
			if (userId is null)
				return Task.FromResult(Result<UserDto>.Unauthorized("لازم تسجل دخول الأول"));

			return identityService.UpdateProfileAsync(userId.Value, request.Name, request.About, request.AvatarUrl, cancellationToken);
		}
	}

}
