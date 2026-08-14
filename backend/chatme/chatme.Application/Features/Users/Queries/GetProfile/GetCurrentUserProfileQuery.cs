using chatme.Application.Common;
using chatme.Application.Common.DTO;
using chatme.Domain.Common;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace chatme.Application.Features.Users.Queries.GetProfile
{
	public sealed record GetCurrentUserProfileQuery : IRequest<Result<UserDto>>;

}
