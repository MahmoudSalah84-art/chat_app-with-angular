using chatme.Application.Common;
using chatme.Application.Common.Interfaces;
using chatme.Domain.Common;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace chatme.Application.Features.Users.Commands.SetOnlineStatus
{
	public sealed class SetOnlineStatusCommandHandler(
		IIdentityService identityService,
		ICurrentUserService currentUserService) : IRequestHandler<SetOnlineStatusCommand, Result>
	{
		public Task<Result> Handle(SetOnlineStatusCommand request, CancellationToken cancellationToken)
		{
			var userId = currentUserService.UserId;
			// مش بنرجّع Failure هنا عمدًا: لو حصلت مشكلة بسيطة في قراءة هوية
			// المستخدم وقت قفل اتصال SignalR، مفيش داعي نوقف أي حاجة أو نظهر
			// خطأ للمستخدم - العملية دي مجرد تحديث حالة "متصل الآن" مش حرجة
			if (userId is null)
				return Task.FromResult(Result.Success());

			return identityService.SetOnlineStatusAsync(userId.Value, request.IsOnline, cancellationToken);
		}
	}

}
