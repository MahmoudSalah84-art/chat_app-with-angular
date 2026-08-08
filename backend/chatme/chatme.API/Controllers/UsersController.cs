using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace chatme.API.Controllers
{
	[ApiController]
	[Route("api/users")]
	[Authorize]
	public sealed class UsersController(IMediator mediator) : ControllerBase
	{
		[HttpGet("me")]
		public async Task<ActionResult<UserDto>> GetProfile(CancellationToken cancellationToken) =>
			Ok(await mediator.Send(new GetCurrentUserProfileQuery(), cancellationToken));

		[HttpPut("me")]
		public async Task<ActionResult<UserDto>> UpdateProfile(UpdateProfileCommand command, CancellationToken cancellationToken) =>
			Ok(await mediator.Send(command, cancellationToken));

		[HttpGet("contacts")]
		public async Task<ActionResult<List<UserDto>>> GetContacts(CancellationToken cancellationToken) =>
			Ok(await mediator.Send(new GetContactsQuery(), cancellationToken));
	}
}
