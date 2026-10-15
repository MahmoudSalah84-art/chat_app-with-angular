using chatme.Domain.Common;
using MediatR;

namespace chatme.Application.Features.Push.Commands.SubscribePush
{
	public sealed record SubscribePushCommand(string Endpoint, string P256dh, string Auth, string? UserAgent) : IRequest<Result>;
}