using chatme.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace chatme.infrastructure.Services.RefreshTocken
{
	public sealed class RefreshTokenCleanupWorker(
		IServiceScopeFactory scopeFactory,
		ILogger<RefreshTokenCleanupWorker> logger) : BackgroundService
	{
		private static readonly TimeSpan Interval = TimeSpan.FromHours(6);

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			using var timer = new PeriodicTimer(Interval);

			do
			{
				try
				{
					using var scope = scopeFactory.CreateScope();
					var dbContext = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

					var now = DateTime.UtcNow;
					var deleted = await dbContext.RefreshTokens
						.Where(t => t.ExpiresAtUtc < now)
						.ExecuteDeleteAsync(stoppingToken);

					if (deleted > 0)
						logger.LogInformation("Deleted {Count} expired refresh tokens", deleted);
				}
				catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
				{
					break;
				}
				catch (Exception ex)
				{
					logger.LogError(ex, "Refresh token cleanup failed");
				}
			}
			while (await WaitForNextTickAsync(timer, stoppingToken));
		}

		private static async Task<bool> WaitForNextTickAsync(PeriodicTimer timer, CancellationToken cancellationToken)
		{
			try
			{
				return await timer.WaitForNextTickAsync(cancellationToken);
			}
			catch (OperationCanceledException)
			{
				return false;
			}
		}
	}
}