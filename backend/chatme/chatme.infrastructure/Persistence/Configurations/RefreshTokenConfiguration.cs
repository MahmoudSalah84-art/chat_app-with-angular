using chatme.Domain.Entities;
using chatme.infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace chatme.infrastructure.Persistence.Configurations
{
	public sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
	{
		public void Configure(EntityTypeBuilder<RefreshToken> builder)
		{
			builder.HasKey(x => x.Id);

			builder.Property(x => x.TokenHash)
				.HasMaxLength(64)
				.IsRequired();

			builder.HasIndex(x => x.TokenHash).IsUnique();

			builder.HasIndex(x => x.FamilyId);
			builder.HasIndex(x => x.UserId);

			builder.HasIndex(x => x.ExpiresAtUtc);

			builder.HasOne<ApplicationUser>()
				.WithMany()
				.HasForeignKey(x => x.UserId)
				.OnDelete(DeleteBehavior.Cascade);
		}
	}
}