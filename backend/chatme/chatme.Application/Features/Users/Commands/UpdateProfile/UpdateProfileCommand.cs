using chatme.Application.Common;
using chatme.Application.Common.DTO;
using chatme.Domain.Common;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace chatme.Application.Features.Users.Commands.UpdateProfile
{
	public sealed record UpdateProfileCommand(string? Name, string? About, string? AvatarUrl) : IRequest<Result<UserDto>>;

}
