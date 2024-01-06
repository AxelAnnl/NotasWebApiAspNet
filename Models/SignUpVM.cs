using System.ComponentModel.DataAnnotations;

namespace NotasWeb.Models
{
	public class SignUpVM
	{
		[Required]
		[EmailAddress]
		public string Email { get; set; }
		[Required]
		[DataType(DataType.Password)]
		public string Password { get; set; }
	}
}
