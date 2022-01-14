using System.ComponentModel.DataAnnotations;

namespace Service.RegistrationApi.Models
{
	public class ChangePasswordRequest
	{
		[Required]
		public string Hash { get; set; }

		[Required]
		public string Password { get; set; }
	}
}