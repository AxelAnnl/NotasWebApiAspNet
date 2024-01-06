using System.ComponentModel.DataAnnotations;

namespace NotasWebApi.Models
{
	public class SendEmailConfirmationViewModel
	{

		[Required(ErrorMessage = "El correo electrónico es requerido.")]
		[EmailAddress(ErrorMessage = "El correo electrónico no es válido.")]
		public string Email { get; set; }
	}
}
