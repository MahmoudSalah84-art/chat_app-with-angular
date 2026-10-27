using chatme.Application.Common.DTO;
using chatme.Application.Common.Interfaces;
using chatme.Application.Features.Auth.Commands.Login;
using chatme.Application.Features.Auth.Commands.Register;
using chatme.Application.Tests.Helpers;
using chatme.Domain.Common;
using chatme.Domain.Enums;
using NSubstitute;

namespace chatme.Application.Tests.Handlers
{
	[TestFixture]
	public class LoginUserCommandHandlerTests
	{
		[Test]
		public async Task Delegates_to_identity_service_and_returns_its_result()
		{
			var identity = Substitute.For<IIdentityService>(); // mock the identity service
			var expected = Result<AuthResponseDto>.Success(TestData.Auth());
			identity.LoginAsync("a@b.com", "pw", Arg.Any<CancellationToken>()).Returns(expected);

			var result = await new LoginUserCommandHandler(identity).Handle(new LoginUserCommand("a@b.com", "pw"), CancellationToken.None);

			Assert.That(result, Is.SameAs(expected));
		}

		[Test]
		public async Task Wrong_credentials_failure_is_passed_through()
		{
			var identity = Substitute.For<IIdentityService>();
			identity.LoginAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
				.Returns(Result<AuthResponseDto>.Unauthorized("wrong"));

			var result = await new LoginUserCommandHandler(identity).Handle(new LoginUserCommand("a@b.com", "bad"), CancellationToken.None);

			ResultAssert.IsFailure(result, ResultErrorType.Unauthorized);
		}
	}

}