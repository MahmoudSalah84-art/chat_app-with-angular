using chatme.infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace chatme.infrastructure.Persistence.Configurations
{
	public sealed class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
	{
		public void Configure(EntityTypeBuilder<ApplicationUser> builder)
		{
			builder.Property(u => u.Name).HasMaxLength(100).IsRequired();
			builder.Property(u => u.AvatarUrl).HasMaxLength(500);
			builder.Property(u => u.About).HasMaxLength(200);
		}
	}
}
