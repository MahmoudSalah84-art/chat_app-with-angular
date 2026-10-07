using chatme.Application.Common.Interfaces;
using chatme.Domain.Common;
using chatme.Domain.Repositories;
using MediatR;

namespace chatme.Application.Features.Messages.Commands.MarkMessagesDelivered
{
	public sealed record MarkMessagesDeliveredCommand(Guid ChatId, Guid UpToMessageId) : IRequest<Result>;

	
}