using chatme.Domain.Common;

namespace chatme.Domain.Entities
{
	public sealed class RefreshToken : BaseEntity
	{
		public Guid UserId { get; private set; }
		public Guid FamilyId { get; private set; }

		public string TokenHash { get; private set; } = null!;

		public DateTime CreatedAtUtc { get; private set; }
		public DateTime ExpiresAtUtc { get; private set; }
		public DateTime? RevokedAtUtc { get; private set; }

		public Guid? ReplacedByTokenId { get; private set; }

		private RefreshToken() { }

		public static RefreshToken Create(Guid userId, Guid familyId, string tokenHash, DateTime now, TimeSpan lifetime) =>
			new()
			{
				Id = Guid.NewGuid(),
				UserId = userId,
				FamilyId = familyId,
				TokenHash = tokenHash,
				CreatedAtUtc = now,
				ExpiresAtUtc = now.Add(lifetime),
			};

		public bool IsRevoked => RevokedAtUtc is not null;

		public bool IsExpired(DateTime now) => ExpiresAtUtc <= now;

		public bool IsActive(DateTime now) => !IsRevoked && !IsExpired(now);
	}
}