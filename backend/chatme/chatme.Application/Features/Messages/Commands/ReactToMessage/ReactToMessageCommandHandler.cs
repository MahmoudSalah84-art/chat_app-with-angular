using chatme.Application.Common.Interfaces;
using chatme.Domain.Common;
using chatme.Domain.Repositories;
using MediatR;

namespace chatme.Application.Features.Messages.Commands.ReactToMessage
{
	public sealed class ReactToMessageCommandHandler(
		IChatRepository chatRepository,
		IUnitOfWork unitOfWork,
		ICurrentUserService currentUserService) : IRequestHandler<ReactToMessageCommand, Result>
	{
		public async Task<Result> Handle(ReactToMessageCommand request, CancellationToken cancellationToken)
		{
			var userId = currentUserService.UserId;
			if (userId is null)
				return Result.Unauthorized("لازم تسجل دخول الأول");

			var chat = await chatRepository.GetByIdForReactionAsync(request.ChatId, request.MessageId, cancellationToken);
			if (chat is null)
				return Result.NotFound("المحادثة دي مش موجودة");

			var result = chat.ReactToMessage(userId.Value, request.MessageId, request.Emoji);
			if (result.IsFailure)
				return result;

			if (result.Value!.Added is { } added)
				chatRepository.AddReaction(added);

			await unitOfWork.SaveChangesAsync(cancellationToken);
			return Result.Success();
		}
	}
}