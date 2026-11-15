using chatme.Application.Common.Helper;
using chatme.Application.Common.Interfaces;
using chatme.Domain.Common;
using chatme.Domain.Entities;
using chatme.infrastructure.Services.JwtToken;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace chatme.infrastructure.Services
{
	public sealed class RefreshTokenService(
		IApplicationDbContext dbContext,
		IOptions<JwtSettings> jwtSettings,
		ILogger<RefreshTokenService> logger) : IRefreshTokenService
	{
		private const string InvalidTokenMessage = "الجلسة انتهت، سجل دخول تاني";

		private TimeSpan Lifetime => TimeSpan.FromDays(jwtSettings.Value.RefreshTokenExpiryDays);

		public async Task<string> IssueAsync(Guid userId, CancellationToken cancellationToken = default)
		{
			var rawToken = ReceiptTokenHelper.Generate();

			var entity = RefreshToken.Create(
				userId,
				familyId: Guid.NewGuid(),
				ReceiptTokenHelper.Hash(rawToken),
				DateTime.UtcNow,
				Lifetime);

			await dbContext.RefreshTokens.AddAsync(entity, cancellationToken);
			await dbContext.SaveChangesAsync(cancellationToken);

			return rawToken;
		}

		public async Task<Result<RefreshTokenRotation>> RotateAsync(
			string refreshToken, CancellationToken cancellationToken = default)
		{
			var now = DateTime.UtcNow;
			var hash = ReceiptTokenHelper.Hash(refreshToken);

			var stored = await dbContext.RefreshTokens
				.AsNoTracking()
				.FirstOrDefaultAsync(t => t.TokenHash == hash, cancellationToken);

			if (stored is null)
				return Result<RefreshTokenRotation>.Unauthorized(InvalidTokenMessage);

			// If the token has already been revoked, it indicates a potential token reuse attack.
			if (stored.IsRevoked)
			{
				logger.LogWarning(
					"Refresh token reuse detected for user {UserId}, revoking family {FamilyId}",
					stored.UserId, stored.FamilyId);

				await RevokeFamilyAsync(stored.FamilyId, now, cancellationToken);
				return Result<RefreshTokenRotation>.Unauthorized(InvalidTokenMessage);
			}

			if (stored.IsExpired(now))
				return Result<RefreshTokenRotation>.Unauthorized(InvalidTokenMessage);

			var newRawToken = ReceiptTokenHelper.Generate();
			var replacement = RefreshToken.Create(
				stored.UserId,
				stored.FamilyId,
				ReceiptTokenHelper.Hash(newRawToken),
				now,
				Lifetime);

			var replacementId = replacement.Id;

			// Attempt to atomically revoke the old token and add the new one.
			// If the update count is 0, it means the token was already revoked by another process, indicating a potential token reuse attack.
			var claimed = await dbContext.RefreshTokens
				.Where(t => t.Id == stored.Id && t.RevokedAtUtc == null)
				.ExecuteUpdateAsync(setters => setters
					.SetProperty(t => t.RevokedAtUtc, (DateTime?)now)
					.SetProperty(t => t.ReplacedByTokenId, (Guid?)replacementId),
					cancellationToken);

			if (claimed == 0)
			{
				logger.LogWarning(
					"Concurrent refresh token reuse for user {UserId}, revoking family {FamilyId}",
					stored.UserId, stored.FamilyId);

				await RevokeFamilyAsync(stored.FamilyId, now, cancellationToken);
				return Result<RefreshTokenRotation>.Unauthorized(InvalidTokenMessage);
			}

			await dbContext.RefreshTokens.AddAsync(replacement, cancellationToken);
			await dbContext.SaveChangesAsync(cancellationToken);

			return Result<RefreshTokenRotation>.Success(new RefreshTokenRotation(stored.UserId, newRawToken));
		}

		public async Task RevokeAsync(string refreshToken, CancellationToken cancellationToken = default)
		{
			var hash = ReceiptTokenHelper.Hash(refreshToken);

			var familyId = await dbContext.RefreshTokens
				.AsNoTracking()
				.Where(t => t.TokenHash == hash)
				.Select(t => (Guid?)t.FamilyId)
				.FirstOrDefaultAsync(cancellationToken);

			if (familyId is not null)
				await RevokeFamilyAsync(familyId.Value, DateTime.UtcNow, cancellationToken);
		}

		public Task RevokeAllAsync(Guid userId, CancellationToken cancellationToken = default) =>
			dbContext.RefreshTokens
				.Where(t => t.UserId == userId && t.RevokedAtUtc == null)
				.ExecuteUpdateAsync(
					setters => setters.SetProperty(t => t.RevokedAtUtc, (DateTime?)DateTime.UtcNow),
					cancellationToken);

		private Task<int> RevokeFamilyAsync(Guid familyId, DateTime now, CancellationToken cancellationToken) =>
			dbContext.RefreshTokens
				.Where(t => t.FamilyId == familyId && t.RevokedAtUtc == null)
				.ExecuteUpdateAsync(
					setters => setters.SetProperty(t => t.RevokedAtUtc, (DateTime?)now),
					cancellationToken);
	}
}