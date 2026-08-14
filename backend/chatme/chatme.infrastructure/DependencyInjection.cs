using chatme.Application.Common.Interfaces;
using chatme.Domain.Repositories;
using chatme.infrastructure.Identity;
using chatme.infrastructure.Persistence;
using chatme.infrastructure.Persistence.Repositories;
using chatme.infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace chatme.infrastructure
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
		{
			services.AddDbContext<ApplicationDbContext>(options =>
				options.UseSqlServer(
					configuration.GetConnectionString("DefaultConnection"),
					sqlOptions => sqlOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

			services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());
			services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<ApplicationDbContext>());

			services.AddScoped<IChatRepository, ChatRepository>();

			// ============================================================
			// ASP.NET Core Identity - بيوفر UserManager, PasswordHasher,
			// قواعد تعقيد كلمة السر، وكل بنية التخزين الآمنة تلقائيًا
			// ============================================================
			services
				.AddIdentityCore<ApplicationUser>(options =>
				{
					// مطابقين نفس القواعد اللي كانت في الـ Frontend (6 حروف بس)
					// عشان تجربة المستخدم متتغيرش، وتقدر تشددها براحتك بعدين
					options.Password.RequiredLength = 6;
					options.Password.RequireDigit = false;
					options.Password.RequireUppercase = false;
					options.Password.RequireLowercase = false;
					options.Password.RequireNonAlphanumeric = false;
					options.User.RequireUniqueEmail = true;
				})
				.AddRoles<IdentityRole<Guid>>()
				.AddEntityFrameworkStores<ApplicationDbContext>()
				.AddDefaultTokenProviders();

			services.AddScoped<IIdentityService, IdentityService>();

			services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
			services.AddScoped<IJwtTokenService, JwtTokenService>();

			services.AddHttpContextAccessor();
			services.AddScoped<ICurrentUserService, CurrentUserService>();

			return services;
		}
	}
}
