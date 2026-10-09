using chatme.Domain.Common;
using MediatR;

namespace chatme.Application.Features.Messages.Commands.ReactToMessage
{
	public sealed record ReactToMessageCommand(Guid ChatId, Guid MessageId, string Emoji) : IRequest<Result>;
}