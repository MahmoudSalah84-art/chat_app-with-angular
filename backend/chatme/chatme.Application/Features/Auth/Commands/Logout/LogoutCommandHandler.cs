using chatme.Application.Common.Interfaces;
using chatme.Domain.Common;
using MediatR;

namespace chatme.Application.Features.Auth.Commands.Logout
{
	public sealed class LogoutCommandHandler(IIdentityService identityService)
		: IRequestHandler<LogoutCommand, Result>
	{
		public Task<Result> Handle(LogoutCommand request, CancellationToken cancellationToken) =>
			identityService.LogoutAsync(request.RefreshToken, cancellationToken);
	}
}