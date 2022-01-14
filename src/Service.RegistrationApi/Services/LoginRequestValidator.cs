using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Service.RegistrationApi.Constants;
using Service.RegistrationApi.Models;

namespace Service.RegistrationApi.Services
{
	public class LoginRequestValidator : ILoginRequestValidator
	{
		private static readonly Regex PasswordRegex = new Regex("^(?=.*[A-Za-z])(?=.*\\d)[A-Za-z\\d]{8,31}$", RegexOptions.Compiled);
		private static readonly Regex FullNameRegex = new Regex("^([A-Za-z]+)\\ ([A-Za-z]+)$", RegexOptions.Compiled);

		public int? ValidateLoginRequest(RegisterRequest request) => ValidateLogin(request.UserName) ?? ValidatePassword(request.Password);

		public int? ValidateRegisterRequest(RegisterRequest request) => ValidateLoginRequest(request) ?? ValidateFullName(request.FullName);

		public int? ValidatePassword(string value) => !PasswordRegex.IsMatch(value) ? (int?) RegistrationResponseCode.NotValidPassword : null;

		private static int? ValidateLogin(string value) => !new EmailAddressAttribute().IsValid(value) ? (int?) RegistrationResponseCode.NotValidEmail : null;

		private static int? ValidateFullName(string value) => !FullNameRegex.IsMatch(value) ? (int?) RegistrationResponseCode.NotValidFullName : null;
	}
}