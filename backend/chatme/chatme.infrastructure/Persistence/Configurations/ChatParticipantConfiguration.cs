using chatme.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace chatme.infrastructure.Persistence.Configurations
{
	public sealed class ChatParticipantConfiguration : IEntityTypeConfiguration<ChatParticipant>
	{
		public void Configure(EntityTypeBuilder<ChatParticipant> builder)
		{
			builder.ToTable("ChatParticipants");
			builder.HasKey(p => p.Id);
			builder.HasIndex(p => new { p.ChatId, p.UserId }).IsUnique();
		}
	}
}
