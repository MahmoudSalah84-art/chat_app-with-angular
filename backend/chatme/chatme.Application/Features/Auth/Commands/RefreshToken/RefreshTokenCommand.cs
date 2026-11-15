using chatme.Application.Common.DTO;
using chatme.Domain.Common;
using MediatR;

namespace chatme.Application.Features.Auth.Commands.RefreshToken
{
	public sealed record RefreshTokenCommand(string RefreshToken) : IRequest<Result<AuthResponseDto>>;
}
