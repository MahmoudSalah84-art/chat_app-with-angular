using chatme.Application.Common.Interfaces;
using chatme.Domain.Common;
using chatme.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace chatme.Application.Features.Push.Commands.SubscribePush
{
	public sealed class SubscribePushCommandHandler(
		IApplicationDbContext dbContext,
		ICurrentUserService currentUserService) : IRequestHandler<SubscribePushCommand, Result>
	{
		private const int MaxSubscriptionsPerUser = 10;

		public async Task<Result> Handle(SubscribePushCommand request, CancellationToken cancellationToken)
		{
			var userId = currentUserService.UserId;
			if (userId is null)
				return Result.Unauthorized("لازم تسجل دخول الأول");

			var hash = WebPushSubscription.HashEndpoint(request.Endpoint);
			var existing = await dbContext.PushSubscriptions
				.FirstOrDefaultAsync(s => s.EndpointHash == hash, cancellationToken);

			if (existing is null)
				dbContext.PushSubscriptions.Add(
					WebPushSubscription.Create(userId.Value, request.Endpoint, request.P256dh, request.Auth, request.UserAgent));
			else
				existing.Refresh(userId.Value, request.P256dh, request.Auth, request.UserAgent);

			await dbContext.SaveChangesAsync(cancellationToken);

			// نخلي آخر 10 أجهزة بس لكل مستخدم
			var stale = await dbContext.PushSubscriptions
				.Where(s => s.UserId == userId.Value)
				.OrderByDescending(s => s.LastSeenAt)
				.Skip(MaxSubscriptionsPerUser)
				.ToListAsync(cancellationToken);

			if (stale.Count > 0)
			{
				dbContext.PushSubscriptions.RemoveRange(stale);
				await dbContext.SaveChangesAsync(cancellationToken);
			}

			return Result.Success();
		}
	}
}