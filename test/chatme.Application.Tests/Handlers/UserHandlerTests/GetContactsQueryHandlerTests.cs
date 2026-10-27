using chatme.Application.Common.DTO;
using chatme.Application.Common.Interfaces;
using chatme.Application.Features.Users.Queries.GetContacts;
using chatme.Application.Tests.Helpers;
using chatme.Domain.Enums;
using NSubstitute;

namespace chatme.Application.Tests.Handlers.UserHandlerTests
{
	[TestFixture]
	public class GetContactsQueryHandlerTests
	{
		[Test]
		public async Task Not_logged_in_returns_unauthorized()
		{
			var current = Substitute.For<ICurrentUserService>();
			var identity = Substitute.For<IIdentityService>();
			var _sut = new GetContactsQueryHandler(identity, current);
			current.UserId.Returns((Guid?)null);

			var result = await _sut.Handle(new GetContactsQuery(), CancellationToken.None);

			ResultAssert.IsFailure(result, ResultErrorType.Unauthorized);
		}

		[Test]
		public async Task Returns_contacts_excluding_current_user()
		{
			var me = Guid.NewGuid();
			var current = Substitute.For<ICurrentUserService>();
			current.UserId.Returns(me);
			var identity = Substitute.For<IIdentityService>();
			var contacts = new List<UserDto> { TestData.User(), TestData.User() };
			identity.GetContactsAsync(me, Arg.Any<CancellationToken>()).Returns(contacts);
			var _sut = new GetContactsQueryHandler(identity, current);

			var result = await _sut.Handle(new GetContactsQuery(), CancellationToken.None);

			ResultAssert.IsSuccess(result);
			Assert.That(result.Value, Is.EqualTo(contacts));
		}
	}
}