using chatme.Application.Features.Push;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace chatme.infrastructure.Services
{
	public sealed class PushDispatchWorker(
		PushJobQueue queue,
		IServiceScopeFactory scopeFactory,
		ILogger<PushDispatchWorker> logger) : BackgroundService
	{
		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			try
			{
				await Parallel.ForEachAsync(
					queue.ReadAllAsync(stoppingToken),
					new ParallelOptions { MaxDegreeOfParallelism = 4, CancellationToken = stoppingToken },
					async (job, cancellationToken) =>
					{
						try
						{
							using var scope = scopeFactory.CreateScope();
							var notifier = scope.ServiceProvider.GetRequiredService<MessagePushNotifier>();
							await notifier.NotifyAsync(job, cancellationToken);
						}
						catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
						{
							throw;
						}
						catch (Exception ex)
						{
							logger.LogError(ex, "Push dispatch failed for message {MessageId}", job.MessageId);
						}
					}
				);
			}
			catch (OperationCanceledException)
			{
				// shutdown عادي
			}
		}
	}
}