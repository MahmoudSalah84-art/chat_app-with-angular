using System;
using System.Collections.Generic;
using System.Text;

namespace chatme.Domain.Repositories
{
	public interface IUnitOfWork
	{
		Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
	}
}
