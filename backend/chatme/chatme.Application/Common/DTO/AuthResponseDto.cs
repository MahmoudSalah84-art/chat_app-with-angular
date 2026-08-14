using System;
using System.Collections.Generic;
using System.Text;

namespace chatme.Application.Common.DTO
{
	public sealed record AuthResponseDto(string Token, UserDto User);

}
