using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using Service.Authorization.Client.Models;
using Service.Authorization.Client.Services;
using Service.Core.Client.Extensions;
using Service.Core.Client.Models;
using Service.Registration.Grpc;
using Service.Registration.Grpc.Models;
using Service.RegistrationApi.Constants;
using Service.RegistrationApi.Models;
using Service.RegistrationApi.Services;
using Service.UserInfo.Crud.Grpc;
using SimpleTrading.ClientApi.Utils;

namespace Service.RegistrationApi.Controllers
{
	[OpenApiTag("Register", Description = "user registration")]
	public class RegisterController : BaseController
	{
		private readonly IRegistrationService _registrationService;
		private readonly ITokenService _tokenService;
		private readonly ILoginRequestValidator _loginRequestValidator;

		public RegisterController(IRegistrationService registrationService,
			ITokenService tokenService,
			ILoginRequestValidator loginRequestValidator,
			IUserInfoService userInfoService) : base(userInfoService)
		{
			_registrationService = registrationService;
			_tokenService = tokenService;
			_loginRequestValidator = loginRequestValidator;
		}

		[HttpPost("create")]
		[SwaggerResponse(HttpStatusCode.OK, typeof (StatusResponse), Description = "Ok")]
		public async ValueTask<IActionResult> RegisterAsync([FromBody] RegisterRequest request)
		{
			int? validationResult = _loginRequestValidator.ValidateRegisterRequest(request);
			if (validationResult != null)
			{
				await WaitFakeRequest();
				return StatusResponse.Error(validationResult.Value);
			}

			Guid? userId = await GetUserIdAsync(request.UserName);
			if (userId != null)
			{
				await WaitFakeRequest();
				return StatusResponse.Error(RegistrationResponseCode.UserAlreadyExists);
			}

			CommonGrpcResponse response = await _registrationService.RegistrationAsync(new RegistrationGrpcRequest
			{
				UserName = request.UserName,
				Password = request.Password,
				FullName = request.FullName
			});

			await WaitFakeRequest();

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
			{
				await WaitFakeRequest();
				return StatusResponse.Error();
			}

			TokenInfo tokenInfo = await _tokenService.GenerateTokensAsync(userName, HttpContext.GetIp());
			return tokenInfo != null
				? DataResponse<TokenInfo>.Ok(tokenInfo)
				: Unauthorized();
		}
	}
}