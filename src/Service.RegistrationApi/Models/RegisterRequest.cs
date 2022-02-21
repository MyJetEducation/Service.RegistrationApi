using System.ComponentModel.DataAnnotations;

namespace Service.RegistrationApi.Models
{
	public class RegisterRequest
	{
		[Required]
		public string UserName { get; set; }

		[Required]
		public string Password { get; set; }

		[Required]
		public string FirstName { get; set; }

		[Required]
		public string LastName { get; set; }
	}
}