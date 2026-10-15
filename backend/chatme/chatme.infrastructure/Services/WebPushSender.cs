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

		private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
		{
			// for allow Arabic characters in payload without escaping like \u0627\u0644\u0639\u0631\u0628\u064a\u0629
			Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
		};

		public async Task SendAsync(IReadOnlyCollection<Guid> userIds, PushPayload payload, CancellationToken cancellationToken = default)
		{
			var settings = options.Value;
			if (!settings.IsConfigured)
			{
				logger.LogWarning("Web Push is not configured (WebPush:Subject / PublicKey / PrivateKey). Skipping.");
				return;
			}

			var ids = userIds.ToArray();
			var subscriptions = await dbContext.PushSubscriptions
				.AsNoTracking()
				.Where(s => ids.Contains(s.UserId))
				.ToListAsync(cancellationToken);

			if (subscriptions.Count == 0)
				return;

			var json = JsonSerializer.Serialize(payload, JsonOptions);
			var vapid = new VapidDetails(settings.Subject, settings.PublicKey, settings.PrivateKey);
			var requestOptions = new Dictionary<string, object>
			{
				["vapidDetails"] = vapid,
				["TTL"] = TimeToLiveSeconds,
			};

			var expired = new ConcurrentBag<Guid>();

			await Parallel.ForEachAsync(
				subscriptions,
				new ParallelOptions { MaxDegreeOfParallelism = 8, CancellationToken = cancellationToken },
				async (subscription, token) =>
				{
					try
					{
						var target = new PushSubscription(subscription.Endpoint, subscription.P256dh, subscription.Auth);
						await client.SendNotificationAsync(target, json, requestOptions, token);
					}
					catch (WebPushException ex) when (ex.StatusCode is HttpStatusCode.Gone or HttpStatusCode.NotFound)
					{
						// user canceled the subscription or the subscription is no longer valid
						expired.Add(subscription.Id);
					}
					catch (WebPushException ex)
					{
						logger.LogWarning("Push to subscription {SubscriptionId} failed: {Status}", subscription.Id, ex.StatusCode);
					}
					catch (Exception ex) when (ex is not OperationCanceledException)
					{
						logger.LogWarning(ex, "Push to subscription {SubscriptionId} failed", subscription.Id);
					}
				}
			);

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