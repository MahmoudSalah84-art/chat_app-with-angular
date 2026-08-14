using System;
using System.Collections.Generic;
using System.Text;

namespace chatme.Application.Common.DTO
{

	public sealed record ChatDto(
		Guid Id, bool IsGroup, string Name, string AvatarUrl,
		List<UserDto> Participants, MessageDto? LastMessage, int UnreadCount);


}
