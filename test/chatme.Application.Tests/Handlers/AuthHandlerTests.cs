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


	[TestFixture]
	public class RegisterUserCommandHandlerTests
	{
		[Test]
		public async Task Delegates_all_fields_to_identity_service()
		{
			var identity = Substitute.For<IIdentityService>();
			identity.RegisterAsync("mahmoud", "a@b.com", "123456", Arg.Any<CancellationToken>())
				.Returns(Result<AuthResponseDto>.Success(TestData.Auth()));

			var result = await new RegisterUserCommandHandler(identity)
				.Handle(new RegisterUserCommand("mahmoud", "a@b.com", "123456"), CancellationToken.None);

			ResultAssert.IsSuccess(result);
			await identity.Received(1).RegisterAsync("mahmoud", "a@b.com", "123456", Arg.Any<CancellationToken>());
		}

		[Test]
		public async Task Duplicate_email_conflict_is_passed_through()
		{
			var identity = Substitute.For<IIdentityService>();
			identity.RegisterAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
				.Returns(Result<AuthResponseDto>.Conflict("exists"));

			var result = await new RegisterUserCommandHandler(identity)
				.Handle(new RegisterUserCommand("mahmoud", "a@b.com", "123456"), CancellationToken.None);

			ResultAssert.IsFailure(result, ResultErrorType.Conflict);
		}
	}
}