using System;
using System.Collections.Generic;
using System.Text;

namespace chatme.Application.Common.DTO
{
	public sealed record ReactionDto(Guid UserId, string Emoji);

}
