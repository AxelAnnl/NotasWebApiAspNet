using System.ComponentModel.DataAnnotations;

namespace NotasWebApi.Models
{
	public class ConfirmEmailViewModel
	{
		[Required(ErrorMessage = "El correo electrónico es requerido.")]
		[EmailAddress(ErrorMessage = "El correo electrónico no es válido.")]
		public string Email { get; set; }

		[Required(ErrorMessage = "El token es requerido.")]
		public string Token { get; set; }
	}

}
