using Autofac;
using Service.RegistrationApi.Services;

namespace Service.RegistrationApi.Modules
{
    public class SettingsModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterInstance(Program.Settings).AsSelf().SingleInstance();
	        builder.RegisterType<LoginRequestValidator>().AsImplementedInterfaces().SingleInstance();
        }
    }
}
