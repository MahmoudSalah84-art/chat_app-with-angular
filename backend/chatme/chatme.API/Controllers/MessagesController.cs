using chatme.Application.Common.DTO;
using chatme.Application.Features.Messages.Commands.DeleteMessage;
using chatme.Application.Features.Messages.Commands.EditMessage;
using chatme.Application.Features.Messages.Commands.SendMessage;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace chatme.API.Controllers
{

	[Route("api/messages")]
	[Authorize]
	public sealed class MessagesController(IMediator mediator) : ApiControllerBase
	{
		[HttpPost]
		public async Task<ActionResult<MessageDto>> Send(SendMessageCommand command, CancellationToken cancellationToken) =>
			HandleResult(await mediator.Send(command, cancellationToken));

		[HttpPut]
		public async Task<ActionResult<MessageDto>> Edit(EditMessageCommand command, CancellationToken cancellationToken) =>
			HandleResult(await mediator.Send(command, cancellationToken));

		[HttpDelete]
		public async Task<IActionResult> Delete([FromBody] DeleteMessageCommand command, CancellationToken cancellationToken) =>
			HandleResult(await mediator.Send(command, cancellationToken));
	}

}
