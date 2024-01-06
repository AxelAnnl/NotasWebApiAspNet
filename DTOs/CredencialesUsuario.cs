using System.ComponentModel.DataAnnotations;
namespace NotasWebApi.DTOs
{
	public class CredencialesUsuario
	{
		[EmailAddress]
		[Required]
		public string Email { get; set; }
		[Required]
		[DataType(DataType.Password)]
		public string Password { get; set; }
	}
}
