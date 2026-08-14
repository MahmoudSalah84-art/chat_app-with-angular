using chatme.Domain.Common;
using chatme.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace chatme.API.Controllers
{
	[ApiController]
	public abstract class ApiControllerBase : ControllerBase
	{
		protected ActionResult HandleResult(Result result) =>
			result.IsSuccess ? NoContent() : MapError(result);

		protected ActionResult<T> HandleResult<T>(Result<T> result) =>
			result.IsSuccess ? Ok(result.Value) : MapError(result);

		private ActionResult MapError(Result result)
		{
			var payload = new { errors = result.Errors };

			return result.ErrorType switch
			{
				ResultErrorType.NotFound => NotFound(payload),
				ResultErrorType.Forbidden => StatusCode(StatusCodes.Status403Forbidden, payload),
				ResultErrorType.Unauthorized => Unauthorized(payload),
				ResultErrorType.Conflict => Conflict(payload),
				_ => BadRequest(payload), //for others
			};
		}
	}

}
