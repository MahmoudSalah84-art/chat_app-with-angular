using System;
using System.Collections.Generic;
using System.Text;

namespace chatme.Application.Common.DTO
{

	public sealed record UserDto(
		Guid Id, string Name, string Email, string AvatarUrl,
		string? About, string? PhoneNumber, bool IsOnline, DateTime? LastSeenAt);

}
