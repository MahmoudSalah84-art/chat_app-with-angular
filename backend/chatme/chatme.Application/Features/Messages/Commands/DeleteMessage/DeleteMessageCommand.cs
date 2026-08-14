using chatme.Application.Common;
using chatme.Domain.Common;
using MediatR;

namespace chatme.Application.Features.Messages.Commands.DeleteMessage
{
	public sealed record DeleteMessageCommand(Guid ChatId, Guid MessageId) : IRequest<Result>;

}