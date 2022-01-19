using System;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using NSwag;
using NSwag.AspNetCore;

namespace Service.RegistrationApi
{
	public static class StartupUtils
	{
		private const string DocumentName = "register";
		private const string ApiName = "RegistrationApi";

		public static void SetupSwagger(this IApplicationBuilder app)
		{
			app.UseOpenApi(settings => settings.Path = "/api/v1/{documentName}/swagger/swagger.json");
			app.UseSwaggerUi3(settings =>
			{
				settings.Path = $"/api/v1/{DocumentName}/swagger";
				settings.SwaggerRoutes.Add(new SwaggerUi3Route("v1", $"/api/v1/{DocumentName}/swagger/swagger.json"));
				settings.DocumentTitle = $"{ApiName} Swagger";
			});
		}

		/// <summary>
		///     Setup swagger ui ba
		/// </summary>
		public static void SetupSwaggerDocumentation(this IServiceCollection services) => services.AddSwaggerDocument(o =>
		{
			o.Title = $"MyJetEducation {ApiName}";
			o.GenerateEnumMappingDescription = true;
			o.DocumentName = DocumentName;
			o.Version = "v1";
			o.AddSecurity("Bearer", Enumerable.Empty<string>(),
				new OpenApiSecurityScheme
				{
					Type = OpenApiSecuritySchemeType.ApiKey,
					Description = "Bearer Token",
					In = OpenApiSecurityApiKeyLocation.Header,
					Name = "Authorization"
				});
		});

		/// <summary>
		///     Headers settings
		/// </summary>
		public static void ConfigurateHeaders(this IServiceCollection services) =>
			services.Configure<ForwardedHeadersOptions>(options => options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto);

		public static void ConfigureJwtBearerOptions(JwtBearerOptions options)
		{
			options.RequireHttpsMetadata = false;
			options.SaveToken = true;

			options.TokenValidationParameters = new TokenValidationParameters
			{
				ValidateIssuerSigningKey = true,
				IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Program.JwtSecret)),
				ValidateIssuer = false,
				ValidateAudience = true,
				ValidAudience = Program.Settings.JwtAudience,
				ValidateLifetime = true,
				LifetimeValidator = (before, expires, token, parameters) => expires != null && expires > DateTime.UtcNow
			};
		}

		public static void ConfigureAuthenticationOptions(AuthenticationOptions options)
		{
			options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
			options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
		}
	}
}