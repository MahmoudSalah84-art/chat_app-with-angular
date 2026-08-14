using System;
using System.Collections.Generic;
using System.Text;

namespace chatme.Domain.Enums
{
	public enum ResultErrorType
	{
		None,
		Validation,
		NotFound,
		Forbidden,
		Unauthorized,
		Conflict,
	}
}
