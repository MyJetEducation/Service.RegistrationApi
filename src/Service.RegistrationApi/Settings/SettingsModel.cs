using MyJetWallet.Sdk.Service;
using MyYamlParser;

namespace Service.RegistrationApi.Settings
{
	public class SettingsModel
	{
		[YamlProperty("RegistrationApi.SeqServiceUrl")]
		public string SeqServiceUrl { get; set; }

		[YamlProperty("RegistrationApi.ZipkinUrl")]
		public string ZipkinUrl { get; set; }

		[YamlProperty("RegistrationApi.ElkLogs")]
		public LogElkSettings ElkLogs { get; set; }

		[YamlProperty("RegistrationApi.JwtTokenExpireMinutes")]
		public int JwtTokenExpireMinutes { get; set; }

		[YamlProperty("RegistrationApi.RefreshTokenExpireMinutes")]
		public int RefreshTokenExpireMinutes { get; set; }

		[YamlProperty("RegistrationApi.JwtAudience")]
		public string JwtAudience { get; set; }

		[YamlProperty("RegistrationApi.UserInfoCrudServiceUrl")]
		public string UserInfoCrudServiceUrl { get; set; }

		[YamlProperty("RegistrationApi.RegistrationServiceUrl")]
		public string RegistrationServiceUrl { get; set; }

		[YamlProperty("RegistrationApi.PasswordRecoveryServiceUrl")]
		public string PasswordRecoveryServiceUrl { get; set; }

		[YamlProperty("RegistrationApi.FakeRequestTimeoutMilliseconds")]
		public int FakeRequestTimeoutMilliseconds { get; set; }
	}
}