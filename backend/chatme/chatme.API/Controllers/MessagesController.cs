using chatme.Application.Common.DTO;
using chatme.Application.Features.Messages.Commands.DeleteMessage;
using chatme.Application.Features.Messages.Commands.EditMessage;
using chatme.Application.Features.Messages.Commands.SendAttachment;
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


		[HttpPost("attachments")]
		[Consumes("multipart/form-data")]
		[RequestSizeLimit(30_000_000)] // for size of the request body
		[RequestFormLimits(MultipartBodyLengthLimit = 30_000_000)] // for size of the file
		public async Task<ActionResult<MessageDto>> SendAttachment(
			[FromForm] SendAttachmentRequest request, CancellationToken cancellationToken)
		{
			await using var stream = request.File.OpenReadStream();

			var command = new SendAttachmentCommand(
				request.ChatId,
				stream,
				Path.GetFileName(request.File.FileName),
				request.File.ContentType,
				request.File.Length,
				request.Caption,
				request.ReplyToMessageId);

			return HandleResult(await mediator.Send(command, cancellationToken));
		}
	}
	public sealed class SendAttachmentRequest
	{
		public Guid ChatId { get; init; }
		public IFormFile File { get; init; } = default!;
		public string? Caption { get; init; }
		public Guid? ReplyToMessageId { get; init; }
	}
}
