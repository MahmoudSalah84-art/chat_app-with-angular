using chatme.Application.Common.Interfaces;
using chatme.Domain.Common;
using chatme.Domain.Entities;
using chatme.Domain.Repositories;
using chatme.infrastructure.Identity;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace chatme.infrastructure.Persistence
{
	public sealed class ApplicationDbContext(
	DbContextOptions<ApplicationDbContext> options,
	IPublisher publisher)
	: IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>(options), IApplicationDbContext, IUnitOfWork
	{

		public DbSet<Chat> Chats => Set<Chat>();
		public IQueryable<Message> MessagesReadOnly => Set<Message>();

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			// لازم base الأول - هي المسؤولة عن تعريف جداول Identity كلها
			base.OnModelCreating(modelBuilder);
			modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
		}

		public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
		{
			var domainEvents = ChangeTracker.Entries<AggregateRoot>()
				.Select(e => e.Entity)
				.Where(e => e.DomainEvents.Count > 0)
				.SelectMany(e => e.DomainEvents)
				.ToList();

			var result = await base.SaveChangesAsync(cancellationToken);

			foreach (var domainEvent in domainEvents)
				await publisher.Publish(domainEvent, cancellationToken);

			foreach (var aggregate in ChangeTracker.Entries<AggregateRoot>().Select(e => e.Entity))
				aggregate.ClearDomainEvents();

			return result;
		}
	}
}