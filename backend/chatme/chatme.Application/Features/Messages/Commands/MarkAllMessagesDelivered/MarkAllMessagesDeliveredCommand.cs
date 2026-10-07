using chatme.Domain.Common;
using MediatR;

namespace chatme.Application.Features.Messages.Commands.MarkAllMessagesDelivered
{
	public sealed record MarkAllMessagesDeliveredCommand : IRequest<Result>;

}
