using chatme.Application.Common.DTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace chatme.Application.Common.Interfaces
{

	public interface IJwtTokenService
	{
		string GenerateToken(UserDto user);
	}
}
