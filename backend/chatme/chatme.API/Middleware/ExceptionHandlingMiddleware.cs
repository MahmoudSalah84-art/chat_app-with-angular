using chatme.Domain.Exceptions;
using System.ComponentModel.DataAnnotations;
using System.Net;

namespace chatme.API.Middleware
{
	public sealed class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
	{
		public async Task InvokeAsync(HttpContext context)
		{
			try
			{
				await next(context);
			}
			catch (Exception exception)
			{
				logger.LogError(exception, "حصل استثناء غير متوقع أثناء معالجة الطلب");

				context.Response.ContentType = "application/json";
				context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

				await context.Response.WriteAsJsonAsync(new
				{
					errors = new[] { "حصل خطأ غير متوقع من السيرفر، حاول تاني" },
				});
			}
		}
	}
}
