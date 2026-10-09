using chatme.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace chatme.infrastructure.Persistence.Configurations
{
	public sealed class MessageReactionConfiguration : IEntityTypeConfiguration<MessageReaction>
	{
		public void Configure(EntityTypeBuilder<MessageReaction> builder)
		{
			builder.ToTable("MessageReactions");
			builder.HasKey(r => r.Id);
			builder.Property(r => r.Id).ValueGeneratedNever();

			builder.Property(r => r.Emoji).HasMaxLength(16).IsRequired();

			builder.HasIndex(r => new { r.MessageId, r.UserId }).IsUnique();
		}
	}
}