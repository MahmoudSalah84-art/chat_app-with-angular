using chatme.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

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

			builder.OwnsOne(m => m.Attachment, a =>
			{
				a.Property(x => x.Url).HasMaxLength(500).HasColumnName("AttachmentUrl");
				a.Property(x => x.PublicId).HasMaxLength(300).HasColumnName("AttachmentPublicId");
				a.Property(x => x.FileName).HasMaxLength(255).HasColumnName("AttachmentFileName");
				a.Property(x => x.ContentType).HasMaxLength(100).HasColumnName("AttachmentContentType");
				a.Property(x => x.SizeInBytes).HasColumnName("AttachmentSize");
			});

			builder.HasMany(m => m.Reactions).WithOne().HasForeignKey(r => r.MessageId).OnDelete(DeleteBehavior.Cascade);
			builder.Navigation(m => m.Reactions).UsePropertyAccessMode(PropertyAccessMode.Field);
		}
	}
}