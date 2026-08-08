using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace chatme.API.Controllers
{
	[ApiController]
	[Route("api/auth")]
	public sealed class AuthController(IMediator mediator) : ControllerBase
	{
		[HttpPost("register")]
		public async Task<ActionResult<AuthResponseDto>> Register(RegisterUserCommand command, CancellationToken cancellationToken) =>
			Ok(await mediator.Send(command, cancellationToken));

		[HttpPost("login")]
		public async Task<ActionResult<AuthResponseDto>> Login(LoginUserCommand command, CancellationToken cancellationToken) =>
			Ok(await mediator.Send(command, cancellationToken));
	}
}