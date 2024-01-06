using System.ComponentModel.DataAnnotations;

namespace NotasWebApi.DTOs
{
	public class CredencialesUsuarioLoginExterno
	{
		[EmailAddress]
		[Required]
		public string Email { get; set; }
	}
}
