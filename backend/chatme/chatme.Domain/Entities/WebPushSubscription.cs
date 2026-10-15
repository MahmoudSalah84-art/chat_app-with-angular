using System.Security.Cryptography;
using System.Text;
using chatme.Domain.Common;

namespace chatme.Domain.Entities
{
	public sealed class WebPushSubscription : BaseEntity
	{
		public Guid UserId { get; private set; }
		public string Endpoint { get; private set; } = string.Empty;
		public string EndpointHash { get; private set; } = string.Empty;
		public string P256dh { get; private set; } = string.Empty;
		public string Auth { get; private set; } = string.Empty;
		public string? UserAgent { get; private set; }
		public DateTime CreatedAt { get; private set; }
		public DateTime LastSeenAt { get; private set; }

		private WebPushSubscription() { }

		public static string HashEndpoint(string endpoint) =>
			Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(endpoint)));

		public static WebPushSubscription Create(Guid userId, string endpoint, string p256dh, string auth, string? userAgent) => new()
		{
			Id = Guid.NewGuid(),
			UserId = userId,
			Endpoint = endpoint,
			EndpointHash = HashEndpoint(endpoint),
			P256dh = p256dh,
			Auth = auth,
			UserAgent = userAgent,
			CreatedAt = DateTime.UtcNow,
			LastSeenAt = DateTime.UtcNow,
		};

		// user may logout and login again, so we need to refresh the subscription with the new userId
		public void Refresh(Guid userId, string p256dh, string auth, string? userAgent)
		{
			UserId = userId;
			P256dh = p256dh;
			Auth = auth;
			UserAgent = userAgent;
			LastSeenAt = DateTime.UtcNow;
		}
	}
}