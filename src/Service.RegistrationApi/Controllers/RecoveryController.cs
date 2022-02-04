using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using Service.Core.Client.Models;
using Service.PasswordRecovery.Grpc;
using Service.PasswordRecovery.Grpc.Models;
using Service.RegistrationApi.Models;
using Service.RegistrationApi.Services;
using Service.UserInfo.Crud.Grpc;

namespace Service.RegistrationApi.Controllers
{
	[OpenApiTag("Recovery", Description = "user password recovery")]
	public class RecoveryController : BaseController
	{
		private readonly IPasswordRecoveryService _passwordRecoveryService;
		private readonly ILoginRequestValidator _loginRequestValidator;

		public RecoveryController(
			IPasswordRecoveryService passwordRecoveryService,
			ILoginRequestValidator loginRequestValidator,
			IUserInfoService userInfoService) : base(userInfoService)
		{
			_passwordRecoveryService = passwordRecoveryService;
			_loginRequestValidator = loginRequestValidator;
		}

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

			CommonGrpcResponse response = await _passwordRecoveryService.Recovery(new RecoveryPasswordGrpcRequest {Email = email});

			await WaitFakeRequest();

			return Result(response?.IsSuccess);
		}

		[HttpPost("change")]
		[SwaggerResponse(HttpStatusCode.OK, typeof (StatusResponse), Description = "Ok")]
		public async ValueTask<IActionResult> ChangePasswordAsync([FromBody, Required] ChangePasswordRequest request)
		{
			string hash = request.Hash;
			string password = request.Password;

			int? validationResult = _loginRequestValidator.ValidatePassword(password);
			if (validationResult != null)
			{
				await WaitFakeRequest();
				return StatusResponse.Error(validationResult.Value);
			}

			CommonGrpcResponse response = await _passwordRecoveryService.Change(new ChangePasswordGrpcRequest {Password = password, Hash = hash});

			return Result(response?.IsSuccess);
		}
	}
}