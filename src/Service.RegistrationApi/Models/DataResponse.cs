using Microsoft.AspNetCore.Mvc;
using Service.Core.Client.Constants;

namespace Service.RegistrationApi.Models
{
	public class DataResponse<T> : StatusResponse
	{
		public T Data { get; set; }

		public static IActionResult Ok(T data) => new OkObjectResult(
			new DataResponse<T>
			{
				Data = data,
				Status = data == null ? ResponseCode.Fail : ResponseCode.Ok
			});
	}
}