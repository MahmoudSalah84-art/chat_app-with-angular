using chatme.Application.Common.Interfaces;
using chatme.Domain.Common;
using chatme.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace chatme.Application.Features.Push.Commands.UnsubscribePush
{
	public sealed class UnsubscribePushCommandHandler(
		IApplicationDbContext dbContext,
		ICurrentUserService currentUserService) : IRequestHandler<UnsubscribePushCommand, Result>
	{
		public async Task<Result> Handle(UnsubscribePushCommand request, CancellationToken cancellationToken)
		{
			var userId = currentUserService.UserId;
			if (userId is null)
				return Result.Unauthorized("لازم تسجل دخول الأول");

			var hash = WebPushSubscription.HashEndpoint(request.Endpoint);

			await dbContext.PushSubscriptions
				.Where(s => s.EndpointHash == hash && s.UserId == userId.Value)
				.ExecuteDeleteAsync(cancellationToken);

			return Result.Success();
		}
	}
}