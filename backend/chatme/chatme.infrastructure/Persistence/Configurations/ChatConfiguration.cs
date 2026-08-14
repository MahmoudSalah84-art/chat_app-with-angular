using chatme.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace chatme.infrastructure.Persistence.Configurations
{
	public sealed class ChatConfiguration : IEntityTypeConfiguration<Chat>
	{
		public void Configure(EntityTypeBuilder<Chat> builder)
		{
			builder.ToTable("Chats");
			builder.HasKey(c => c.Id);

			builder.Property(c => c.Name).HasMaxLength(100);
			builder.Property(c => c.AvatarUrl).HasMaxLength(500);

			builder.HasMany(c => c.Participants).WithOne().HasForeignKey(p => p.ChatId).OnDelete(DeleteBehavior.Cascade);
			builder.HasMany(c => c.Messages).WithOne().HasForeignKey(m => m.ChatId).OnDelete(DeleteBehavior.Cascade);

			builder.Navigation(c => c.Participants).UsePropertyAccessMode(PropertyAccessMode.Field);
			builder.Navigation(c => c.Messages).UsePropertyAccessMode(PropertyAccessMode.Field);

			builder.Ignore(c => c.DomainEvents);
		}
	}
}
