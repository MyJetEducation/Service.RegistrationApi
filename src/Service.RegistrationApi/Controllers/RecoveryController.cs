using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using Service.Authorization.Client.Services;
using Service.Core.Client.Models;
using Service.Grpc;
using Service.PasswordRecovery.Grpc;
using Service.PasswordRecovery.Grpc.Models;
using Service.RegistrationApi.Constants;
using Service.RegistrationApi.Models;
using Service.UserInfo.Crud.Grpc;
using Service.Web;

namespace Service.RegistrationApi.Controllers
{
	[OpenApiTag("Recovery", Description = "user password recovery")]
	public class RecoveryController : BaseController
	{
		private readonly IGrpcServiceProxy<IPasswordRecoveryService> _passwordRecoveryService;

		public RecoveryController(
			IGrpcServiceProxy<IPasswordRecoveryService> passwordRecoveryService,
			IGrpcServiceProxy<IUserInfoService> userInfoService) : base(userInfoService) =>
				_passwordRecoveryService = passwordRecoveryService;

		[HttpPost("recovery")]
		[SwaggerResponse(HttpStatusCode.OK, typeof (StatusResponse), Description = "Ok")]
		public async ValueTask<IActionResult> PasswordRecoveryAsync([FromBody, Required] string email)
		{
			Guid? userId = await GetUserIdAsync(email);
			if (userId == null)
			{
				await WaitFakeRequest();
				return StatusResponse.Ok();
			}

			CommonGrpcResponse response = await _passwordRecoveryService.TryCall(service => service.Recovery(new RecoveryPasswordGrpcRequest
			{
				Email = email
			}));

			await WaitFakeRequest();

			return StatusResponse.Result(response);
		}

		[HttpPost("change")]
		[SwaggerResponse(HttpStatusCode.OK, typeof (StatusResponse), Description = "Ok")]
		public async ValueTask<IActionResult> ChangePasswordAsync([FromBody, Required] ChangePasswordRequest request)
		{
			string password = request.Password;

			if (!UserDataRequestValidator.ValidatePassword(password))
			{
				await WaitFakeRequest();
				return StatusResponse.Error(RegistrationResponseCode.NotValidPassword);
			}

			CommonGrpcResponse response = await _passwordRecoveryService.TryCall(service => service.Change(new ChangePasswordGrpcRequest
			{
				Password = password, 
				Hash = request.Hash
			}));

			return StatusResponse.Result(response);
		}
	}
}