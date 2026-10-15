using chatme.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace chatme.infrastructure.Persistence.Configurations
{
	public sealed class WebPushSubscriptionConfiguration : IEntityTypeConfiguration<WebPushSubscription>
	{
		public void Configure(EntityTypeBuilder<WebPushSubscription> builder)
		{
			builder.ToTable("WebPushSubscriptions");
			builder.HasKey(s => s.Id);
			builder.Property(s => s.Id).ValueGeneratedNever();

			builder.Property(s => s.Endpoint).HasMaxLength(2048).IsUnicode(false).IsRequired();
			builder.Property(s => s.EndpointHash).HasMaxLength(64).IsUnicode(false).IsFixedLength().IsRequired();
			builder.Property(s => s.P256dh).HasMaxLength(256).IsUnicode(false).IsRequired();
			builder.Property(s => s.Auth).HasMaxLength(64).IsUnicode(false).IsRequired();
			builder.Property(s => s.UserAgent).HasMaxLength(300);

			builder.HasIndex(s => s.EndpointHash).IsUnique();
			builder.HasIndex(s => s.UserId);
		}
	}
}