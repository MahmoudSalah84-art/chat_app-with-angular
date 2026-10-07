using chatme.Application.Common.Interfaces;
using chatme.Domain.Common;
using chatme.Domain.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace chatme.Application.Features.Messages.Commands.MarkAllMessagesDelivered
{
	public sealed class MarkAllMessagesDeliveredCommandHandler(
		IApplicationDbContext dbContext,
		IUnitOfWork unitOfWork,
		ICurrentUserService currentUserService) : IRequestHandler<MarkAllMessagesDeliveredCommand, Result>
	{
		public async Task<Result> Handle(MarkAllMessagesDeliveredCommand request, CancellationToken cancellationToken)
		{
			var userId = currentUserService.UserId;
			if (userId is null)
				return Result.Unauthorized("لازم تسجل دخول الأول");

			var chats = await dbContext.Chats
				.Include(c => c.Participants)
				.Include(c => c.Messages)
				.Where(c => c.Participants.Any(p => p.UserId == userId))
				.ToListAsync(cancellationToken);

			foreach (var chat in chats)
				chat.MarkAllAsDelivered(userId.Value);

			await unitOfWork.SaveChangesAsync(cancellationToken);
			return Result.Success();
		}
	}
}
