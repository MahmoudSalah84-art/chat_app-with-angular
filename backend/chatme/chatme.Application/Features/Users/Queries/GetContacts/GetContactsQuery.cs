using chatme.Application.Common;
using chatme.Application.Common.DTO;
using chatme.Domain.Common;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace chatme.Application.Features.Users.Queries.GetContacts
{
	public sealed record GetContactsQuery : IRequest<Result<List<UserDto>>>;

}
