using chatme.Application.Common;
using chatme.Application.Common.DTO;
using chatme.Domain.Common;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace chatme.Application.Features.Auth.Commands.Login
{
	public sealed record LoginUserCommand(string Email, string Password) : IRequest<Result<AuthResponseDto>>;

}
