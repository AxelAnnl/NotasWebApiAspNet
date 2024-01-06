using System.ComponentModel.DataAnnotations;

namespace NotasWebApi.Models
{
	public class ResetPasswordRequest
	{
		[Required]
		[EmailAddress]
		public string Email { get; set; }
	}
}
