using chatme.Application.Common.DTO;
using chatme.Application.Features.Auth.Commands.Login;
using chatme.Application.Features.Auth.Commands.Register;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace chatme.API.Controllers
{
	[Route("api/auth")]
	public sealed class AuthController(IMediator mediator) : ApiControllerBase
	{
		[HttpPost("register")]
		public async Task<ActionResult<AuthResponseDto>> Register(RegisterUserCommand command, CancellationToken cancellationToken) =>
			HandleResult(await mediator.Send(command, cancellationToken));

		[HttpPost("login")]
		public async Task<ActionResult<AuthResponseDto>> Login(LoginUserCommand command, CancellationToken cancellationToken) =>
			HandleResult(await mediator.Send(command, cancellationToken));
	}

}

