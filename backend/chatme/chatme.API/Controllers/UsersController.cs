using chatme.Application.Common.DTO;
using chatme.Application.Features.Users.Commands.UpdateProfile;
using chatme.Application.Features.Users.Commands.UploadAvatar;
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

		[HttpPost("me/avatar")]
		[Consumes("multipart/form-data")]
		[RequestSizeLimit(6_000_000)]
		[RequestFormLimits(MultipartBodyLengthLimit = 6_000_000)]
		public async Task<ActionResult<UserDto>> UploadAvatar(
			[FromForm] UploadAvatarRequest request,
			CancellationToken cancellationToken)
		{
			await using var stream = request.File.OpenReadStream();

			var command = new UploadAvatarCommand(
				stream,
				Path.GetFileName(request.File.FileName),
				request.File.Length);

			return HandleResult(
				await mediator.Send(command, cancellationToken));
		}
	}

	public sealed class UploadAvatarRequest
	{
		public IFormFile File { get; init; } = default!;
	}

}
