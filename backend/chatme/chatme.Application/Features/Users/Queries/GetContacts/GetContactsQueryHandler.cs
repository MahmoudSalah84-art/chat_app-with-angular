using chatme.Application.Common;
using chatme.Application.Common.DTO;
using chatme.Application.Common.Interfaces;
using chatme.Domain.Common;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace chatme.Application.Features.Users.Queries.GetContacts
{

	public sealed class GetContactsQueryHandler(
		IIdentityService identityService,
		ICurrentUserService currentUserService) : IRequestHandler<GetContactsQuery, Result<List<UserDto>>>
	{
		public async Task<Result<List<UserDto>>> Handle(GetContactsQuery request, CancellationToken cancellationToken)
		{
			var userId = currentUserService.UserId;
			if (userId is null)
				return Result<List<UserDto>>.Unauthorized("لازم تسجل دخول الأول");

			var contacts = await identityService.GetContactsAsync(userId.Value, cancellationToken);
			return Result<List<UserDto>>.Success(contacts);
		}
	}

}
