using chatme.Domain.Common;
using MediatR;

namespace chatme.Application.Features.Push.Commands.UnsubscribePush
{
	public sealed record UnsubscribePushCommand(string Endpoint) : IRequest<Result>;
}