using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace chatme.infrastructure.Identity
{
	public sealed class ApplicationUser : IdentityUser<Guid>
	{
		public string Name { get; set; } = string.Empty;
		public string AvatarUrl { get; set; } = string.Empty;
		public string? About { get; set; }
		public bool IsOnline { get; set; }
		public DateTime? LastSeenAt { get; set; }
		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
	}

}
