using chatme.Application.Features.Push.Commands.SubscribePush;
using chatme.Application.Features.Push.Commands.UnsubscribePush;
using chatme.infrastructure.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace chatme.API.Controllers
{
	public sealed record PushKeysRequest(string P256dh, string Auth);
	public sealed record SubscribePushRequest(string Endpoint, PushKeysRequest Keys);
	public sealed record UnsubscribePushRequest(string Endpoint);
	public sealed record PublicKeyResponse(string PublicKey);

	[Route("api/push")]
	[Authorize]
	public sealed class PushController(IMediator mediator, IOptions<WebPushSettings> options) : ApiControllerBase
	{
		[HttpGet("public-key")]
		public ActionResult<PublicKeyResponse> GetPublicKey() =>
			string.IsNullOrWhiteSpace(options.Value.PublicKey)
				? StatusCode(StatusCodes.Status503ServiceUnavailable)
				: Ok(new PublicKeyResponse(options.Value.PublicKey));

		[HttpPost("subscribe")]
		public async Task<IActionResult> Subscribe(SubscribePushRequest request, CancellationToken cancellationToken) =>
			HandleResult(await mediator.Send(new SubscribePushCommand(
				request.Endpoint,
				request.Keys.P256dh,
				request.Keys.Auth,
				Request.Headers.UserAgent.ToString()), cancellationToken));

		[HttpPost("unsubscribe")]
		public async Task<IActionResult> Unsubscribe(UnsubscribePushRequest request, CancellationToken cancellationToken) =>
			HandleResult(await mediator.Send(new UnsubscribePushCommand(request.Endpoint), cancellationToken));
	}
}