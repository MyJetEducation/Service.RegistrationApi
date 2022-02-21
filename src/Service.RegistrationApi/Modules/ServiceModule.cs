using Autofac;
using Microsoft.Extensions.Logging;
using Service.Authorization.Client.Services;
using Service.PasswordRecovery.Client;
using Service.Registration.Client;
using Service.UserInfo.Crud.Client;
using Service.UserInfo.Crud.Grpc;

namespace Service.RegistrationApi.Modules
{
	public class ServiceModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterUserInfoCrudClient(Program.Settings.UserInfoCrudServiceUrl);
			
			builder.RegisterRegistrationClient(Program.Settings.RegistrationServiceUrl, Program.LogFactory.CreateLogger(typeof(RegistrationClientFactory)));
			builder.RegisterPasswordRecoveryClient(Program.Settings.PasswordRecoveryServiceUrl, Program.LogFactory.CreateLogger(typeof(PasswordRecoveryClientFactory)));

			builder.Register(context =>
				new TokenService(
					context.Resolve<IUserInfoService>(),
					Program.Settings.JwtAudience,
					Program.JwtSecret,
					Program.Settings.JwtTokenExpireMinutes,
					Program.Settings.RefreshTokenExpireMinutes,
					context.Resolve<ILogger<TokenService>>()))
				.As<ITokenService>()
				.SingleInstance();
		}
	}
}