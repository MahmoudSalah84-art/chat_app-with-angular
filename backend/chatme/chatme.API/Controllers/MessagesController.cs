using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace chatme.API.Controllers
{
	[ApiController]
	[Route("api/messages")]
	[Authorize]
	public sealed class MessagesController(IMediator mediator) : ControllerBase
	{
		[HttpPost]
		public async Task<ActionResult<MessageDto>> Send(SendMessageCommand command, CancellationToken cancellationToken) =>
			Ok(await mediator.Send(command, cancellationToken));

		[HttpPut]
		public async Task<ActionResult<MessageDto>> Edit(EditMessageCommand command, CancellationToken cancellationToken) =>
			Ok(await mediator.Send(command, cancellationToken));

		[HttpDelete]
		public async Task<IActionResult> Delete([FromBody] DeleteMessageCommand command, CancellationToken cancellationToken)
		{
			await mediator.Send(command, cancellationToken);
			return NoContent();
		}
	}
}
