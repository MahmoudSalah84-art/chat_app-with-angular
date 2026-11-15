using chatme.Application.Common.DTO;
using chatme.Application.Common.Interfaces;
using chatme.Domain.Common;
using MediatR;

namespace chatme.Application.Features.Auth.Commands.RefreshToken
{
	public sealed class RefreshTokenCommandHandler(IIdentityService identityService)
		: IRequestHandler<RefreshTokenCommand, Result<AuthResponseDto>>
	{
		public Task<Result<AuthResponseDto>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken) =>
			identityService.RefreshTokenAsync(request.RefreshToken, cancellationToken);
	}
}
