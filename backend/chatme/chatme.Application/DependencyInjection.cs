using chatme.Application.Common.Behaviors;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace chatme.Application
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddApplication(this IServiceCollection services)
		{
			services.AddMediatR(cfg =>
			{
				cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);
				cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
			});

			services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

			return services;
		}
	}
}
