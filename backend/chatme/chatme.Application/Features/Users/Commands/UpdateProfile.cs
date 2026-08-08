namespace chatme.Application.Features.Users.Commands
{
	public sealed record UpdateProfileCommand(
	string? Name,
	string? About,
	string? AvatarUrl) : IRequest<UserDto>;

}
