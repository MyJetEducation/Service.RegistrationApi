using System;
using System.Net;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MyJetWallet.Sdk.Service;
using MySettingsReader;
using Service.RegistrationApi.Settings;
using Service.Core.Domain.Extensions;

namespace Service.RegistrationApi
{
	public class Program
	{
		private const string JwtSecretName = "JWT_SECRET";
		private const string SettingsFileName = ".myjeteducation";

		public static SettingsModel Settings { get; private set; }

		public static ILoggerFactory LogFactory { get; private set; }

		public static string JwtSecret { get; private set; }

		public static void Main(string[] args)
		{
			Console.Title = "MyJetEducation Service.RegistrationApi";
			LoadJwtSecret();
			Settings = LoadSettings();

			using ILoggerFactory loggerFactory = LogConfigurator.ConfigureElk("MyJetEducation", Settings.SeqServiceUrl, Settings.ElkLogs);
			ILogger<Program> logger = loggerFactory.CreateLogger<Program>();
			LogFactory = loggerFactory;

			try
			{
				logger.LogInformation("Application is being started");

				CreateHostBuilder(loggerFactory, args).Build().Run();

				logger.LogInformation("Application has been stopped");
			}
			catch (Exception ex)
			{
				logger.LogCritical(ex, "Application has been terminated unexpectedly");
			}
		}

		private static void LoadJwtSecret()
		{
			JwtSecret = Environment.GetEnvironmentVariable(JwtSecretName);

			void ShowError(string message)
			{
				Console.WriteLine(message);
				throw new Exception(message);
			}

			if (JwtSecret.IsNullOrWhiteSpace())
				ShowError($"ERROR! Please configure environment variable: {JwtSecretName}!");

			else if (JwtSecret.Length <= 15)
				ShowError($"ERROR! Length of environment variable {JwtSecretName} must be greater or equal than 16 symbols!");
		}

		private static SettingsModel LoadSettings() => SettingsReader.GetSettings<SettingsModel>(SettingsFileName);

		public static Func<T> ReloadedSettings<T>(Func<SettingsModel, T> getter) => () => getter.Invoke(LoadSettings());

		public static IHostBuilder CreateHostBuilder(ILoggerFactory loggerFactory, string[] args) =>
			Host.CreateDefaultBuilder(args)
				.UseServiceProviderFactory(new AutofacServiceProviderFactory())
				.ConfigureWebHostDefaults(webBuilder =>
				{
					string httpPort = Environment.GetEnvironmentVariable("HTTP_PORT") ?? "8080";
					string grpcPort = Environment.GetEnvironmentVariable("GRPC_PORT") ?? "80";

					Console.WriteLine($"HTTP PORT: {httpPort}");
					Console.WriteLine($"GRPC PORT: {grpcPort}");

					webBuilder.ConfigureKestrel(options =>
					{
						options.Listen(IPAddress.Any, int.Parse(httpPort), o => o.Protocols = HttpProtocols.Http1);
						options.Listen(IPAddress.Any, int.Parse(grpcPort), o => o.Protocols = HttpProtocols.Http2);
					});

					webBuilder.UseStartup<Startup>();
				})
				.ConfigureServices(services =>
				{
					services.AddSingleton(loggerFactory);
					services.AddSingleton(typeof (ILogger<>), typeof (Logger<>));
				});
	}
}