using chatme.API.Hubs;
using chatme.API.Middleware;
using chatme.API.Services;
using chatme.Application.Common.Interfaces;
using chatme.infrastructure;
using chatme.infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;
using chatme.Application;
var builder = WebApplication.CreateBuilder(args);

// ============================================================
// تسجيل الطبقات: كل طبقة بتسجل نفسها بسطر واحد بفضل الـ
// DependencyInjection.cs اللي عملناه في كل مشروع
// ============================================================
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// تنفيذ الإشعارات اللحظية (SignalR) - موجود هنا بس لأن الـ Api هي الطبقة
// الوحيدة اللي "تعرف" SignalR، والـ Application بيشتغل بالـ Interface بس
builder.Services.AddScoped<IChatNotificationService, SignalRChatNotificationService>();

builder.Services.AddControllers();
builder.Services.AddSignalR();

// ============================================================
// Swagger - مع دعم إدخال الـ JWT Token لتجربة الـ Endpoints المحمية
// ============================================================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
	options.SwaggerDoc("v1", new OpenApiInfo { Title = "WhatsApp Clone API", Version = "v1" });

	options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
	{
		Name = "Authorization",
		Type = SecuritySchemeType.Http,
		Scheme = "Bearer",
		BearerFormat = "JWT",
		In = ParameterLocation.Header,
		Description = "اكتب: Bearer {التوكن اللي رجع من /api/auth/login}",
	});

	//options.AddSecurityRequirement(new OpenApiSecurityRequirement
	//{
	//	{
	//		new OpenApiSecurityScheme
	//		{
	//			Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" },
	//		},
	//		Array.Empty<string>()
	//	},
	//});
});

// ============================================================
// JWT Authentication
// ============================================================
var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
	?? throw new InvalidOperationException("إعدادات Jwt مش موجودة في appsettings.json");

builder.Services
	.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
	.AddJwtBearer(options =>
	{
		options.TokenValidationParameters = new TokenValidationParameters
		{
			ValidateIssuer = true,
			ValidateAudience = true,
			ValidateLifetime = true,
			ValidateIssuerSigningKey = true,
			ValidIssuer = jwtSettings.Issuer,
			ValidAudience = jwtSettings.Audience,
			IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
			ClockSkew = TimeSpan.FromMinutes(2),
		};

		// المتصفح مش بيقدر يحط Authorization Header على اتصال WebSocket،
		// فـ SignalR بيبعت التوكن كـ query string بدل كده. السطور دي بتقول
		// للـ Middleware يدور على التوكن هناك كمان، بس لو الطلب داخل فعلاً
		// على مسار الـ Hub بتاعنا (احتياط أمني - مش أي endpoint تاني)
		options.Events = new JwtBearerEvents
		{
			OnMessageReceived = context =>
			{
				var accessToken = context.Request.Query["access_token"];
				var path = context.HttpContext.Request.Path;

				if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs/chat"))
					context.Token = accessToken;

				return Task.CompletedTask;
			},
		};
	});

builder.Services.AddAuthorization();

// ============================================================
// CORS - عشان Angular (شغال على بورت مختلف وقت التطوير) يقدر يكلم الـ API
// ============================================================
const string AngularClientPolicy = "AngularClient";
builder.Services.AddCors(options =>
{
	options.AddPolicy(AngularClientPolicy, policy =>
	{
		policy
			.WithOrigins("http://localhost:4200")
			.AllowAnyHeader()
			.AllowAnyMethod()
			.AllowCredentials(); // لازم عشان SignalR (WebSocket) يشتغل صح مع الـ CORS
	});
});

var app = builder.Build();

// الـ Middleware ده أول حاجة في الـ Pipeline عشان يقدر يلقط أي خطأ
// يحصل في أي حاجة بعده (حتى لو في الـ Authentication نفسه)
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}
else
{
	app.UseHttpsRedirection();
}

app.UseCors(AngularClientPolicy);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<ChatHub>("/hubs/chat");

app.Run();
