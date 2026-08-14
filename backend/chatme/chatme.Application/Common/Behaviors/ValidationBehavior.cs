using chatme.Domain.Common;
using chatme.Domain.Enums;
using FluentValidation;
using MediatR;

namespace chatme.Application.Common.Behaviors
{
	public sealed class ValidationBehavior<TRequest, TResponse>(
	IEnumerable<IValidator<TRequest>> validators) : IPipelineBehavior<TRequest, TResponse>
	where TRequest : IRequest<TResponse>
	where TResponse : IResult<TResponse>
	{
		public async Task<TResponse> Handle(
			TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
		{
			if (!validators.Any())
				return await next();

			var context = new ValidationContext<TRequest>(request);

			var failures = (await Task.WhenAll(
					validators.Select(v => v.ValidateAsync(context, cancellationToken))))
				.SelectMany(result => result.Errors)
				.Where(failure => failure is not null)
				.Select(failure => failure.ErrorMessage)
				.ToList();

			if (failures.Count > 0)
				return TResponse.CreateFailure(ResultErrorType.Validation, failures);

			return await next();
		}
	}
}
