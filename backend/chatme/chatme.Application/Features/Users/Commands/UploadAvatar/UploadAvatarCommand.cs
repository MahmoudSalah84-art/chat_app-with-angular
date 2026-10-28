using chatme.Application.Common.DTO;
using chatme.Domain.Common;
using MediatR;

namespace chatme.Application.Features.Users.Commands.UploadAvatar
{
	public sealed record UploadAvatarCommand(
	Stream Content,
	string FileName,
	long Size
	) : IRequest<Result<UserDto>>;

}
