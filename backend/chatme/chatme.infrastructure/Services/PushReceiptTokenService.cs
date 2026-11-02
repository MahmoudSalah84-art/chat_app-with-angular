using chatme.Application.Common.Helper;
using chatme.Application.Common.Interfaces;
using chatme.Domain.Entities;

namespace chatme.infrastructure.Services
{
	public sealed class PushReceiptTokenService(
	IApplicationDbContext dbContext) : IPushReceiptTokenService
	{
		public async Task<string> CreateAsync(
			Guid chatId,
			Guid messageId,
			Guid receiverId,
			CancellationToken cancellationToken)
		{
			var token = ReceiptTokenHelper.Generate();

			var tokenHash = ReceiptTokenHelper.Hash(token);

			var receipt = PushReceiptToken.Create(
				chatId,
				messageId,
				receiverId,
				tokenHash);

			await dbContext.PushReceiptTokens.AddAsync(
				receipt,
				cancellationToken);

			await dbContext.SaveChangesAsync(cancellationToken);

			return token;
		}
	}
}
