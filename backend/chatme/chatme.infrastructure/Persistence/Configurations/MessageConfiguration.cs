using chatme.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace chatme.infrastructure.Persistence.Configurations
{
	public sealed class MessageConfiguration : IEntityTypeConfiguration<Message>
	{
		public void Configure(EntityTypeBuilder<Message> builder)
		{
			builder.ToTable("Messages");
			builder.HasKey(m => m.Id);
			builder.Property(m => m.Content).HasMaxLength(4000);
			builder.HasIndex(m => m.ChatId);
			builder.HasIndex(m => new { m.ChatId, m.SentAt });
		}
	}
}
