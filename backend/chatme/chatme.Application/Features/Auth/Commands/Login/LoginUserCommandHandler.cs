using chatme.Application.Common;
using chatme.Application.Common.DTO;
using chatme.Application.Common.Interfaces;
using chatme.Domain.Common;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace chatme.Application.Features.Auth.Commands.Login
{

	public sealed class LoginUserCommandHandler(IIdentityService identityService)
		: IRequestHandler<LoginUserCommand, Result<AuthResponseDto>>
	{
		public Task<Result<AuthResponseDto>> Handle(LoginUserCommand request, CancellationToken cancellationToken) =>
			identityService.LoginAsync(request.Email, request.Password, cancellationToken);
	}

}
