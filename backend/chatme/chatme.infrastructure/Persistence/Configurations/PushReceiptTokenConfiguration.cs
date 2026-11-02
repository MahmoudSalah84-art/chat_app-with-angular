using chatme.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace chatme.infrastructure.Persistence.Configurations
{
	public sealed class PushReceiptTokenConfiguration
	: IEntityTypeConfiguration<PushReceiptToken>
	{
		public void Configure(EntityTypeBuilder<PushReceiptToken> builder)
		{
			builder.HasKey(x => x.Id);

			builder.Property(x => x.TokenHash)
				.HasMaxLength(64)
				.IsRequired();

			builder.HasIndex(x => x.TokenHash)
				.IsUnique();

			builder.HasIndex(x => new
			{
				x.ReceiverId,
				x.ChatId,
				x.MessageId
			});
		}
	}
}
