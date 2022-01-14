using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using Service.Authorization.Domain.Models;
using Service.Core.Domain.Extensions;
using Service.Core.Grpc.Models;
using Service.PasswordRecovery.Grpc;
using Service.PasswordRecovery.Grpc.Models;
using Service.Registration.Grpc;
using Service.Registration.Grpc.Models;
using Service.RegistrationApi.Constants;
using Service.RegistrationApi.Models;
using Service.RegistrationApi.Services;
using Service.UserInfo.Crud.Grpc;
using Service.UserInfo.Crud.Grpc.Models;
using SimpleTrading.ClientApi.Utils;

namespace Service.RegistrationApi.Controllers
{
	[ApiController]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	[OpenApiTag("Register", Description = "user registration")]
	[Route("/api/v1/register")]
	public class RegisterController : ControllerBase
	{
		private readonly IPasswordRecoveryService _passwordRecoveryService;
		private readonly IRegistrationService _registrationService;
		private readonly ITokenService _tokenService;
		private readonly ILoginRequestValidator _loginRequestValidator;
		private readonly IUserInfoService _userInfoService;

		public RegisterController(
			IPasswordRecoveryService passwordRecoveryService,
			IRegistrationService registrationService,
			ITokenService tokenService,
			ILoginRequestValidator loginRequestValidator,
			IUserInfoService userInfoService)
		{
			_passwordRecoveryService = passwordRecoveryService;
			_registrationService = registrationService;
			_tokenService = tokenService;
			_loginRequestValidator = loginRequestValidator;
			_userInfoService = userInfoService;
		}

		[HttpPost("create")]
		[SwaggerResponse(HttpStatusCode.OK, typeof (StatusResponse), Description = "Ok")]
		public async ValueTask<IActionResult> RegisterAsync([FromBody] RegisterRequest request)
		{
			int? validationResult = _loginRequestValidator.ValidateRegisterRequest(request);
			if (validationResult != null)
			{
				WaitFakeRequest();
				return StatusResponse.Error(validationResult.Value);
			}

			Guid? userId = await GetUserIdAsync(request.UserName);
			if (userId != null)
				return StatusResponse.Error(RegistrationResponseCode.UserAlreadyExists);

			CommonGrpcResponse response = await _registrationService.RegistrationAsync(new RegistrationGrpcRequest
			{
				UserName = request.UserName,
				Password = request.Password,
				FullName = request.FullName
			});

			return Result(response?.IsSuccess);
		}

		[HttpPost("confirm")]
		[SwaggerResponse(HttpStatusCode.OK, typeof (DataResponse<TokenInfo>), Description = "Ok")]
		[SwaggerResponse(HttpStatusCode.Unauthorized, null, Description = "Unauthorized")]
		public async ValueTask<IActionResult> ConfirmRegisterAsync([FromBody, Required] string hash)
		{
			ConfirmRegistrationGrpcResponse response = await _registrationService.ConfirmRegistrationAsync(new ConfirmRegistrationGrpcRequest {Hash = hash});

			string userName = response?.Email;
			if (userName.IsNullOrEmpty())
				return StatusResponse.Error();

			TokenInfo tokenInfo = await _tokenService.GenerateTokensAsync(userName, HttpContext.GetIp());
			return tokenInfo != null
				? DataResponse<TokenInfo>.Ok(tokenInfo)
				: Unauthorized();
		}

		[HttpPost("recovery")]
		[SwaggerResponse(HttpStatusCode.OK, typeof (StatusResponse), Description = "Ok")]
		public async ValueTask<IActionResult> PasswordRecoveryAsync([FromBody, Required] string email)
		{
			CommonGrpcResponse response = await _passwordRecoveryService.Recovery(new RecoveryPasswordGrpcRequest {Email = email});

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
				WaitFakeRequest();
				return StatusResponse.Error(validationResult.Value);
			}

			CommonGrpcResponse response = await _passwordRecoveryService.Change(new ChangePasswordGrpcRequest {Password = password, Hash = hash});

			return Result(response?.IsSuccess);
		}

		private static void WaitFakeRequest() => Thread.Sleep(200);

		private static IActionResult Result(bool? isSuccess) => isSuccess == true ? StatusResponse.Ok() : StatusResponse.Error();

		private async ValueTask<Guid?> GetUserIdAsync(string userName)
		{
			UserInfoResponse userInfoResponse = await _userInfoService.GetUserInfoByLoginAsync(new UserInfoAuthRequest
			{
				UserName = userName
			});

			return userInfoResponse?.UserInfo?.UserId;
		}
	}
}