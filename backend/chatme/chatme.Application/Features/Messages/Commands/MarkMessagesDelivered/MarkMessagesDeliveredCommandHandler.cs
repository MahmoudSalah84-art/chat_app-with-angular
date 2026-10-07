using chatme.Application.Common.Interfaces;
using chatme.Domain.Common;
using chatme.Domain.Repositories;
using MediatR;

namespace chatme.Application.Features.Messages.Commands.MarkMessagesDelivered
{
	public sealed class MarkMessagesDeliveredCommandHandler(
		IChatRepository chatRepository,
		IUnitOfWork unitOfWork,
		ICurrentUserService currentUserService) : IRequestHandler<MarkMessagesDeliveredCommand, Result>
	{
		public async Task<Result> Handle(MarkMessagesDeliveredCommand request, CancellationToken cancellationToken)
		{
			var userId = currentUserService.UserId;
			if (userId is null)
				return Result.Unauthorized("لازم تسجل دخول الأول");

			var chat = await chatRepository.GetByIdWithDetailsAsync(request.ChatId, cancellationToken);
			if (chat is null)
				return Result.NotFound("المحادثة دي مش موجودة");

			var result = chat.MarkAsDelivered(userId.Value, request.UpToMessageId);
			if (result.IsFailure)
				return result;

			await unitOfWork.SaveChangesAsync(cancellationToken);
			return Result.Success();
		}
	}
}