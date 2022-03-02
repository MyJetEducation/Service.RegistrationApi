using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service.Grpc;
using Service.UserInfo.Crud.Grpc;
using Service.UserInfo.Crud.Grpc.Models;

namespace Service.RegistrationApi.Controllers
{
	[ApiController]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	[Route("/api/v1/register")]
	public class BaseController : ControllerBase
	{
		private readonly IGrpcServiceProxy<IUserInfoService> _userInfoService;

		public BaseController(IGrpcServiceProxy<IUserInfoService> userInfoService) => _userInfoService = userInfoService;

		protected static async Task WaitFakeRequest()
		{
			Func<int> timeoutSettings = Program.ReloadedSettings(model => model.FakeRequestTimeoutMilliseconds);

			await Task.Delay(timeoutSettings.Invoke());
		}

		protected async ValueTask<Guid?> GetUserIdAsync(string userName)
		{
			UserInfoResponse userInfoResponse = await _userInfoService.Service.GetUserInfoByLoginAsync(new UserInfoAuthRequest
			{
				UserName = userName
			});

			return userInfoResponse?.UserInfo?.UserId;
		}
	}
}