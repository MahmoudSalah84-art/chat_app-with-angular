using chatme.Application.Common.DTO;
using chatme.Application.Features.Users.Commands.UpdateProfile;
using chatme.Application.Features.Users.Queries.GetContacts;
using chatme.Application.Features.Users.Queries.GetProfile;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace chatme.API.Controllers
{

	[Route("api/users")]
	[Authorize]
	public sealed class UsersController(IMediator mediator) : ApiControllerBase
	{
		[HttpGet("me")]
		public async Task<ActionResult<UserDto>> GetProfile(CancellationToken cancellationToken) =>
			HandleResult(await mediator.Send(new GetCurrentUserProfileQuery(), cancellationToken));

		[HttpPut("me")]
		public async Task<ActionResult<UserDto>> UpdateProfile(UpdateProfileCommand command, CancellationToken cancellationToken) =>
			HandleResult(await mediator.Send(command, cancellationToken));

		[HttpGet("contacts")]
		public async Task<ActionResult<List<UserDto>>> GetContacts(CancellationToken cancellationToken) =>
			HandleResult(await mediator.Send(new GetContactsQuery(), cancellationToken));
	}

}
