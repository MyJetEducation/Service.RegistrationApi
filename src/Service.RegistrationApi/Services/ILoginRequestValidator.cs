using Service.RegistrationApi.Models;

namespace Service.RegistrationApi.Services
{
	public interface ILoginRequestValidator
	{
		int? ValidateRegisterRequest(RegisterRequest request);

		int? ValidatePassword(string value);
	}
}