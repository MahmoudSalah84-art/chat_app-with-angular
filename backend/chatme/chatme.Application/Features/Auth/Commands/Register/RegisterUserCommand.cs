
using chatme.Application.Common;
using chatme.Application.Common.DTO;
using chatme.Domain.Common;
using MediatR;

namespace chatme.Application.Features.Auth.Commands.Register
{
	public sealed record RegisterUserCommand(string Name, string Email, string Password) : IRequest<Result<AuthResponseDto>>;


}
