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
using Service.Grpc;
using Service.Registration.Grpc;
using Service.Registration.Grpc.Models;
using Service.RegistrationApi.Constants;
using Service.RegistrationApi.Models;
using Service.UserInfo.Crud.Grpc;
using Service.Web;
using SimpleTrading.ClientApi.Utils;

namespace Service.RegistrationApi.Controllers
{
	[OpenApiTag("Register", Description = "user registration")]
	public class RegisterController : BaseController
	{
		private readonly IGrpcServiceProxy<IRegistrationService> _registrationService;
		private readonly ITokenService _tokenService;

		public RegisterController(IGrpcServiceProxy<IRegistrationService> registrationService,
			ITokenService tokenService,
			IGrpcServiceProxy<IUserInfoService> userInfoService) : base(userInfoService)
		{
			_registrationService = registrationService;
			_tokenService = tokenService;
		}

		[HttpPost("create")]
		[SwaggerResponse(HttpStatusCode.OK, typeof (StatusResponse), Description = "Ok")]
		public async ValueTask<IActionResult> RegisterAsync([FromBody] RegisterRequest request)
		{
			int? validationResult = ValidateRegisterRequest(request);
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

			CommonGrpcResponse response = await _registrationService.TryCall(service => service.RegistrationAsync(new RegistrationGrpcRequest
			{
				UserName = request.UserName,
				Password = request.Password,
				FirstName = request.FirstName.Trim(),
				LastName = request.LastName.Trim()
			}));

			await WaitFakeRequest();

			return StatusResponse.Result(response);
		}

		[HttpPost("confirm")]
		[SwaggerResponse(HttpStatusCode.OK, typeof (DataResponse<TokenInfo>), Description = "Ok")]
		[SwaggerResponse(HttpStatusCode.Unauthorized, null, Description = "Unauthorized")]
		public async ValueTask<IActionResult> ConfirmRegisterAsync([FromBody, Required] string hash)
		{
			ConfirmRegistrationGrpcResponse response = await _registrationService.TryCall(service => service.ConfirmRegistrationAsync(new ConfirmRegistrationGrpcRequest
			{
				Hash = hash
			}));

			string userName = response?.Email;
			if (userName.IsNullOrEmpty())
			{
				await WaitFakeRequest();
				return Unauthorized();
			}

			TokenInfo tokenInfo = await _tokenService.GenerateTokensAsync(userName, HttpContext.GetIp());
			return tokenInfo != null
				? DataResponse<TokenInfo>.Ok(tokenInfo)
				: Unauthorized();
		}

		private static int? ValidateRegisterRequest(RegisterRequest request)
		{
			if (!UserDataRequestValidator.ValidateLogin(request.UserName))
				return RegistrationResponseCode.NotValidEmail;

			if (!UserDataRequestValidator.ValidatePassword(request.Password))
				return RegistrationResponseCode.NotValidPassword;

			if (!UserDataRequestValidator.ValidateName(request.FirstName) || !UserDataRequestValidator.ValidateName(request.LastName))
				return RegistrationResponseCode.NotValidFullName;

			return null;
		}
	}
}