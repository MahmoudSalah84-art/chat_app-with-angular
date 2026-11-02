using System.Collections.Concurrent;
using System.Net;
using System.Text.Encodings.Web;
using System.Text.Json;
using chatme.Application.Common.Interfaces;
using chatme.infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WebPush;

namespace chatme.infrastructure.Services
{
	public sealed class WebPushSender(
	ApplicationDbContext dbContext,
	WebPushClient client,
	IOptions<WebPushSettings> options,
	ILogger<WebPushSender> logger) : IWebPushSender
	{
		private const int TimeToLiveSeconds = 24 * 60 * 60;

		private static readonly JsonSerializerOptions JsonOptions =
			new(JsonSerializerDefaults.Web)
			{
				Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
			};

		public async Task SendAsync(
			Guid userId,
			PushPayload payload,
			CancellationToken cancellationToken = default)
		{
			var settings = options.Value;

			if (!settings.IsConfigured)
			{
				logger.LogWarning("Web Push is not configured.");
				return;
			}

			var subscriptions = await dbContext.PushSubscriptions
				.AsNoTracking()
				.Where(s => s.UserId == userId)
				.ToListAsync(cancellationToken);

			if (subscriptions.Count == 0)
				return;

			var json = JsonSerializer.Serialize(payload, JsonOptions);

			var vapid = new VapidDetails(
				settings.Subject,
				settings.PublicKey,
				settings.PrivateKey);

			var requestOptions = new Dictionary<string, object>
			{
				["vapidDetails"] = vapid,
				["TTL"] = TimeToLiveSeconds
			};

			var expired = new ConcurrentBag<Guid>();

			await Parallel.ForEachAsync(
				subscriptions,
				new ParallelOptions
				{
					MaxDegreeOfParallelism = 8,
					CancellationToken = cancellationToken
				},
				async (subscription, token) =>
				{
					try
					{
						var target = new PushSubscription(
							subscription.Endpoint,
							subscription.P256dh,
							subscription.Auth);

						await client.SendNotificationAsync(
							target,
							json,
							requestOptions,
							token);
					}
					catch (WebPushException ex)
						when (ex.StatusCode is HttpStatusCode.Gone
							or HttpStatusCode.NotFound)
					{
						expired.Add(subscription.Id);
					}
					catch (WebPushException ex)
					{
						logger.LogWarning(
							"Push to subscription {SubscriptionId} failed: {Status}",
							subscription.Id,
							ex.StatusCode);
					}
					catch (Exception ex) when (ex is not OperationCanceledException)
					{
						logger.LogWarning(
							ex,
							"Push to subscription {SubscriptionId} failed",
							subscription.Id);
					}
				});

			if (!expired.IsEmpty)
			{
				var expiredIds = expired.ToArray();

				await dbContext.PushSubscriptions
					.Where(s => expiredIds.Contains(s.Id))
					.ExecuteDeleteAsync(cancellationToken);
			}
		}
	}
}