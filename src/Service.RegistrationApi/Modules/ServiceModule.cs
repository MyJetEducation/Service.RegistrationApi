using Autofac;
using Microsoft.Extensions.Logging;
using Service.Authorization.Client.Services;
using Service.Core.Client.Services;
using Service.Grpc;
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
			builder.RegisterUserInfoCrudClient(Program.Settings.UserInfoCrudServiceUrl, Program.LogFactory.CreateLogger(typeof(UserInfoCrudClientFactory)));
			builder.RegisterRegistrationClient(Program.Settings.RegistrationServiceUrl, Program.LogFactory.CreateLogger(typeof(RegistrationClientFactory)));
			builder.RegisterPasswordRecoveryClient(Program.Settings.PasswordRecoveryServiceUrl, Program.LogFactory.CreateLogger(typeof(PasswordRecoveryClientFactory)));

			builder.RegisterType<SystemClock>().AsImplementedInterfaces().SingleInstance();

			builder.Register(context => new EncoderDecoder(Program.EncodingKey))
				.As<IEncoderDecoder>()
				.SingleInstance();

			builder.Register(context =>
				new TokenService(
					context.Resolve<IGrpcServiceProxy<IUserInfoService>>(),
					context.Resolve<IEncoderDecoder>(),
					context.Resolve<ILogger<TokenService>>(),
					context.Resolve<ISystemClock>(),
					Program.Settings.JwtAudience,
					Program.JwtSecret,
					Program.Settings.JwtTokenExpireMinutes,
					Program.Settings.RefreshTokenExpireMinutes
					))
				.As<ITokenService>()
				.SingleInstance();
		}
	}
}