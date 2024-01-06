using System.ComponentModel.DataAnnotations;

namespace NotasWebApi.Models
{
	public class ResetPasswordViewModel
	{
		[Required]
		public string Token { get; set; }
		[Required]
		public string NewPassword { get; set; }

		[Required]
		[EmailAddress]
		public string Email { get; set; }
	}
}
