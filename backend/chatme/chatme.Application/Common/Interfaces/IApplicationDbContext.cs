using chatme.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace chatme.Application.Common.Interfaces
{
	public interface IApplicationDbContext
	{
		DbSet<Chat> Chats { get; }
		IQueryable<Message> MessagesReadOnly { get; }

		Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
	}

}
