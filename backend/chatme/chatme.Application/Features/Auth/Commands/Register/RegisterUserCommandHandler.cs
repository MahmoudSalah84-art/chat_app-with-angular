
using chatme.Application.Common;
using chatme.Application.Common.DTO;
using chatme.Application.Common.Interfaces;
using chatme.Domain.Common;
using MediatR;

namespace chatme.Application.Features.Auth.Commands.Register
{

	public sealed class RegisterUserCommandHandler(IIdentityService identityService)
		: IRequestHandler<RegisterUserCommand, Result<AuthResponseDto>>
	{
		public Task<Result<AuthResponseDto>> Handle(RegisterUserCommand request, CancellationToken cancellationToken) =>
			identityService.RegisterAsync(request.Name, request.Email, request.Password, cancellationToken);
	}

}
