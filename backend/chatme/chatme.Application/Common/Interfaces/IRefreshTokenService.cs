using chatme.Domain.Common;

namespace chatme.Application.Common.Interfaces
{
	public sealed record RefreshTokenRotation(Guid UserId, string NewRefreshToken);

	public interface IRefreshTokenService
	{
		Task<string> IssueAsync(Guid userId, CancellationToken cancellationToken = default);

		/// <summary>
		/// بيبدّل التوكن القديم بتوكن جديد في نفس العيلة. لو التوكن مش صالح بيرجع Unauthorized،
		/// ولو اتستخدم بعد ما اتلغى (احتمال سرقة) بيلغي العيلة كلها.
		/// </summary>
		Task<Result<RefreshTokenRotation>> RotateAsync(string refreshToken, CancellationToken cancellationToken = default);

		/// <summary> it revokes the refresh token.</summary>
		Task RevokeAsync(string refreshToken, CancellationToken cancellationToken = default);

		/// <summary> it revokes all refresh tokens for the user.</summary>
		Task RevokeAllAsync(Guid userId, CancellationToken cancellationToken = default);
	}
}
