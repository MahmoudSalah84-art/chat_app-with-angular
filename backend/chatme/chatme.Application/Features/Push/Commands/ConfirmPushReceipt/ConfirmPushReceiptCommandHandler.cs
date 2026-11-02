using chatme.Application.Common.Helper;
using chatme.Application.Common.Interfaces;
using chatme.Domain.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;


namespace chatme.Application.Features.Push.Commands.ConfirmPushReceipt
{
	public sealed class ConfirmPushReceiptCommandHandler(
	IApplicationDbContext dbContext)
	: IRequestHandler<ConfirmPushReceiptCommand, Result>
	{
		public async Task<Result> Handle(
			ConfirmPushReceiptCommand request,
			CancellationToken cancellationToken)
		{
			var tokenHash = ReceiptTokenHelper.Hash(request.ReceiptToken);

			// Validate receipt token first.
			var receipt = await dbContext.PushReceiptTokens
				.FirstOrDefaultAsync(
					x => x.TokenHash == tokenHash &&
					x.UsedAtUtc == null &&
					x.ExpiresAtUtc > DateTime.UtcNow,
					cancellationToken
				);


			if (receipt is null)
				return Result.Failure("Invalid receipt token");

			var chat = await dbContext.Chats
				.Include(x => x.Participants)
				.Include(x => x.Messages)
				.FirstOrDefaultAsync(x =>
					x.Id == receipt.ChatId,
					cancellationToken);

			if (chat is null)
				return Result.NotFound("Chat not found");

			var result = chat.MarkAsDelivered(
				receipt.ReceiverId,
				receipt.MessageId);

			if (result.IsFailure)
				return result;

			receipt.MarkAsUsed(DateTime.UtcNow);

			await dbContext.SaveChangesAsync(cancellationToken);

			return Result.Success();
		}
	}
}
