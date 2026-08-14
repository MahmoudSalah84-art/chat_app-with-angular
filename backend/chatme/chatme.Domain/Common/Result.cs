using chatme.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace chatme.Domain.Common
{
	public interface IResult<TSelf> where TSelf : IResult<TSelf>
	{
		static abstract TSelf CreateFailure(ResultErrorType errorType, IReadOnlyList<string> errors);
	}

	public class Result : IResult<Result>
	{
		public bool IsSuccess { get; }
		public bool IsFailure => !IsSuccess;
		public ResultErrorType ErrorType { get; }
		public IReadOnlyList<string> Errors { get; }

		protected Result(bool isSuccess, ResultErrorType errorType, IReadOnlyList<string> errors)
		{
			IsSuccess = isSuccess;
			ErrorType = errorType;
			Errors = errors;
		}

		public static Result Success() => new(true, ResultErrorType.None, []);

		public static Result Failure(string error) => new(false, ResultErrorType.Validation, [error]);
		public static Result Failure(IEnumerable<string> errors) => new(false, ResultErrorType.Validation, errors.ToList());
		public static Result NotFound(string error) => new(false, ResultErrorType.NotFound, [error]);
		public static Result Forbidden(string error) => new(false, ResultErrorType.Forbidden, [error]);
		public static Result Unauthorized(string error) => new(false, ResultErrorType.Unauthorized, [error]);
		public static Result Conflict(string error) => new(false, ResultErrorType.Conflict, [error]);

		static Result IResult<Result>.CreateFailure(ResultErrorType errorType, IReadOnlyList<string> errors) =>
			new(false, errorType, errors);

		/// <summary>
		/// بيحوّل Result فاشل لنوع Result&lt;T&gt; تاني بنفس سبب الفشل بالظبط.
		/// مفيد جدًا لما تنادي عملية بترجع Result وعايز "تمرر" نفس فشلها لدالة
		/// بترجع Result&lt;T&gt; مختلف، من غير ما تعيد كتابة رسالة الخطأ.
		/// مثال: `if (chatResult.IsFailure) return chatResult.ToFailure&lt;ChatDto&gt;();`
		/// </summary>
		public Result<TValue> ToFailure<TValue>()
		{
			if (IsSuccess)
				throw new InvalidOperationException("ToFailure() بيتستخدم بس مع Result فاشل");

			return Result<TValue>.FromError(ErrorType, Errors);
		}
	}

	/// <summary>Result بيحمل قيمة معاه (T) في حالة النجاح بس.</summary>
	public sealed class Result<T> : Result, IResult<Result<T>>
	{
		public T? Value { get; }

		private Result(bool isSuccess, ResultErrorType errorType, IReadOnlyList<string> errors, T? value)
			: base(isSuccess, errorType, errors)
		{
			Value = value;
		}

		public static Result<T> Success(T value) => new(true, ResultErrorType.None, [], value);

		public static new Result<T> Failure(string error) => new(false, ResultErrorType.Validation, [error], default);
		public static new Result<T> Failure(IEnumerable<string> errors) => new(false, ResultErrorType.Validation, errors.ToList(), default);
		public static new Result<T> NotFound(string error) => new(false, ResultErrorType.NotFound, [error], default);
		public static new Result<T> Forbidden(string error) => new(false, ResultErrorType.Forbidden, [error], default);
		public static new Result<T> Unauthorized(string error) => new(false, ResultErrorType.Unauthorized, [error], default);
		public static new Result<T> Conflict(string error) => new(false, ResultErrorType.Conflict, [error], default);

		internal static Result<T> FromError(ResultErrorType errorType, IReadOnlyList<string> errors) =>
			new(false, errorType, errors, default);

		static Result<T> IResult<Result<T>>.CreateFailure(ResultErrorType errorType, IReadOnlyList<string> errors) =>
			new(false, errorType, errors, default);
	}

}
