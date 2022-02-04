using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service.RegistrationApi.Models;
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
		private readonly IUserInfoService _userInfoService;

		public BaseController(IUserInfoService userInfoService) => _userInfoService = userInfoService;

		protected static async Task WaitFakeRequest()
		{
			Func<int> timeoutSettings = Program.ReloadedSettings(model => model.FakeRequestTimeoutMilliseconds);

			await Task.Delay(timeoutSettings.Invoke());
		}

		protected static IActionResult Result(bool? isSuccess) => isSuccess == true ? StatusResponse.Ok() : StatusResponse.Error();

		protected async ValueTask<Guid?> GetUserIdAsync(string userName)
		{
			UserInfoResponse userInfoResponse = await _userInfoService.GetUserInfoByLoginAsync(new UserInfoAuthRequest
			{
				UserName = userName
			});

			return userInfoResponse?.UserInfo?.UserId;
		}
	}
}