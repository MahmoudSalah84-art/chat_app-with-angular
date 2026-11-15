using chatme.Domain.Common;
using MediatR;

namespace chatme.Application.Features.Auth.Commands.Logout
{
	public sealed record LogoutCommand(string RefreshToken) : IRequest<Result>;
}