using chatme.Domain.Common;
using chatme.Domain.Enums;

namespace chatme.Application.Tests.Helpers
{
	internal static class ResultAssert
	{
		public static void IsSuccess(Result result) =>
		Assert.That(result.IsSuccess, Is.True,
			$"Expected success but got {result.ErrorType}: {string.Join(" | ", result.Errors)}");


		public static void IsFailure(Result result, ResultErrorType expected) =>
			Assert.Multiple(() =>
			{
				Assert.That(result.IsFailure, Is.True, "Expected failure but result succeeded");
				Assert.That(result.ErrorType, Is.EqualTo(expected));
			});
	}
}
